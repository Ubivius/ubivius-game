using System.Threading;
using ubv.common.data;
using ubv.microservices;
using UnityEngine;
using static ubv.microservices.DispatcherMicroservice;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the transition state between character select and
    /// game. Goes to a game lobby OR rejoins a current game.
    /// </summary>
    public class ClientGameSearch : ClientSyncState
    {
        private enum SubState
        {
            SUBSTATE_WAITING_FOR_SERVER_INFO,
            SUBSTATE_WAITING_FOR_SERVER_CONNECTION,
            SUBSTATE_WAITING_FOR_SERVER_STATUS,
            SUBSTATE_WAITING_FOR_REJOIN,
            SUBSTATE_GOING_TO_LOBBY,
            SUBSTATE_GOING_TO_GAME,
            SUBSTATE_GOING_BACK,
            SUBSTATE_TRANSITION,
        }

        [SerializeField] private string m_clientGameScene;
        [SerializeField] private string m_clientLobbyScene;
        
        
        [SerializeField] private float m_dispatcherTimeout = 10.0f;
        private float m_dispatcherTimeoutTimer;
        
        private SubState m_currentSubState;
        private bool m_subscribed;
        
        protected override void StateLoad()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_INFO;

#if DEBUG_LOG
            Debug.Log("Initializing client state [init]");
#endif // DEBUG_LOG
            m_subscribed = false;
            SubscribeToServer();
        }

        private void SubscribeToServer()
        {
            if (!m_subscribed)
            {
                m_server.OnSuccessfulConnect += OnSuccessfulConnect;
                m_server.OnServerDisconnect += OnDisconnect;
                m_server.OnFailureToConnect += OnFailureToConnect;
                m_server.OnTCPReceive += Receive;
                m_subscribed = true;
            }
        }

        private void UnsubscribeFromServer()
        {
            if (m_subscribed)
            {
                m_server.OnSuccessfulConnect -= OnSuccessfulConnect;
                m_server.OnServerDisconnect -= OnDisconnect;
                m_server.OnFailureToConnect -= OnFailureToConnect;
                m_server.OnTCPReceive -= Receive;
                m_subscribed = false;
            }
        }

        public override void OnStart()
        {
            RequestServerInfo();
        }

        public override void StateUpdate()
        {
            switch (m_currentSubState)
            {
                case SubState.SUBSTATE_WAITING_FOR_SERVER_INFO:
                    m_dispatcherTimeoutTimer += Time.deltaTime;
                    if (m_dispatcherTimeoutTimer > m_dispatcherTimeout)
                    {
#if DEBUG_LOG
                        Debug.Log("Server info cannot be found.");
#endif // DEBUG_LOG
                        data.ClientCacheData.SaveCache(string.Empty);
                        GoBackToPreviousState();
                    }
                    break;
                case SubState.SUBSTATE_WAITING_FOR_SERVER_CONNECTION:
                    break;
                case SubState.SUBSTATE_WAITING_FOR_REJOIN:
                    break;
                case SubState.SUBSTATE_GOING_TO_LOBBY:
                    GoToLobby();
                    break;
                case SubState.SUBSTATE_GOING_TO_GAME:
                    GoToGame();
                    break;
                case SubState.SUBSTATE_GOING_BACK:
                    GoBackToPreviousState();
                    break;
                case SubState.SUBSTATE_WAITING_FOR_SERVER_STATUS:
                    break;
                default:
                    break;
            }
        }

        public void RequestServerInfo()
        {
#if DEBUG_LOG
            Debug.Log("Sending server info request to dispatcher...");
#endif // DEBUG_LOG

            if (!string.IsNullOrEmpty(data.LoadingData.GameID))
            {
                DispatcherService.RequestServerInfo(data.LoadingData.GameID, OnServerInfoReceived, OnDispatcherFail);
            }
            else
            {
                DispatcherService.RequestNewServerInfo(OnServerInfoReceived, OnDispatcherFail);
            }
        }

        private void OnDispatcherFail(string message)
        {
#if DEBUG_LOG
            Debug.LogWarning("Could not get server info: " + message);
#endif // DEBUG_LOG

            OnFailureToConnect();
        }

        private void OnServerInfoReceived(ServerInfo info)
        {
            Debug.Log("Server info : " + info.GameID + ", " + info.ServerTCPAddress);
            data.LoadingData.GameID = info.GameID;
            data.ClientCacheData.SaveCache(info.GameID);
            EstablishConnectionToServer(info);
        }

        private void EstablishConnectionToServer(ServerInfo info)
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_CONNECTION;
            m_server.Connect(info);
        }
        
        private void OnSuccessfulConnect()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_STATUS;
            m_server.TCPSend(new ServerStatusMessage(CurrentUser.ID).GetBytes());
        }

        private void OnFailureToConnect()
        {
            data.ClientCacheData.SaveCache(string.Empty);
            m_currentSubState = SubState.SUBSTATE_GOING_BACK;
        }

        private void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Disconnected from server");
#endif // DEBUG_LOG
            m_currentSubState = SubState.SUBSTATE_GOING_BACK;
        }
        
        private void Receive(tcp.TCPToolkit.Packet packet)
        {
            if(m_currentSubState == SubState.SUBSTATE_WAITING_FOR_SERVER_STATUS)
            {
                ServerStatusMessage status = common.serialization.IConvertible.CreateFromBytes<ServerStatusMessage>(packet.Data.ArraySegment());
                if(status != null)
                {
                    data.LoadingData.GameChatID = status.GameChatID.Value;
                    if (!status.PlayerID.Value.Equals(CurrentUser.ID))
                    {
#if DEBUG_LOG
                        Debug.LogError("Mismatch between server status message requests");
#endif // DEBUG_LOG
                        return;
                    }

                    if (status.IsInServer.Value 
                        && string.IsNullOrEmpty(data.LoadingData.ActiveCharacterID)
                        && !string.IsNullOrEmpty(status.CharacterID.Value))
                    {
                        data.LoadingData.ActiveCharacterID = status.CharacterID.Value;
                    }

                    if (status.IsInServer.Value
                        && string.IsNullOrEmpty(data.LoadingData.ActiveCharacterID)
                        && string.IsNullOrEmpty(status.CharacterID.Value))
                    {
#if DEBUG_LOG
                        Debug.LogWarning("Player has no character. Back to menu.");
#endif // DEBUG_LOG
                        data.ClientCacheData.SaveCache(string.Empty);
                        data.LoadingData.GameID = string.Empty;
                        m_currentSubState = SubState.SUBSTATE_GOING_BACK;
                        return;
                    }

                    if (status.IsInServer.Value && status.GameStatus.Value == (uint)ServerStatusMessage.ServerStatus.STATUS_GAME)
                    {
                        m_currentSubState = SubState.SUBSTATE_WAITING_FOR_REJOIN;
                        m_server.TCPSend(new ServerRejoinGameDemand().GetBytes());
                        return;
                    }

                    if (status.AcceptsNewPlayers.Value && status.GameStatus.Value == (uint)ServerStatusMessage.ServerStatus.STATUS_LOBBY)
                    {
                        m_currentSubState = SubState.SUBSTATE_GOING_TO_LOBBY;
                        return;
                    }

#if DEBUG_LOG
                    Debug.LogError("Could not join game server.");
#endif // DEBUG_LOG

                    OnFailureToConnect();
                    return;
                }
            }
            else if (m_currentSubState == SubState.SUBSTATE_WAITING_FOR_REJOIN)
            {
                Thread deserializeWorldThread = new Thread(() =>
                {
                    ServerInitMessage init = common.serialization.IConvertible.CreateFromBytes<ServerInitMessage>(packet.Data.ArraySegment());
                    if (init != null)
                    {
#if DEBUG_LOG
                        Debug.Log("Player is in server game. Going to game.");
#endif

                        data.LoadingData.ServerInit = init;
                        m_currentSubState = SubState.SUBSTATE_GOING_TO_GAME;
                        return;
                    }
                });
                deserializeWorldThread.Start();
            }
        }
        
        private void GoToLobby()
        {
            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            ClientStateManager.Instance.PushScene(m_clientLobbyScene);
        }

        private void GoToGame()
        {
            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            ClientStateManager.Instance.PushScene(m_clientGameScene);
        }

        private void GoBackToPreviousState()
        {
            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            ClientStateManager.Instance.PopState();
        }

        protected override void StateUnload()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_INFO;
            UnsubscribeFromServer();
        }

        protected override void StatePause()
        {
            UnsubscribeFromServer();
        }

        protected override void StateResume()
        {
            SubscribeToServer();
        }
    }
}

