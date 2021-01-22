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

                public uint PlayerIndex { get; private set; }

                public ClientConnection(uint playerIndex)
                {
                    State = new client.ClientState();
                    State.SetPlayerID(playerIndex);
                    PlayerIndex = playerIndex;
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

                public GameCreationState(udp.server.UDPServer server,  GameObject playerPrefab, common.StandardMovementSettings movementSettings, int snapshotDelay, string physicsScene) : base(server)
                {
                    m_server.AddReceiver(this);
                    m_clientConnections = new Dictionary<IPEndPoint, ClientConnection>();

                    m_movementSettings = movementSettings;
                    m_snapshotDelay = snapshotDelay;
                    m_physicsScene = physicsScene;
                    m_playerPrefab = playerPrefab;
                }

                public override ServerState Update()
                {
                    int i = 0; // temp
                    lock (m_lock)
                    {
                        if (i == 1 || m_clientConnections.Count > 2)
                        {
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
                        m_clientConnections[clientIP] = new ClientConnection((uint)m_clientConnections.Count);
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
                private Dictionary<uint, Rigidbody2D> m_bodies;
                
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
                    m_server.AddReceiver(this);
                    m_tickAccumulator = 0;
                    m_clientConnections = clientConnections;

                    m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(physicsScene).GetPhysicsScene2D();
                    m_playerPrefab = playerPrefab;

                    m_bodies = new Dictionary<uint, Rigidbody2D>();
                    m_clientInputs = new Dictionary<ClientConnection, common.data.InputMessage>();

                    foreach(IPEndPoint ip in m_clientConnections.Keys)
                    {
                        uint id = m_clientConnections[ip].PlayerIndex;
                        Rigidbody2D body = GameObject.Instantiate(playerPrefab).GetComponent<Rigidbody2D>();
                        m_bodies.Add(id, body);

                        for (int j = 0; j < m_clientConnections.Count; j++)
                        {
                            m_clientConnections[ip].State.AddPlayer(new common.data.PlayerState());
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
                                    Rigidbody2D body = m_bodies[client.PlayerIndex];

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
