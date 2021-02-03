using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;

namespace ubv
{
    namespace server
    {
        namespace logic
        {
            abstract public class ServerState
            {
                protected udp.server.UDPServer m_server;
                protected readonly object m_lock = new object();

                public ServerState(udp.server.UDPServer server)
                {
                    m_server = server;
                }

                public abstract ServerState Update();
                public abstract ServerState FixedUpdate();
            }


            public class ClientConnection
            {
                public uint ServerTick;
                public client.ClientState State;

                public int PlayerGUID { get; private set; }

                public ClientConnection(int playerGUID)
                {
                    State = new client.ClientState();
                    State.SetPlayerID(playerGUID);
                    PlayerGUID = playerGUID;
                }
            }

            /// <summary>
            /// Represents the state of the server before the game.
            /// In charge of regrouping player parties, and launching 
            /// the game with a fixed number of players
            /// </summary>
            public class GameCreationState : ServerState, udp.server.IServerReceiver
            {
                private Dictionary<IPEndPoint, ClientConnection> m_clientConnections;

                // transfer to gameplay
                private string m_physicsScene;
                private common.StandardMovementSettings m_movementSettings;
                private int m_snapshotDelay;
                private GameObject m_playerPrefab;

                private List<common.data.PlayerState> m_players;

#if NETWORK_SIMULATE
                private bool m_forceStartGame;
#endif // NETWORK_SIMULATE

                public GameCreationState(udp.server.UDPServer server, 
                    GameObject playerPrefab, 
                    common.StandardMovementSettings 
                    movementSettings, 
                    int snapshotDelay, 
                    string physicsScene
#if NETWORK_SIMULATE
                    , ServerUpdate parent
#endif // NETWORK_SIMULATE 
                    ) : base(server)
                {

                    m_players = new List<common.data.PlayerState>();
                    m_clientConnections = new Dictionary<IPEndPoint, ClientConnection>();

                    m_movementSettings = movementSettings;
                    m_snapshotDelay = snapshotDelay;
                    m_physicsScene = physicsScene;
                    m_playerPrefab = playerPrefab;

                    m_forceStartGame = false;

#if NETWORK_SIMULATE
                    parent.ForceStartGameButtonEvent.AddListener(() => { m_forceStartGame = true; });
#endif // NETWORK_SIMULATE 

                    m_server.Subscribe(this);
                }

                public override ServerState Update()
                {
                    lock (m_lock)
                    {
                        if (m_clientConnections.Count > 3
#if NETWORK_SIMULATE
                            || m_forceStartGame
#endif // NETWORK_SIMULATE
                        )
                        {
                            m_server.Unsubscribe(this);

                            common.data.GameStartMessage message = new common.data.GameStartMessage();
                            message.Players.Set(m_players);
                            foreach (IPEndPoint ip in m_clientConnections.Keys)
                            {
                                m_server.Send(message.GetBytes(), ip);
                            }

                            return new GameplayState(m_server, m_playerPrefab, m_clientConnections, m_movementSettings, m_snapshotDelay, m_physicsScene);
                        }
                    }
                    return this;
                }

                public override ServerState FixedUpdate()
                {
                    return this;
                }
                
                public void Receive(UDPToolkit.Packet packet, IPEndPoint clientIP)
                {
                    //throw new System.NotImplementedException();
                }

                public void OnConnect(IPEndPoint clientIP)
                {
                    lock (m_lock)
                    {
                        int playerID = System.Guid.NewGuid().GetHashCode();

                        // TODO get rid of client connection data and only use serializable list of int after serialize rework
                        m_clientConnections[clientIP] = new ClientConnection(playerID);

                        common.data.IdentificationMessage idMessage = new common.data.IdentificationMessage();
                        idMessage.PlayerID.Set(playerID);

                        common.data.PlayerState playerState = new common.data.PlayerState();
                        playerState.GUID.Set(playerID);

                        // set rotation / position according to existing players?

                        m_players.Add(playerState);

                        m_server.Send(idMessage.GetBytes(), clientIP);

                        Debug.Log("Received connection request from " + clientIP.ToString());
                    }
                }

                public void OnDisconnect(IPEndPoint clientIP)
                {
                    //throw new System.NotImplementedException();
                    lock (m_lock)
                    {
                        m_clientConnections.Remove(clientIP);
                    }
                }
            }

            /// <summary>
            /// Represents the state of the server during the game
            /// </summary>
            public class GameplayState : ServerState, udp.server.IServerReceiver 
            {
                private Dictionary<IPEndPoint, ClientConnection> m_clientConnections;
                private Dictionary<ClientConnection, common.data.InputMessage> m_clientInputs;
                private Dictionary<int, Rigidbody2D> m_bodies;
                
                private common.StandardMovementSettings m_movementSettings;
                private readonly int m_snapshotDelay;
                
                private uint m_tickAccumulator;

                private PhysicsScene2D m_serverPhysics;

                private GameObject m_playerPrefab;

                public GameplayState(udp.server.UDPServer server, 
                    GameObject playerPrefab, Dictionary<IPEndPoint, 
                    ClientConnection> clientConnections, 
                    common.StandardMovementSettings movementSettings, 
                    int snapshotDelay, 
                    string physicsScene) : base(server)
                {
                    m_server.Subscribe(this);
                    m_tickAccumulator = 0;
                    m_clientConnections = clientConnections;

                    m_movementSettings = movementSettings;

                    m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(physicsScene).GetPhysicsScene2D();
                    m_playerPrefab = playerPrefab;

                    m_bodies = new Dictionary<int, Rigidbody2D>();
                    m_clientInputs = new Dictionary<ClientConnection, common.data.InputMessage>();

                    // instantiate each player
                    foreach(IPEndPoint ip in m_clientConnections.Keys)
                    {
                        int id = m_clientConnections[ip].PlayerGUID;
                        Rigidbody2D body = GameObject.Instantiate(playerPrefab).GetComponent<Rigidbody2D>();
                        body.position = m_bodies.Count * Vector2.left * 3;
                        body.name = "Server player " + id.ToString();
                        m_bodies.Add(id, body);
                    }

                    // add each player to client states
                    foreach (IPEndPoint ip in m_clientConnections.Keys)
                    {
                        common.data.PlayerState player = new common.data.PlayerState();
                        player.GUID.Set(m_clientConnections[ip].PlayerGUID);
                        player.Position.Set(m_bodies[m_clientConnections[ip].PlayerGUID].position);

                        m_clientConnections[ip].State.AddPlayer(player);
                        m_clientConnections[ip].State.SetPlayerID(player.GUID);
                    }

                    // add each player to each other client state
                    foreach (ClientConnection baseConn in m_clientConnections.Values)
                    {
                        foreach (ClientConnection conn in m_clientConnections.Values)
                        {
                            common.data.PlayerState currentPlayer = conn.State.GetPlayer();
                            if (baseConn.PlayerGUID != conn.PlayerGUID)
                            {
                                baseConn.State.AddPlayer(currentPlayer);
                            }
                        }
                    }
                }

                public override ServerState Update()
                {
                    return this;
                }

                public override ServerState FixedUpdate()
                {
                    // for each player
                    // check if missing frames
                    // update frames

                    uint framesToSimulate = 0;
                    lock (m_lock)
                    {
                        foreach (ClientConnection client in m_clientInputs.Keys)
                        {
                            common.data.InputMessage message = m_clientInputs[client];
                            int messageCount = message.InputFrames.Value.Count;
                            uint maxTick = message.StartTick + (uint)(messageCount - 1);
#if DEBUG_LOG
            Debug.Log("max tick to simulate = " + maxTick.ToString());
#endif // DEBUG_LOG

                            // on recule jusqu'à ce qu'on trouve le  tick serveur le plus récent
                            uint missingFrames = (maxTick > client.ServerTick) ? maxTick - client.ServerTick : 0;

                            if (framesToSimulate < missingFrames) framesToSimulate = missingFrames;
                        }

                        for (uint f = framesToSimulate; f > 0; f--)
                        {
                            foreach (ClientConnection client in m_clientInputs.Keys)
                            {
                                common.data.InputMessage message = m_clientInputs[client];
                                int messageCount = message.InputFrames.Value.Count;
                                if (messageCount > f)
                                {
                                    common.data.InputFrame frame = message.InputFrames.Value[messageCount - (int)f - 1];

                                    // must be called in main unity thread
                                    Rigidbody2D body = m_bodies[client.PlayerGUID];

                                    common.logic.PlayerMovement.Execute(ref body, m_movementSettings, frame, Time.fixedDeltaTime);

                                    client.State.GetPlayer().Position.Set(body.position);
                                    client.State.Tick.Set(client.ServerTick);
                                    client.ServerTick++;
                                }
                            }

                            m_serverPhysics.Simulate(Time.fixedDeltaTime);
                        }

                        m_clientInputs.Clear();
                    }

                    if (++m_tickAccumulator > m_snapshotDelay)
                    {
                        m_tickAccumulator = 0;
                        foreach (IPEndPoint ip in m_clientConnections.Keys)
                        {
                            m_server.Send(m_clientConnections[ip].State.GetBytes(), ip);
                        }
                    }
                    return this;
                }

                public void Receive(udp.UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
                {
                    common.data.InputMessage inputs = udp.Serializable.FromBytes<common.data.InputMessage>(packet.Data);
                    if (inputs != null && m_clientConnections.ContainsKey(clientEndPoint))
                    {
                        lock (m_lock)
                        {
                            m_clientInputs[m_clientConnections[clientEndPoint]] = inputs;
                        }
                    }
                }

                public void OnConnect(IPEndPoint clientIP)
                {
                    //throw new System.NotImplementedException();
                }
                
                public void OnDisconnect(IPEndPoint clientIP)
                {
                    //m_IPConnections.Remove(clientIP);
                }
            }
        }
    }
}
