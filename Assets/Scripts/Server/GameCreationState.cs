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
        [SerializeField] private int m_simulationBuffer;
        [SerializeField] private common.world.WorldGenerator m_worldGenerator;
        private Dictionary<IPEndPoint, ClientState> m_TCPClientStates;
        private Dictionary<IPEndPoint, ClientState> m_UDPClientStates;

        private Dictionary<int, bool> m_readyClients;
        private List<PlayerState> m_players;

        [SerializeField] private List<ServerInitializer> m_serverInitializers;

        // Flags
        private bool m_readyToStartGame;
        private bool m_awaitingClientLoadWorld;

        protected override void StateAwake()
        {
            ServerState.m_gameCreationState = this;
            m_currentState = this;
            m_readyToStartGame = false;
            m_awaitingClientLoadWorld = false;
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
            
            foreach(ServerInitializer initializer in m_serverInitializers)
            {
                initializer.Init();
            }
                    
            m_TCPServer.Subscribe(this);
            m_UDPServer.Subscribe(this);
            m_UDPServer.AcceptNewClients = true;

        }

        private bool EveryoneIsReady()
        {
            foreach(bool ready in m_readyClients.Values)
            {
                if (!ready)
                {
                    return false;
                }
            }

            return m_readyClients.Count > 0;
        }

        private bool EveryoneIsWorldLoaded()
        {
            return m_awaitingClientLoadWorld && m_readyClients.Count == 0;
        }

        protected override void StateUpdate()
        {
            lock (m_lock)
            {
                if (EveryoneIsReady() && !m_awaitingClientLoadWorld)
                {
                    m_UDPServer.Unsubscribe(this);

                    common.world.cellType.CellInfo[,] cellInfoArray = m_worldGenerator.GetCellInfoArray();

                    ServerInitMessage message = new ServerInitMessage(m_simulationBuffer, m_players, cellInfoArray);

                    foreach (IPEndPoint ip in m_TCPClientStates.Keys)
                    {
                        m_TCPServer.Send(message.GetBytes(), ip);
                    }

                    Debug.Log("Waiting for clients to be ready");
                    m_awaitingClientLoadWorld = true;
                }
                
                if (EveryoneIsWorldLoaded())
                {
                    m_readyToStartGame = true;
                    m_awaitingClientLoadWorld = false;
                }
            }
                    
            if (m_readyToStartGame)
            {
                m_readyToStartGame = false;
                ServerStartsMessage message = new ServerStartsMessage();

                Debug.Log("Starting game.");
                foreach (IPEndPoint ip in m_TCPClientStates.Keys)
                {
                    m_TCPServer.Send(message.GetBytes(), ip);
                }

                m_TCPServer.Unsubscribe(this);

                m_gameplayState.Init(m_UDPClientStates, m_simulationBuffer);
                m_currentState = m_gameplayState;
            }
        }
        
        public void ReceivePacket(TCPToolkit.Packet packet, IPEndPoint clientIP)
        {
            IdentificationMessage identification = Serializable.CreateFromBytes<IdentificationMessage>(packet.Data);
            if (identification != null)
            {
                lock (m_lock)
                {
                    if (!m_TCPClientStates.ContainsKey(clientIP))
                    {
                        int playerID = identification.PlayerID.Value;

                        // TODO get rid of client connection data and only use serializable list of int after serialize rework
                        m_TCPClientStates[clientIP] = new ClientState(playerID);
                        
                        PlayerState playerState = new PlayerState(playerID);

                        // set rotation / position according to existing players?

                        m_players.Add(playerState);
                        m_readyClients[playerID] = false;

                        ServerSuccessfulConnectMessage serverSuccessPing = new ServerSuccessfulConnectMessage();

                        m_TCPServer.Send(serverSuccessPing.GetBytes(), clientIP);

#if DEBUG_LOG
                        Debug.Log("Received connection request from " + clientIP.ToString() + " (player ID  " + playerID + ")");
#endif // DEBUG_LOG
                    }
                }
                return;
            }

            ClientReadyMessage ready = IConvertible.CreateFromBytes<ClientReadyMessage>(packet.Data);
            if (ready != null && m_readyClients.ContainsKey(ready.PlayerID.Value))
            {
                Debug.Log("Client " + ready.PlayerID.Value + " is ready to receive world.");
                m_readyClients[ready.PlayerID.Value] = true;
                return;
            }

            if (m_awaitingClientLoadWorld)
            {
                ClientWorldLoadedMessage clientWorldLoaded = IConvertible.CreateFromBytes<ClientWorldLoadedMessage>(packet.Data);
                if (clientWorldLoaded != null)
                {
                    m_readyClients.Remove(clientWorldLoaded.PlayerID.Value);
                }
            }
            
        }

        public void OnConnect(IPEndPoint clientIP)
        { }

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
                Debug.Log("UDP Client " + clientIP.ToString() + " already connected. Ignoring.");
#endif // DEBUG_LOG
                return;
            }

            IdentificationMessage identification = Serializable.CreateFromBytes<common.data.IdentificationMessage>(packet.Data);
            if (identification != null)
            {
#if DEBUG_LOG
                Debug.Log("Received UDP confirmation from " + clientIP.ToString() + " (player ID  " + identification.PlayerID.Value + ")");
#endif // DEBUG_LOG
                
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
