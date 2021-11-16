using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using UnityEngine.Events;
using static ubv.microservices.CharacterDataService;
using static ubv.common.data.CharacterListMessage;
using ubv.utils;
using ubv.microservices;
using System.Threading;

namespace ubv.client.logic
{
    public class ClientSyncLobby : ClientSyncState
    {
        private enum SubState
        {
            SUBSTATE_WAITING_FOR_WORLD,
            SUBSTATE_GOING_TO_GAME,
            SUBSTATE_LEAVING_LOBBY,
            SUBSTATE_TRANSITION
        }

        [SerializeField] private string m_menuScene;
        [SerializeField] private string m_characterSelectScene;
        [SerializeField] private string m_clientPlayScene;

        private Flag m_serverSignal;
        
        private Dictionary<int, CharacterData> m_clientCharacters;

        private ServerInitMessage m_awaitedInitMessage;

        public UnityAction<List<CharacterData>> OnClientListUpdate;

        private string m_activeCharacterID;

        private SubState m_currentSubState;

        public float LoadPercentage { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            m_canBack = true;
        }

        protected override void StateLoad()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_WORLD;
            LoadPercentage = 0;
            m_clientCharacters = new Dictionary<int, CharacterData>();
            m_server.OnTCPReceive += Receive;
            m_server.OnServerDisconnect += OnDisconnect;
            Init();
        }
        
        public void SendReadyToServer()
        {
            ClientReadyMessage clientReadyMessage = new ClientReadyMessage();
            m_server.TCPSend(clientReadyMessage.GetBytes());
        }
        
        private void Receive(tcp.TCPToolkit.Packet packet)
        {
            if (m_currentSubState == SubState.SUBSTATE_WAITING_FOR_WORLD)
            {
                // loads other players in lobby, receives message from server indicating a new player joined / left
                CharacterListMessage clientList = common.serialization.IConvertible.CreateFromBytes<CharacterListMessage>(packet.Data.ArraySegment());
                if (clientList != null)
                {
                    Debug.Log("Received " + clientList.PlayerCharacters.Value.Count + " characters from server");
                    m_clientCharacters.Clear(); // clear old list
                    foreach (common.serialization.types.String id in clientList.PlayerCharacters.Value.Values)
                    {
                        Debug.Log("Fetching character " + id.Value + " from microservice");
                        // fetch character data from microservice
                        string strID = id.Value;
                        CharacterService.GetCharacter(strID, (CharacterData character) =>
                        {
                            lock (m_lock)
                            {
                                Debug.Log("Got character from " + character.PlayerID + " : " + character.Name);
                                m_clientCharacters[character.PlayerID.GetHashCode()] = character;
                                if (character.PlayerID.Equals(CurrentUser.StringID))
                                {
                                    data.LoadingData.ActiveCharacterID = character.ID;
                                }
                                OnClientListUpdate?.Invoke(new List<CharacterData>(m_clientCharacters.Values));
                            }
                        });
                    }
                    return;
                }

                Thread deserializeWorldThread = new Thread(() =>
                {
#if DEBUG_LOG
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                    watch.Start();
#endif // DEBUG_LOG
                ServerInitMessage init = common.serialization.IConvertible.CreateFromBytes<ServerInitMessage>(packet.Data.ArraySegment());
#if DEBUG_LOG
                watch.Stop();
#endif //DEBUG_LOG
                if (init != null)
                    {
#if DEBUG_LOG
                    Debug.Log("Time elapsed for world deserialization : " + watch.ElapsedMilliseconds + " ms");
#endif // DEBUG_LOG
                    m_awaitedInitMessage = init;
                    }
                });
                deserializeWorldThread.Start();
            }
        }

        public override void StateUpdate()
        {
            switch(m_currentSubState)
            {
                case SubState.SUBSTATE_WAITING_FOR_WORLD:

                    if (m_awaitedInitMessage != null)
                    {
#if DEBUG_LOG
                        Debug.Log("Client received confirmation that server is about to start game with " + m_awaitedInitMessage.PlayerCharacters.Value.Keys.Count + " players");
#endif // DEBUG_LOG

#if DEBUG_LOG
                        Debug.Log("Starting to load world.");
#endif // DEBUG_LOG
                        data.LoadingData.ServerInit = m_awaitedInitMessage;
                        m_currentSubState = SubState.SUBSTATE_GOING_TO_GAME;
                    }
                    break;
                case SubState.SUBSTATE_GOING_TO_GAME:
                    GoToGame();
                    break;
                case SubState.SUBSTATE_LEAVING_LOBBY:
                    BackToCharacterSelect();
                    break;
                case SubState.SUBSTATE_TRANSITION:
                    break;
                default:
                    break;
            }
        }

        private void GoToGame()
        {
            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            ClientStateManager.Instance.PushScene(m_clientPlayScene);
        }

        private void Init()
        {
            m_activeCharacterID = data.LoadingData.ActiveCharacterID;
            if (!string.IsNullOrEmpty(m_activeCharacterID))
            {
                m_awaitedInitMessage = null;
                m_clientCharacters?.Clear();
                Debug.Log("Entering lobby with character:" + m_activeCharacterID);
                m_server.TCPSend(new OnLobbyEnteredMessage(m_activeCharacterID).GetBytes());
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("No active character. Leaving lobby.");
#endif // DEBUG_LOG
            }
        }
        
        private void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Lobby : lost connection to game server. Leaving lobby.");
#endif // DEBUG_LOG
            m_currentSubState = SubState.SUBSTATE_LEAVING_LOBBY;
        }

        public void BackToCharacterSelect()
        {
            if (m_server.IsConnected())
            {
                m_server.TCPSend(new ClientLeavingMessage().GetBytes());
                m_server.Disconnect();
            }

            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            if (!ClientStateManager.Instance.BackToScene(m_characterSelectScene))
            {
                ClientStateManager.Instance.BackToScene(m_menuScene);
            }
        }
        
        protected override void StateUnload()
        {
            m_server.OnTCPReceive -= Receive;
            m_server.OnServerDisconnect -= OnDisconnect;
        }

        protected override void StatePause()
        {
            m_server.OnTCPReceive -= Receive;
            m_server.OnServerDisconnect -= OnDisconnect;
        }

        protected override void StateResume()
        {
            m_server.OnTCPReceive += Receive;
            m_server.OnServerDisconnect += OnDisconnect;
        }
    }   
}
