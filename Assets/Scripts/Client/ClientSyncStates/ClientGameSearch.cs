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
        
        protected override void StateLoad()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_INFO;

#if DEBUG_LOG
            Debug.Log("Initializing client state [init]");
#endif // DEBUG_LOG
        }

        private void Start()
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

            if (!data.LoadingData.GameID.Equals(string.Empty))
            {
                DispatcherService.RequestServerInfo(data.LoadingData.GameID, OnServerInfoReceived);
            }
            else
            {
                DispatcherService.RequestServerInfo(data.LoadingData.GameID, OnServerInfoReceived);
            }
        }

        private void OnServerInfoReceived(ServerInfo info)
        {
            data.LoadingData.GameID = info.GameID;
            EstablishConnectionToServer(info);
        }

        private void EstablishConnectionToServer(ServerInfo info)
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_CONNECTION;
            m_server.OnSuccessfulConnect += OnSuccessfulConnect;
            m_server.Connect(info);
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

        private void OnSuccessfulConnect()
        {
            m_server.OnSuccessfulConnect -= OnSuccessfulConnect;
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_STATUS;
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
        
        public void ReceivePacket(tcp.TCPToolkit.Packet packet)
        {
            if(m_currentSubState == SubState.SUBSTATE_WAITING_FOR_SERVER_STATUS)
            {
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

        protected override void StateUnload()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_INFO;
        }

        protected override void StatePause()
        {
        }

        protected override void StateResume()
        {
        }
    }
}

