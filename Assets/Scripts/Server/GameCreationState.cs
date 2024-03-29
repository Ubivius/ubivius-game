﻿using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;
using ubv.common.data;
using ubv.common.serialization;
using ubv.common;
using ubv.utils;
using ubv.microservices;

namespace ubv.server.logic
{
    /// <summary>
    /// Represents the state of the server before the game.
    /// In charge of regrouping player parties, and launching 
    /// the game with a fixed number of players
    /// </summary>
    public class GameCreationState : ServerState
    {
        static public ServerInitMessage CachedServerInit;

        [SerializeField]
        private http.agonesServer.HTTPAgonesServer m_agones;

        private enum SubState
        {
            SUBSTATE_WAITING_FOR_PLAYERS = 0,
            SUBSTATE_WAITING_FOR_WORLD_LOADED,
            SUBSTATE_GOING_TO_PLAY,
        }

        private SubState m_currentSubState;

        [SerializeField] private TextChatService m_textChatService;
        [SerializeField] private common.world.WorldGenerator m_worldGenerator;

        private Dictionary<int, common.serialization.types.String> m_clientCharacters;
        private HashSet<int> m_activeClients;

        private HashSet<int> m_readyClients;
        private HashSet<int> m_worldLoadedClients;

        private Dictionary<int, string> m_intToStringUserIDs;
             
        private string m_conversationID;
        private string m_uniqueGameID;

        [SerializeField] private List<ServerInitializer> m_serverInitializers;
        

        protected override void StateAwake()
        {
            CachedServerInit = null;
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_PLAYERS;
            ServerState.m_gameCreationState = this;
            ChangeState(this);
            m_uniqueGameID = System.Guid.NewGuid().ToString();
        }

        protected override void StateStart()
        {
            Init();
        }

        public void Init()
        {
            m_intToStringUserIDs = new Dictionary<int, string>();
            m_activeClients = new HashSet<int>();
            m_clientCharacters = new Dictionary<int, common.serialization.types.String>();

            m_readyClients = new HashSet<int>();
            m_worldLoadedClients = new HashSet<int>();
            
            foreach(ServerInitializer initializer in m_serverInitializers)
            {
                initializer.Init();
            }
            
            m_textChatService.CreateNewConversation(m_uniqueGameID, new string[0], OnConversationCreated);
        }

        private void OnConversationCreated(string conversationID)
        {
            Debug.Log("Creating conversation :" + conversationID);
            m_conversationID = conversationID;
            m_agones.ReadyGameServer();
        }
        
        private bool EveryoneIsReady()
        {
            return m_readyClients.Count > 0 && (m_readyClients.Count == m_activeClients.Count);
        }

        private bool EveryoneHasWorld()
        {
            return m_worldLoadedClients.Count > 0 && m_worldLoadedClients.Count == m_activeClients.Count;
        }

        protected override void StateUpdate()
        {
            switch (m_currentSubState)
            {
                case SubState.SUBSTATE_WAITING_FOR_PLAYERS:
                    if (EveryoneIsReady())
                    {
                        common.world.cellType.CellInfo[,] cellInfoArray = m_worldGenerator.GetCellInfoArray();

                        CachedServerInit = new ServerInitMessage(m_clientCharacters, cellInfoArray);

                        foreach (int id in m_activeClients)
                        {
                            m_serverConnection.TCPServer.Send(CachedServerInit.GetBytes(), id);
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

                        m_currentSubState = SubState.SUBSTATE_GOING_TO_PLAY;

                        m_gameplayState.Init(m_clientCharacters);
                        ChangeState(m_gameplayState);
                    }
                    break;
                default:
                    break;
            }
        }

        private void BroadcastPlayerList()
        {
            byte[] bytes = new CharacterListMessage(GetActiveCharacters()).GetBytes();
            foreach (int id in m_activeClients)
            {
                m_serverConnection.TCPServer.Send(bytes, id);
            }
        }

        private void BroadcastClientsStatus()
        {
            Dictionary<int, common.serialization.types.Bool> readyClients = new Dictionary<int, common.serialization.types.Bool>();
            
            foreach(int  id in m_activeClients)
            {
                readyClients.Add(id, new common.serialization.types.Bool(m_readyClients.Contains(id)));
            }

            byte[] bytes = new ClientStatusMessage(readyClients).GetBytes();
            foreach (int id in m_activeClients)
            {
                Debug.Log("Sending to :" + id);
                m_serverConnection.TCPServer.Send(bytes, id);
            }
        }

        private Dictionary<int, common.serialization.types.String> GetActiveCharacters()
        {
            Dictionary<int, common.serialization.types.String> activeCharacters = new Dictionary<int, common.serialization.types.String>();
            foreach (int id in m_activeClients)
            {
                Debug.Log("Active character :" + m_clientCharacters[id]);
                activeCharacters.Add(id, m_clientCharacters[id]);
            }
            return activeCharacters;
        }

        protected override void OnTCPReceiveFrom (TCPToolkit.Packet packet, int playerID)
        {
            ServerStatusMessage status = Serializable.CreateFromBytes<ServerStatusMessage>(packet.Data.ArraySegment());
            if (status != null)
            {
                Debug.Log("Received status request from " + playerID);
                status.PlayerID.Value = playerID;
                status.IsInServer.Value = m_clientCharacters.ContainsKey(playerID);
                status.GameStatus.Value = (uint)ServerStatusMessage.ServerStatus.STATUS_LOBBY;
                status.AcceptsNewPlayers.Value = m_currentSubState == SubState.SUBSTATE_WAITING_FOR_PLAYERS && m_activeClients.Count < 4;
                status.CharacterID.Value = m_clientCharacters.ContainsKey(playerID) ? m_clientCharacters[playerID].Value : string.Empty;
                status.GameChatID.Value = m_conversationID;
                m_serverConnection.TCPServer.Send(status.GetBytes(), playerID);
                return;
            }

            OnLobbyEnteredMessage lobbyEnter = Serializable.CreateFromBytes<OnLobbyEnteredMessage>(packet.Data.ArraySegment());
            if (lobbyEnter != null)
            {
                lock (m_lock)
                {
                    if (m_clientCharacters.ContainsKey(playerID))
                    {
                        if (!string.IsNullOrEmpty(lobbyEnter.CharacterID.Value))
                        {
                            if(m_activeClients.Count == 0)
                            {
                                m_agones.AllocateGameServer();
                            }
                            m_intToStringUserIDs[playerID] = lobbyEnter.StringPlayerID.Value;
#if DEBUG_LOG
                            Debug.Log("Adding character " + lobbyEnter.CharacterID.Value + " to player " + playerID);
#endif // DEBUG_LOG
                            m_clientCharacters[playerID] = lobbyEnter.CharacterID;
                            m_activeClients.Add(playerID);
                            BroadcastPlayerList();
                            m_textChatService.SetUsersOfConversation(m_conversationID, m_uniqueGameID, ActiveUsersAsStringArray(), null);
                        }
                        else
                        {
#if DEBUG_LOG
                            Debug.Log("Player " + playerID + " has no character. Refusing.");
#endif // DEBUG_LOG
                            RemovePlayerFromLobby(playerID);
                        }
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

                BroadcastClientsStatus();
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

        private string[] ActiveUsersAsStringArray()
        {
            string[] users = new string[m_activeClients.Count];

            int i = 0;
            foreach(int id in m_activeClients)
            {
                users[i++] = m_intToStringUserIDs[id];
            }

            return users;
        }

        protected override void OnPlayerConnect(int playerID)
        {
            lock (m_lock)
            {
                if (!m_clientCharacters.ContainsKey(playerID))
                {
#if DEBUG_LOG
                    Debug.Log("New player just connected to server: " + playerID + ". Awaiting his character");
#endif // DEBUG_LOG
                    m_clientCharacters[playerID] = new common.serialization.types.String(string.Empty);
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
                m_activeClients.Remove(playerID);
                m_readyClients.Remove(playerID);
                m_worldLoadedClients.Remove(playerID);
                m_textChatService.SetUsersOfConversation(m_conversationID, m_uniqueGameID, ActiveUsersAsStringArray(), null);
            }
        }
    }
}
