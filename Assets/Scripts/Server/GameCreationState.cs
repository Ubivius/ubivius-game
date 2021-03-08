using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;
using ubv.common.data;
using ubv.common.serialization;

namespace ubv.server.logic
{
    /// <summary>
    /// Represents the state of the server before the game.
    /// In charge of regrouping player parties, and launching 
    /// the game with a fixed number of players
    /// </summary>
    public class GameCreationState : ServerState, tcp.server.ITCPServerReceiver, udp.server.IUDPServerReceiver
    {
        private common.world.WorldGenerator m_worldGenerator;
        private tcp.server.TCPServer m_TCPServer;
        private Dictionary<IPEndPoint, ClientConnection> m_TCPClientConnections;
        private Dictionary<IPEndPoint, ClientConnection> m_UDPClientConnections;

        private Dictionary<int, bool> m_readyClients;

        // transfer to gameplay
        private readonly string m_physicsScene;
        private readonly udp.server.UDPServer m_UDPserver;
        private readonly common.StandardMovementSettings m_movementSettings;
        private readonly int m_snapshotDelay;
        private readonly GameObject m_playerPrefab;
        private readonly int m_simulationBuffer;

        private List<common.data.PlayerState> m_players;

#if NETWORK_SIMULATE
        private bool m_forceStartGame;
#endif // NETWORK_SIMULATE

        public GameCreationState(udp.server.UDPServer UDPServer, 
            tcp.server.TCPServer TCPServer,
            GameObject playerPrefab, 
            common.world.WorldGenerator worldGenerator,
            common.StandardMovementSettings 
            movementSettings, 
            int snapshotDelay, 
            int simulationBuffer,
            string physicsScene
#if NETWORK_SIMULATE
            , ServerUpdate parent
#endif // NETWORK_SIMULATE 
            )
        {
            m_UDPserver = UDPServer;
            m_TCPServer = TCPServer;
            m_players = new List<common.data.PlayerState>();
            m_TCPClientConnections = new Dictionary<IPEndPoint, ClientConnection>();
            m_UDPClientConnections = new Dictionary<IPEndPoint, ClientConnection>();

            m_readyClients = new Dictionary<int, bool>();

            m_movementSettings = movementSettings;
            m_snapshotDelay = snapshotDelay;
            m_simulationBuffer = simulationBuffer;
            m_physicsScene = physicsScene;
            m_playerPrefab = playerPrefab;

            m_forceStartGame = false;

            m_worldGenerator = worldGenerator;

            m_worldGenerator.GenerateWorld();
                    
#if NETWORK_SIMULATE
            parent.ForceStartGameButtonEvent.AddListener(() => 
            {
                Debug.Log("Forcing game start");
                m_forceStartGame = true;
            });
#endif // NETWORK_SIMULATE 
                    
            m_TCPServer.Subscribe(this);
            m_UDPserver.Subscribe(this);
        }

        public override ServerState Update()
        {
            lock (m_lock)
            {
                if ((m_TCPClientConnections.Count > 3 // TODO : Change here when matchmaking microservice is up
#if NETWORK_SIMULATE
                    || m_forceStartGame
#endif // NETWORK_SIMULATE
                    ) && !awaitingClients)
                {
                    m_UDPserver.Unsubscribe(this);

                    common.world.cellType.CellInfo[,] cellInfoArray = m_worldGenerator.GetCellInfoArray();

                    common.data.GameInitMessage message = new common.data.GameInitMessage(m_simulationBuffer, m_players, cellInfoArray);

                    foreach (IPEndPoint ip in m_TCPClientConnections.Keys)
                    {
                        m_TCPServer.Send(message.GetBytes(), ip);
                    }

                    Debug.Log("Waiting for clients to be ready");
                    awaitingClients = true;
                }
            }
                    
            if(m_readyClients.Count == m_players.Count && m_players.Count > 0 && awaitingClients)
            {
                GameReadyMessage message = new GameReadyMessage();

                Debug.Log("Starting game.");
                foreach (IPEndPoint ip in m_TCPClientConnections.Keys)
                {
                    m_TCPServer.Send(message.GetBytes(), ip);
                }

                return new GameplayState(m_UDPserver, m_playerPrefab, m_UDPClientConnections, m_movementSettings, m_snapshotDelay, m_simulationBuffer, m_physicsScene);
            }

            return this;
        }

        public override ServerState FixedUpdate()
        {
            return this;
        }

        // TEMP
        bool awaitingClients = false;

        public void ReceivePacket(TCPToolkit.Packet packet, IPEndPoint clientIP)
        {
            GameReadyMessage ready = Serializable.CreateFromBytes<GameReadyMessage>(packet.Data);
            if(ready != null)
            {
                Debug.Log("Client " + ready.PlayerID.Value + " is ready");
                m_readyClients[ready.PlayerID.Value] = true;
            }
            return;
        }

        public void OnConnect(IPEndPoint clientIP)
        {
            lock (m_lock)
            {
                int playerID = System.Guid.NewGuid().GetHashCode();

                // TODO get rid of client connection data and only use serializable list of int after serialize rework
                m_TCPClientConnections[clientIP] = new ClientConnection(playerID);

                common.data.IdentificationMessage idMessage = new common.data.IdentificationMessage(playerID);

                common.data.PlayerState playerState = new common.data.PlayerState(playerID);

                // set rotation / position according to existing players?

                m_players.Add(playerState);
                m_UDPserver.RegisterClient(clientIP.Address);

                m_TCPServer.Send(idMessage.GetBytes(), clientIP);

#if DEBUG_LOG
                Debug.Log("Received connection request from " + clientIP.ToString() + ", attributed " + playerID);
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
            
            common.data.IdentificationMessage identification = common.serialization.Serializable.CreateFromBytes<common.data.IdentificationMessage>(packet.Data);
            if (identification != null)
            {
                m_UDPClientConnections[clientIP] = new ClientConnection(identification.PlayerID.Value);

                // broadcast connection to all players
                byte[] bytes = new ClientListMessage(m_players).GetBytes();
                foreach (IPEndPoint ip in m_TCPClientConnections.Keys)
                {
                    m_TCPServer.Send(bytes, ip);
                }
            }
        }
    }
}
