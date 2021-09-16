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
    public class GameCreationState : ServerState, tcp.server.ITCPServerReceiver, udp.server.IUDPServerReceiver, http.server.IHTTPServerReceiver
    {
        [SerializeField] private int m_simulationBuffer;
        [SerializeField] private common.world.WorldGenerator m_worldGenerator;

        private Dictionary<int, common.serialization.types.String> m_clientCharacters; 

        private Dictionary<int, bool> m_readyClients;

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
            m_clientCharacters = new Dictionary<int, common.serialization.types.String>();

            m_readyClients = new Dictionary<int, bool>();
            
            foreach(ServerInitializer initializer in m_serverInitializers)
            {
                initializer.Init();
            }
                    
            m_TCPServer.Subscribe(this);
            m_UDPServer.Subscribe(this);
            m_UDPServer.AcceptNewClients = true;
            m_HTTPServer.Subscribe(this);
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
                    
                    ServerInitMessage message = new ServerInitMessage(m_simulationBuffer, m_clientCharacters, cellInfoArray);

                    foreach (int id in m_clientCharacters.Keys)
                    {
                        m_TCPServer.Send(message.GetBytes(), id);
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
                foreach (int id in m_clientCharacters.Keys)
                {
                    m_TCPServer.Send(message.GetBytes(), id);
                }

                m_TCPServer.Unsubscribe(this);

                m_gameplayState.Init(m_clientCharacters, m_simulationBuffer);
                m_currentState = m_gameplayState;
            }
        }
        
        private void AddNewPlayer(int playerID)
        {
            m_clientCharacters[playerID] = new common.serialization.types.String();
            m_readyClients[playerID] = false;
        }

        private void BroadcastPlayerList()
        {
            byte[] bytes = new CharacterListMessage(m_clientCharacters).GetBytes();
            foreach (int id in m_clientCharacters.Keys)
            {
                Debug.Log("Broadcasting player/char " + m_clientCharacters[id].Value + " (from player " + id + ")");
                m_TCPServer.Send(bytes, id);
            }
        }

        public void ReceivePacket(TCPToolkit.Packet packet, int playerID)
        {
            IdentificationMessage identification = Serializable.CreateFromBytes<IdentificationMessage>(packet.Data.ArraySegment());
            if (identification != null)
            {
                lock (m_lock)
                {
                    if (!m_clientCharacters.ContainsKey(playerID)) // it's a new player
                    {
                        AddNewPlayer(playerID);
                    }
                    
#if DEBUG_LOG
                    Debug.Log("Received TCP connection request from player (ID  " + playerID + ")");
#endif // DEBUG_LOG

                    ServerSuccessfulConnectMessage serverSuccessPing = new ServerSuccessfulConnectMessage();
                    m_TCPServer.Send(serverSuccessPing.GetBytes(), playerID);
                }
                return;
            }

            OnLobbyEnteredMessage lobbyEnter = Serializable.CreateFromBytes<OnLobbyEnteredMessage>(packet.Data.ArraySegment());
            if (lobbyEnter != null)
            {
                lock (m_lock)
                {
                    if (m_clientCharacters.ContainsKey(playerID))
                    {
                        Debug.Log("Adding character " + lobbyEnter.CharacterID.Value + " to player " + playerID);
                        m_clientCharacters[playerID] = lobbyEnter.CharacterID;
                        BroadcastPlayerList();
                    }
                }
                return;
            }

            ClientReadyMessage ready = IConvertible.CreateFromBytes<ClientReadyMessage>(packet.Data.ArraySegment());
            if (ready != null && m_readyClients.ContainsKey(playerID))
            {
                Debug.Log("Client " + playerID + " is ready to receive world.");
                m_readyClients[playerID] = true;
                return;
            }

            if (m_awaitingClientLoadWorld)
            {
                ClientWorldLoadedMessage clientWorldLoaded = IConvertible.CreateFromBytes<ClientWorldLoadedMessage>(packet.Data.ArraySegment());
                if (clientWorldLoaded != null)
                {
                    m_readyClients.Remove(playerID);
                }
            }
            
        }

        public void OnConnect(int playerID)
        { }

        public void OnDisconnect(int playerID)
        {
            lock (m_lock)
            {
                m_clientCharacters.Remove(playerID);
            }
        }

        public void Receive(UDPToolkit.Packet packet, int playerID)
        {
            IdentificationMessage identification = Serializable.CreateFromBytes<common.data.IdentificationMessage>(packet.Data.ArraySegment());
            if (identification != null)
            {
#if DEBUG_LOG
                Debug.Log("Received UDP confirmation from player (ID  " + playerID + ")");
#endif // DEBUG_LOG
                ServerSuccessfulConnectMessage serverSuccessPing = new ServerSuccessfulConnectMessage();
                m_UDPServer.Send(serverSuccessPing.GetBytes(), playerID);
            } 
        }
    }
}
