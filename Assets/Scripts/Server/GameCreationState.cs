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
        private enum SubState
        {
            SUBSTATE_WAITING_FOR_PLAYERS = 0,
            SUBSTATE_WAITING_FOR_WORLD_LOADED,
            SUBSTATE_WAITING_FOR_PLAY,
        }

        private SubState m_currentSubState;

        [SerializeField] private common.world.WorldGenerator m_worldGenerator;

        private Dictionary<int, common.serialization.types.String> m_clientCharacters; 

        private HashSet<int> m_readyClients;
        private HashSet<int> m_worldLoadedClients;

        [SerializeField] private List<ServerInitializer> m_serverInitializers;
        

        protected override void StateAwake()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_PLAYERS;
            ServerState.m_gameCreationState = this;
            ChangeState(this);
        }

        protected override void StateStart()
        {
            Init();
        }

        public void Init()
        {
            m_serverConnection.UDPServer.AcceptNewClients = true;
            m_clientCharacters = new Dictionary<int, common.serialization.types.String>();

            m_readyClients = new HashSet<int>();
            m_worldLoadedClients = new HashSet<int>();
            
            foreach(ServerInitializer initializer in m_serverInitializers)
            {
                initializer.Init();
            }
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
            switch (m_currentSubState)
            {
                case SubState.SUBSTATE_WAITING_FOR_PLAYERS:
                    if (EveryoneIsReady())
                    {
                        common.world.cellType.CellInfo[,] cellInfoArray = m_worldGenerator.GetCellInfoArray();

                        ServerInitMessage message = new ServerInitMessage(m_clientCharacters, cellInfoArray);

                        foreach (int id in m_clientCharacters.Keys)
                        {
                            m_serverConnection.TCPServer.Send(message.GetBytes(), id);
                        }
#if DEBUG_LOG
                        Debug.Log("Waiting for clients to load their worlds");
#endif // DEBUG_LOG
                        m_currentSubState = SubState.SUBSTATE_WAITING_FOR_WORLD_LOADED;
                    }
                    break;
                case SubState.SUBSTATE_WAITING_FOR_WORLD_LOADED:
                    if (EveryoneHasWorld())
                    {
                        m_worldLoadedClients.Clear();
                        ServerStartsMessage message = new ServerStartsMessage();
#if DEBUG_LOG
                        Debug.Log("Starting game.");
#endif // DEBUG_LOG
                        foreach (int id in m_clientCharacters.Keys)
                        {
                            m_serverConnection.TCPServer.Send(message.GetBytes(), id);
                        }

                        m_currentSubState = SubState.SUBSTATE_WAITING_FOR_PLAY;
                        
                        m_gameplayState.Init(m_clientCharacters.Keys);
                        ChangeState(m_gameplayState);
                    }
                    break;
                default:
                    break;
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
#if DEBUG_LOG
                Debug.Log("Broadcasting character " + m_clientCharacters[id].Value + " (from player " + id + ")");
#endif // DEBUG_LOG
                m_serverConnection.TCPServer.Send(bytes, id);
            }
        }

        protected override void OnTCPReceiveFrom (TCPToolkit.Packet packet, int playerID)
        {
            OnLobbyEnteredMessage lobbyEnter = Serializable.CreateFromBytes<OnLobbyEnteredMessage>(packet.Data.ArraySegment());
            if (lobbyEnter != null)
            {
                lock (m_lock)
                {
                    if (m_clientCharacters.ContainsKey(playerID))
                    {
#if DEBUG_LOG
                        Debug.Log("Adding character " + lobbyEnter.CharacterID.Value + " to player " + playerID);
#endif // DEBUG_LOG
                        m_clientCharacters[playerID] = lobbyEnter.CharacterID;
                        BroadcastPlayerList();
                    }
                }
                return;
            }

            ClientReadyMessage ready = IConvertible.CreateFromBytes<ClientReadyMessage>(packet.Data.ArraySegment());
            if (ready != null)
            {
#if DEBUG_LOG
                Debug.Log("Client " + playerID + " is ready to receive world.");
#endif // DEBUG_LOG
                m_readyClients.Add(playerID);
                return;
            }
            
            ClientWorldLoadedMessage clientWorldLoaded = IConvertible.CreateFromBytes<ClientWorldLoadedMessage>(packet.Data.ArraySegment());
            if (clientWorldLoaded != null)
            {
#if DEBUG_LOG
                Debug.Log("Client " + playerID + " has loaded its world.");
#endif // DEBUG_LOG
                m_worldLoadedClients.Add(playerID);
                return;
            }
        }

        protected override void OnPlayerConnect(int playerID)
        {
            lock (m_lock)
            {
                if (!m_clientCharacters.ContainsKey(playerID))
                {
#if DEBUG_LOG
                    Debug.Log("New player just connected to server: " + playerID);
#endif // DEBUG_LOG
                    AddNewPlayer(playerID);
                }
            }
        }

        protected override void OnPlayerDisconnect(int playerID)
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
    }
}
