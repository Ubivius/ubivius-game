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

        private Dictionary<int, IPEndPoint> m_TCPClientEndPoints;
        private Dictionary<int, IPEndPoint> m_UDPClientEndPoints;

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

            m_TCPClientEndPoints = new Dictionary<int, IPEndPoint>();
            m_UDPClientEndPoints = new Dictionary<int, IPEndPoint>();

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

                    foreach (IPEndPoint ip in m_TCPClientEndPoints.Values)
                    {
                        m_TCPServer.Send(message.GetBytes(), ip);
                    }

                    Debug.Log("Waiting for clients to load their worlds");
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
                foreach (IPEndPoint ip in m_TCPClientEndPoints.Values)
                {
                    m_TCPServer.Send(message.GetBytes(), ip);
                }

                m_TCPServer.Unsubscribe(this);

                m_gameplayState.Init(m_UDPClientEndPoints, m_simulationBuffer);
                m_currentState = m_gameplayState;
            }
        }
        
        private void AddNewPlayer(int playerID)
        {
            m_players.Add(new PlayerState(playerID));
            m_readyClients[playerID] = false;
        }

        private void BroadcastPlayerList()
        {
            byte[] bytes = new ClientListMessage(m_players).GetBytes();
            foreach (IPEndPoint ip in m_TCPClientEndPoints.Values)
            {
                m_TCPServer.Send(bytes, ip);
            }
        }

        public void ReceivePacket(TCPToolkit.Packet packet, IPEndPoint clientIP)
        {
            IdentificationMessage identification = Serializable.CreateFromBytes<IdentificationMessage>(packet.Data);
            if (identification != null)
            {
                lock (m_lock)
                {
                    int playerID = identification.PlayerID.Value;

                    if (!m_TCPClientEndPoints.ContainsKey(playerID)) // it's a new player
                    {
                        AddNewPlayer(playerID);
                    }
                    // we update endpoint whether or not it's a new player (could have been a disconnect)
                    m_TCPClientEndPoints[playerID] = clientIP;

                    ServerSuccessfulConnectMessage serverSuccessPing = new ServerSuccessfulConnectMessage();
                    m_TCPServer.Send(serverSuccessPing.GetBytes(), clientIP);

#if DEBUG_LOG
                    Debug.Log("Received TCP connection request from " + clientIP.ToString() + " (player ID  " + playerID + ")");
#endif // DEBUG_LOG

                    BroadcastPlayerList();
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
                foreach(int id in m_TCPClientEndPoints.Keys)
                {
                    if(m_TCPClientEndPoints[id] == clientIP)
                    {
                        m_TCPClientEndPoints[id] = null;
                    }
                }

                foreach (int id in m_UDPClientEndPoints.Keys)
                {
                    if (m_UDPClientEndPoints[id] == clientIP)
                    {
                        m_UDPClientEndPoints[id] = null;
                    }
                }
            }
        }

        public void Receive(UDPToolkit.Packet packet, IPEndPoint clientIP)
        {
            IdentificationMessage identification = Serializable.CreateFromBytes<common.data.IdentificationMessage>(packet.Data);
            if (identification != null)
            {
                int playerID = identification.PlayerID.Value;
#if DEBUG_LOG
                Debug.Log("Received UDP confirmation from " + clientIP.ToString() + " (player ID  " + playerID + ")");
#endif // DEBUG_LOG

                m_UDPClientEndPoints[playerID] = clientIP;
            }
        }
    }
}
