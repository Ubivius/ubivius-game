using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;
using ubv.common.data;
using ubv.common.serialization;
using ubv.common;

namespace ubv.server.logic
{
    /// <summary>
    /// Represents the state of the server before the game.
    /// In charge of regrouping player parties, and launching 
    /// the game with a fixed number of players
    /// </summary>
    public class GameCreationState : ServerState, tcp.server.ITCPServerReceiver, udp.server.IUDPServerReceiver
    {
#if NETWORK_SIMULATE
        [SerializeField] private ServerUpdate m_parent;
#endif // NETWORK_SIMULATE 

        [SerializeField] private int m_simulationBuffer;
        [SerializeField] private common.world.WorldGenerator m_worldGenerator;
        private Dictionary<IPEndPoint, ClientState> m_TCPClientStates;
        private Dictionary<IPEndPoint, ClientState> m_UDPClientStates;

        private Dictionary<int, bool> m_readyClients;
        
        private List<PlayerState> m_players;

        [SerializeField] private List<ServerInitializer> m_serverInitializers;

#if NETWORK_SIMULATE
        private bool m_forceStartGame;
#endif // NETWORK_SIMULATE

        protected override void StateAwake()
        {
            ServerState.m_gameCreationState = this;
            m_currentState = this;
        }

        protected override void StateStart()
        {
            Init();
        }

        public void Init()
        {
            m_players = new List<PlayerState>();
            m_TCPClientStates = new Dictionary<IPEndPoint, ClientState>();
            m_UDPClientStates = new Dictionary<IPEndPoint, ClientState>();

            m_readyClients = new Dictionary<int, bool>();
            
            m_forceStartGame = false;
            
            foreach(ServerInitializer initializer in m_serverInitializers)
            {
                initializer.Init();
            }
                    
#if NETWORK_SIMULATE
            m_parent.ForceStartGameButtonEvent.AddListener(() => 
            {
                Debug.Log("Forcing game start");
                m_forceStartGame = true;
            });
#endif // NETWORK_SIMULATE 
                    
            m_TCPServer.Subscribe(this);
            m_UDPServer.Subscribe(this);
        }

        protected override void StateUpdate()
        {
            lock (m_lock)
            {
                if ((m_TCPClientStates.Count > 3 // TODO : Change here when matchmaking microservice is up
#if NETWORK_SIMULATE
                    || m_forceStartGame
#endif // NETWORK_SIMULATE
                    ) && !awaitingClients)
                {
                    m_UDPServer.Unsubscribe(this);

                    common.world.cellType.CellInfo[,] cellInfoArray = m_worldGenerator.GetCellInfoArray();

                    GameInitMessage message = new GameInitMessage(m_simulationBuffer, m_players, cellInfoArray);

                    foreach (IPEndPoint ip in m_TCPClientStates.Keys)
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
                foreach (IPEndPoint ip in m_TCPClientStates.Keys)
                {
                    m_TCPServer.Send(message.GetBytes(), ip);
                }

                m_gameplayState.Init(m_UDPClientStates, m_simulationBuffer);
                m_currentState = m_gameplayState;
            }
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
                m_TCPClientStates[clientIP] = new ClientState(playerID);

                IdentificationMessage idMessage = new IdentificationMessage(playerID);

                PlayerState playerState = new PlayerState(playerID);

                // set rotation / position according to existing players?

                m_players.Add(playerState);
                m_UDPServer.RegisterClient(clientIP.Address);

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
                m_TCPClientStates.Remove(clientIP);
            }
        }

        public void Receive(UDPToolkit.Packet packet, IPEndPoint clientIP)
        {
            if (m_UDPClientStates.ContainsKey(clientIP))
            {
#if DEBUG_LOG
                Debug.Log("Client " + clientIP.ToString() + " already connected. Ignoring.");
#endif // DEBUG_LOG
                return;
            }
            
            IdentificationMessage identification = Serializable.CreateFromBytes<common.data.IdentificationMessage>(packet.Data);
            if (identification != null)
            {
                m_UDPClientStates[clientIP] = new ClientState(identification.PlayerID.Value);

                // broadcast connection to all players
                byte[] bytes = new ClientListMessage(m_players).GetBytes();
                foreach (IPEndPoint ip in m_TCPClientStates.Keys)
                {
                    m_TCPServer.Send(bytes, ip);
                }
            }
        }
    }
}
