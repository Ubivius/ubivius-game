using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;

namespace ubv.server.logic
{
    abstract public class ServerState
    {
<<<<<<< HEAD
        protected udp.server.UDPServer m_server;
        protected readonly object m_lock = new object();

        public ServerState(udp.server.UDPServer server)
        {
            m_server = server;
        }

        public abstract ServerState Update();
        public abstract ServerState FixedUpdate();
    }
=======
        namespace logic
        {
            abstract public class ServerState
            {
                protected readonly object m_lock = new object();
                
                public abstract ServerState Update();
                public abstract ServerState FixedUpdate();
            }
>>>>>>> origin/master


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

<<<<<<< HEAD
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
=======
            /// <summary>
            /// Represents the state of the server before the game.
            /// In charge of regrouping player parties, and launching 
            /// the game with a fixed number of players
            /// </summary>
            public class GameCreationState : ServerState, tcp.server.ITCPServerReceiver, udp.server.IUDPServerReceiver
            {
                private tcp.server.TCPServer m_TCPServer;
                private Dictionary<IPEndPoint, ClientConnection> m_TCPClientConnections;
                private Dictionary<IPEndPoint, ClientConnection> m_UDPClientConnections;

                // transfer to gameplay
                private readonly string m_physicsScene;
                private readonly udp.server.UDPServer m_UDPserver;
                private readonly common.StandardMovementSettings m_movementSettings;
                private readonly int m_snapshotDelay;
                private readonly GameObject m_playerPrefab;
>>>>>>> origin/master

        private List<common.data.PlayerState> m_players;

#if NETWORK_SIMULATE
        private bool m_forceStartGame;
#endif // NETWORK_SIMULATE

<<<<<<< HEAD
        public GameCreationState(udp.server.UDPServer server, 
            GameObject playerPrefab, 
            common.StandardMovementSettings 
            movementSettings, 
            int snapshotDelay, 
            string physicsScene
=======
                public GameCreationState(udp.server.UDPServer UDPServer, 
                    tcp.server.TCPServer TCPServer,
                    GameObject playerPrefab, 
                    common.StandardMovementSettings 
                    movementSettings, 
                    int snapshotDelay, 
                    string physicsScene
>>>>>>> origin/master
#if NETWORK_SIMULATE
            , ServerUpdate parent
#endif // NETWORK_SIMULATE 
<<<<<<< HEAD
            ) : base(server)
        {

            m_players = new List<common.data.PlayerState>();
            m_clientConnections = new Dictionary<IPEndPoint, ClientConnection>();
=======
                    )
                {
                    m_UDPserver = UDPServer;
                    m_TCPServer = TCPServer;
                    m_players = new List<common.data.PlayerState>();
                    m_TCPClientConnections = new Dictionary<IPEndPoint, ClientConnection>();
                    m_UDPClientConnections = new Dictionary<IPEndPoint, ClientConnection>();
>>>>>>> origin/master

            m_movementSettings = movementSettings;
            m_snapshotDelay = snapshotDelay;
            m_physicsScene = physicsScene;
            m_playerPrefab = playerPrefab;

            m_forceStartGame = false;

#if NETWORK_SIMULATE
            parent.ForceStartGameButtonEvent.AddListener(() => { m_forceStartGame = true; });
#endif // NETWORK_SIMULATE 
<<<<<<< HEAD

            m_server.AddReceiver(this);
        }

        public override ServerState Update()
        {
            lock (m_lock)
            {
                if (m_clientConnections.Count > 3
=======
                    
                    m_TCPServer.Subscribe(this);
                }

                public override ServerState Update()
                {
                    lock (m_lock)
                    {
                        if (m_TCPClientConnections.Count > 3
>>>>>>> origin/master
#if NETWORK_SIMULATE
                    || m_forceStartGame
#endif // NETWORK_SIMULATE
<<<<<<< HEAD
                )
                {
                    common.data.GameStartMessage message = new common.data.GameStartMessage();
                    message.Players.Set(m_players);
                    foreach (IPEndPoint ip in m_clientConnections.Keys)
                    {
                        m_server.Send(message.GetBytes(), ip);
=======
                        )
                        {
                            m_TCPServer.Unsubscribe(this);
                            m_UDPserver.Unsubscribe(this);

                            common.data.GameStartMessage message = new common.data.GameStartMessage();
                            message.Players.Set(m_players);

                            foreach (IPEndPoint ip in m_TCPClientConnections.Keys)
                            {
                                m_TCPServer.Send(message.GetBytes(), ip);
                            }

                            return new GameplayState(m_UDPserver, m_playerPrefab, m_UDPClientConnections, m_movementSettings, m_snapshotDelay, m_physicsScene);
                        }
>>>>>>> origin/master
                    }

<<<<<<< HEAD
                    return new GameplayState(m_server, m_playerPrefab, m_clientConnections, m_movementSettings, m_snapshotDelay, m_physicsScene);
=======
                public override ServerState FixedUpdate()
                {
                    return this;
                }
                
                public void ReceivePacket(TCPToolkit.Packet packet, IPEndPoint clientIP)
                {
                    return;
>>>>>>> origin/master
                }
            }
            return this;
        }

<<<<<<< HEAD
        public override ServerState FixedUpdate()
        {
            return this;
        }
                
        public void Receive(UDPToolkit.Packet packet, IPEndPoint clientIP)
        {
            //throw new System.NotImplementedException();
        }
=======
                public void OnConnect(IPEndPoint clientIP)
                {
                    lock (m_lock)
                    {
                        int playerID = System.Guid.NewGuid().GetHashCode();

                        // TODO get rid of client connection data and only use serializable list of int after serialize rework
                        m_TCPClientConnections[clientIP] = new ClientConnection(playerID);
>>>>>>> origin/master

        public void OnConnect(IPEndPoint clientIP)
        {
            lock (m_lock)
            {
                int playerID = System.Guid.NewGuid().GetHashCode();
                m_clientConnections[clientIP] = new ClientConnection(playerID);

                common.data.IdentificationMessage idMessage = new common.data.IdentificationMessage();
                idMessage.PlayerID.Set(playerID);

                common.data.PlayerState playerState = new common.data.PlayerState();
                playerState.GUID.Set(playerID);

<<<<<<< HEAD
                // set rotation / position according to existing players?

                m_players.Add(playerState);

                m_server.Send(idMessage.GetBytes(), clientIP);

                Debug.Log("Received connection request from " + clientIP.ToString());
=======
                        m_players.Add(playerState);
                        m_UDPserver.Subscribe(this);
                        m_UDPserver.RegisterClient(clientIP.Address);

                        m_TCPServer.Send(idMessage.GetBytes(), clientIP);

#if DEBUG_LOG
                        Debug.Log("Received connection request from " + clientIP.ToString());
#endif // DEBUG_LOG
                    }
                }

                public void OnDisconnect(IPEndPoint clientIP)
                {
                    lock (m_lock)
                    {
                        m_TCPClientConnections.Remove(clientIP);
                    }
                }

                public void Receive(UDPToolkit.Packet packet, IPEndPoint clientIP)
                {
                    if (m_UDPClientConnections.ContainsKey(clientIP))
                    {
#if DEBUG_LOG
                        Debug.Log("Client " + clientIP.ToString() + " already connected. Ignoring.");
#endif // DEBUG_LOG
                        return;
                    }

                    common.data.IdentificationMessage identification = udp.Serializable.FromBytes<common.data.IdentificationMessage>(packet.Data);
                    if (identification != null)
                    {
                        m_UDPClientConnections[clientIP] = new ClientConnection(identification.PlayerID);
                    }
                }
>>>>>>> origin/master
            }
        }

<<<<<<< HEAD
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
=======
            /// <summary>
            /// Represents the state of the server during the game
            /// </summary>
            public class GameplayState : ServerState, udp.server.IUDPServerReceiver
            {
                private udp.server.UDPServer m_UDPserver;

                private Dictionary<IPEndPoint, ClientConnection> m_UDPClientConnections;
                private Dictionary<ClientConnection, common.data.InputMessage> m_clientInputs;
                private Dictionary<int, Rigidbody2D> m_bodies;
>>>>>>> origin/master
                
        private common.StandardMovementSettings m_movementSettings;
        private readonly int m_snapshotDelay;
                
        private uint m_tickAccumulator;

        private PhysicsScene2D m_serverPhysics;

        private GameObject m_playerPrefab;

<<<<<<< HEAD
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
=======
                public GameplayState(udp.server.UDPServer UDPServer,
                    GameObject playerPrefab, 
                    Dictionary<IPEndPoint, ClientConnection> UDPClientConnections, 
                    common.StandardMovementSettings movementSettings, 
                    int snapshotDelay, 
                    string physicsScene)
                {
                    m_UDPserver = UDPServer;
                    m_UDPserver.Subscribe(this);
                    m_tickAccumulator = 0;
                    m_UDPClientConnections = UDPClientConnections;

                    m_movementSettings = movementSettings;
>>>>>>> origin/master

            m_movementSettings = movementSettings;

<<<<<<< HEAD
            m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(physicsScene).GetPhysicsScene2D();
            m_playerPrefab = playerPrefab;

            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_clientInputs = new Dictionary<ClientConnection, common.data.InputMessage>();

            foreach(IPEndPoint ip in m_clientConnections.Keys)
            {
                int id = m_clientConnections[ip].PlayerGUID;
                Rigidbody2D body = GameObject.Instantiate(playerPrefab).GetComponent<Rigidbody2D>();
                m_bodies.Add(id, body);
=======
                    m_bodies = new Dictionary<int, Rigidbody2D>();
                    m_clientInputs = new Dictionary<ClientConnection, common.data.InputMessage>();
                    
                    // instantiate each player
                    foreach (IPEndPoint ip in m_UDPClientConnections.Keys)
                    {
                        int id = m_UDPClientConnections[ip].PlayerGUID;
                        Rigidbody2D body = GameObject.Instantiate(playerPrefab).GetComponent<Rigidbody2D>();
                        body.position = m_bodies.Count * Vector2.left * 3;
                        body.name = "Server player " + id.ToString();
                        m_bodies.Add(id, body);
                    }

                    // add each player to client states
                    foreach (IPEndPoint ip in m_UDPClientConnections.Keys)
                    {
                        common.data.PlayerState player = new common.data.PlayerState();
                        player.GUID.Set(m_UDPClientConnections[ip].PlayerGUID);
                        player.Position.Set(m_bodies[m_UDPClientConnections[ip].PlayerGUID].position);

                        m_UDPClientConnections[ip].State.AddPlayer(player);
                        m_UDPClientConnections[ip].State.SetPlayerID(player.GUID);
                    }

                    // add each player to each other client state
                    foreach (ClientConnection baseConn in m_UDPClientConnections.Values)
                    {
                        foreach (ClientConnection conn in m_UDPClientConnections.Values)
                        {
                            common.data.PlayerState currentPlayer = conn.State.GetPlayer();
                            if (baseConn.PlayerGUID != conn.PlayerGUID)
                            {
                                baseConn.State.AddPlayer(currentPlayer);
                            }
                        }
                    }
                }
>>>>>>> origin/master

                for (int j = 0; j < m_clientConnections.Count; j++)
                {
                    common.data.PlayerState player = new common.data.PlayerState();
                    player.GUID.Set(id);
                    m_clientConnections[ip].State.AddPlayer(player);
                    m_clientConnections[ip].State.SetPlayerID(id);
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

<<<<<<< HEAD
                    if (framesToSimulate < missingFrames) framesToSimulate = missingFrames;
=======
                    if (++m_tickAccumulator > m_snapshotDelay)
                    {
                        m_tickAccumulator = 0;
                        foreach (IPEndPoint ip in m_UDPClientConnections.Keys)
                        {
                            m_UDPserver.Send(m_UDPClientConnections[ip].State.GetBytes(), ip);
                        }
                    }
                    return this;
>>>>>>> origin/master
                }

                for (uint f = framesToSimulate; f > 0; f--)
                {
<<<<<<< HEAD
                    foreach (ClientConnection client in m_clientInputs.Keys)
=======
                    common.data.InputMessage inputs = udp.Serializable.FromBytes<common.data.InputMessage>(packet.Data);
                    if (inputs != null && m_UDPClientConnections.ContainsKey(clientEndPoint))
>>>>>>> origin/master
                    {
                        common.data.InputMessage message = m_clientInputs[client];
                        int messageCount = message.InputFrames.Value.Count;
                        if (messageCount > f)
                        {
<<<<<<< HEAD
                            common.data.InputFrame frame = message.InputFrames.Value[messageCount - (int)f - 1];

                            // must be called in main unity thread
                            Rigidbody2D body = m_bodies[client.PlayerGUID];

                            common.logic.PlayerMovement.Execute(ref body, m_movementSettings, frame, Time.fixedDeltaTime);

                            client.State.GetPlayer().Position.Set(body.position);
                            client.State.Tick.Set(client.ServerTick);
                            client.ServerTick++;
=======
                            m_clientInputs[m_UDPClientConnections[clientEndPoint]] = inputs;
>>>>>>> origin/master
                        }
                    }

                    m_serverPhysics.Simulate(Time.fixedDeltaTime);
                }
<<<<<<< HEAD

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
            common.data.InputMessage inputs = serialization.Serializable.FromBytes<common.data.InputMessage>(packet.Data);
            if (inputs != null && m_clientConnections.ContainsKey(clientEndPoint))
            {
                lock (m_lock)
                {
                    m_clientInputs[m_clientConnections[clientEndPoint]] = inputs;
                }
=======
>>>>>>> origin/master
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
