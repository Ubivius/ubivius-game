using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;
using ubv.common.data;
using ubv.common.serialization;
using ubv.common;
using ubv.utils;

namespace ubv.server.logic
{
    /// <summary>
    /// Represents the state of the server before the game.
    /// In charge of regrouping player parties, and launching 
    /// the game with a fixed number of players
    /// </summary>
    public class GameCreationState : ServerState
    {
        [SerializeField] private common.world.WorldGenerator m_worldGenerator;

        private Dictionary<int, common.serialization.types.String> m_clientCharacters; 

        private HashSet<int> m_readyClients;
        private HashSet<int> m_worldLoadedClients;

        [SerializeField] private List<ServerInitializer> m_serverInitializers;

        // Flags
        private Flag m_readyToStartGame;

        protected override void StateAwake()
        {
            ServerState.m_gameCreationState = this;
            m_currentState = this;
            m_readyToStartGame = new Flag();
        }

        protected override void StateStart()
        {
            Init();
        }

        public void Init()
        {
            m_clientCharacters = new Dictionary<int, common.serialization.types.String>();

            m_readyClients = new HashSet<int>();
            m_worldLoadedClients = new HashSet<int>();
            
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
            return m_readyClients.Count > 0 && (m_readyClients.Count == m_clientCharacters.Count);
        }

        private bool EveryoneHasWorld()
        {
            return m_worldLoadedClients.Count > 0 && m_worldLoadedClients.Count == m_clientCharacters.Count;
        }

        protected override void StateUpdate()
        {
            lock (m_lock)
            {
                if (EveryoneIsReady())
                {
                    m_readyClients.Clear();
                    m_UDPServer.Unsubscribe(this);

                    common.world.cellType.CellInfo[,] cellInfoArray = m_worldGenerator.GetCellInfoArray();
                    
                    ServerInitMessage message = new ServerInitMessage(m_clientCharacters, cellInfoArray);

                    foreach (int id in m_clientCharacters.Keys)
                    {
                        m_TCPServer.Send(message.GetBytes(), id);
                    }

                    Debug.Log("Waiting for clients to load their worlds");
                }
                
                if (EveryoneHasWorld())
                {
                    m_readyToStartGame.Raise();
                }
            }
                    
            if (m_readyToStartGame.Read())
            {
                m_worldLoadedClients.Clear();
                ServerStartsMessage message = new ServerStartsMessage();

                Debug.Log("Starting game.");
                foreach (int id in m_clientCharacters.Keys)
                {
                    m_TCPServer.Send(message.GetBytes(), id);
                }

                m_TCPServer.Unsubscribe(this);

                m_gameplayState.Init(m_clientCharacters.Keys);
                m_currentState = m_gameplayState;
            }
        }
        
        private void AddNewPlayer(int playerID)
        {
            m_clientCharacters[playerID] = new common.serialization.types.String();
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
            if (ready != null)
            {
                Debug.Log("Client " + playerID + " is ready to receive world.");
                m_readyClients.Add(playerID);
                return;
            }
            
            ClientWorldLoadedMessage clientWorldLoaded = IConvertible.CreateFromBytes<ClientWorldLoadedMessage>(packet.Data.ArraySegment());
            if (clientWorldLoaded != null)
            {
                Debug.Log("Client " + playerID + " has loaded its world.");
                m_worldLoadedClients.Add(playerID);
                return;
            }
        }

        public void OnConnect(int playerID)
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
            }
        }

        public void OnDisconnect(int playerID)
        {
#if DEBUG_LOG
            Debug.Log("Player (ID  " + playerID + ") disconnected.");
#endif // DEBUG_LOG
            RemovePlayerFromLobby(playerID);
            BroadcastPlayerList();
        }

        private void RemovePlayerFromLobby(int playerID)
        {
            lock (m_lock)
            {
                m_clientCharacters.Remove(playerID);
                m_readyClients.Remove(playerID);
                m_worldLoadedClients.Remove(playerID);
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
