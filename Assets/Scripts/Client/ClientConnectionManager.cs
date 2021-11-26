using UnityEngine;
using System.Collections;
using ubv.tcp;
using ubv.udp;
using UnityEngine.Events;
using ubv.common.data;
using ubv.tcp.client;

namespace ubv.client
{
    public class ClientConnectionManager : MonoBehaviour, tcp.client.ITCPClientReceiver, udp.client.IUDPClientReceiver
    {
        [SerializeField]
        private tcp.client.TCPClient m_TCPClient;
        [SerializeField]
        private udp.client.UDPClient m_UDPClient;

        private microservices.ServerInfo m_cachedServerInfo;
        private byte[] m_identificationMessageBytes;

        public bool AutoReconnect;
        
        private int? m_playerID;

        private bool m_tryToReconnect;

        [SerializeField] private float m_TCPTimeout = 10.0f;
        private float m_TCPTimeoutTimer;

        [SerializeField] private float m_UDPTimeout = 10.0f;
        private float m_UDPTimeoutTimer;

        [SerializeField] private float m_UDPPingTimerIntervalMS = 500f;
        private float m_UDPPingTimer;
        
        private enum SubState
        {
            SUBSTATE_IDLE,
            SUBSTATE_WAITING_FOR_TCP,
            SUBSTATE_WAITING_FOR_UDP,
            SUBSTATE_CONNECTED,
            SUBSTATE_RECONNECTING
        }

        private SubState m_currentSubState;

        [SerializeField] private float m_reconnectTryIntervalMS = 2000;
        private float m_reconnectTryTimer;

        public UnityAction OnFailureToConnect;
        public UnityAction OnSuccessfulConnect;
        public UnityAction OnServerDisconnect;

        public UnityAction<UDPToolkit.Packet> OnUDPReceive;
        public UnityAction<TCPToolkit.Packet> OnTCPReceive;

        private void Start()
        {
            m_currentSubState = SubState.SUBSTATE_IDLE;
            m_cachedServerInfo = null;
            m_identificationMessageBytes = new IdentificationMessage().GetBytes();
            m_TCPClient.Subscribe(this);
            m_UDPClient.Subscribe(this);
            m_TCPTimeoutTimer = 0;
            m_UDPTimeoutTimer = 0;
        }

        // Update is called once per frame
        void Update()
        {
            switch (m_currentSubState)
            {
                case SubState.SUBSTATE_IDLE:
                    break;
                case SubState.SUBSTATE_WAITING_FOR_TCP:
                    m_TCPTimeoutTimer += Time.deltaTime;
                    if (m_TCPTimeoutTimer > m_TCPTimeout)
                    {
#if DEBUG_LOG
                        Debug.Log("Cannot connect to TCP Server");
#endif // DEBUG_LOG
                        OnFailureToConnect?.Invoke();
                        m_currentSubState = SubState.SUBSTATE_IDLE;
                        m_TCPTimeoutTimer = 0;
                    }
                    break;
                case SubState.SUBSTATE_WAITING_FOR_UDP:
                    if (m_TCPClient.IsConnected())
                    {
                        m_UDPPingTimer += Time.deltaTime;
                        if (m_UDPPingTimer > m_UDPPingTimerIntervalMS / 1000f)
                        {
                            m_UDPPingTimer = 0;
                            SendUDPIdentificationPing();
                        }
                    }

                    m_UDPTimeoutTimer += Time.deltaTime;
                    if (m_UDPTimeoutTimer > m_UDPTimeout)
                    {
#if DEBUG_LOG
                        Debug.Log("Cannot connect to UDP Server");
#endif // DEBUG_LOG
                        OnFailureToConnect?.Invoke();
                        m_currentSubState = SubState.SUBSTATE_IDLE;
                        m_UDPTimeoutTimer = 0;
                    }
                    break;
            }
        }

        public void Disconnect()
        {
            m_TCPClient.Disconnect();
        }

        public void OnDisconnect()
        {
            OnServerDisconnect?.Invoke();
            data.LoadingData.ActiveCharacterID = string.Empty;
            data.LoadingData.GameChatID = string.Empty;
            data.LoadingData.GameID = string.Empty;
            data.LoadingData.ServerInit = null;
            //data.LoadingData.GameStats = null;
            m_currentSubState = SubState.SUBSTATE_IDLE;
        }

        public void OnSuccessfulTCPConnect()
        {
            m_tryToReconnect = true;
        }

        public bool IsConnected()
        {
            return m_TCPClient.IsConnected();
        }

        public void TCPSend(byte[] bytes)
        {
            m_TCPClient.Send(bytes);
        }

        public void UDPSend(byte[] bytes)
        {
            m_UDPClient.Send(bytes, m_playerID.Value);
        }

        public void ReceivePacket(TCPToolkit.Packet packet)
        {
            if (m_currentSubState == SubState.SUBSTATE_WAITING_FOR_TCP)
            {
                ServerSuccessfulTCPConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulTCPConnectMessage>(packet.Data.ArraySegment());
                if (serverSuccessPing != null)
                {
#if DEBUG_LOG
                    Debug.Log("Received TCP connection confirmation (via TCP).");
#endif // DEBUG_LOG
                    m_currentSubState = SubState.SUBSTATE_WAITING_FOR_UDP;
                    m_UDPClient.SetTargetServer(m_cachedServerInfo.ServerUDPAddress, m_cachedServerInfo.ServerUDPPort);
                }
            }
            else if (m_currentSubState == SubState.SUBSTATE_WAITING_FOR_UDP)
            {
                ServerSuccessfulUDPConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulUDPConnectMessage>(packet.Data.ArraySegment());
                if (serverSuccessPing != null)
                {
#if DEBUG_LOG
                    Debug.Log("Received UDP connection confirmation (via TCP).");
#endif // DEBUG_LOG
                    m_currentSubState = SubState.SUBSTATE_CONNECTED;
                    OnSuccessfulConnect();
                }
            }
            else if (m_currentSubState == SubState.SUBSTATE_CONNECTED)
            {
                OnTCPReceive?.Invoke(packet);
            }
        }

        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            if (m_currentSubState == SubState.SUBSTATE_CONNECTED)
            {
                OnUDPReceive?.Invoke(packet);
            }
        }
        
        public void Reconnect()
        {
#if DEBUG_LOG
            Debug.Log("Trying to reconnect...");
#endif // DEBUG_LOG

            if (m_cachedServerInfo == null)
            {
#if DEBUG_LOG
                Debug.Log("No previous connection has been made.");
#endif // DEBUG_LOG
            }
            else
            {
                Connect(m_cachedServerInfo);
            }
        }

        public void Connect(microservices.ServerInfo info)
        {
            if(m_currentSubState == SubState.SUBSTATE_CONNECTED)
            {
                OnSuccessfulConnect?.Invoke();
                return;
            }

            if(m_currentSubState != SubState.SUBSTATE_RECONNECTING && m_currentSubState != SubState.SUBSTATE_IDLE)
            {
                return;
            }
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_TCP;
            m_cachedServerInfo = info;
            m_TCPClient.SetPlayerID(m_playerID.Value);
            m_TCPClient.Connect(m_cachedServerInfo.ServerTCPAddress, m_cachedServerInfo.ServerTCPPort);
            m_UDPClient.SetTargetServer(m_cachedServerInfo.ServerUDPAddress, m_cachedServerInfo.ServerUDPPort);
        }

        private void ReconnectCheck()
        {
            if (!m_currentSubState.Equals(SubState.SUBSTATE_IDLE))
            {
                return;
            }

            if (!m_TCPClient.IsConnected() && AutoReconnect && m_tryToReconnect)
            {
                m_reconnectTryTimer += Time.deltaTime;
                if (m_reconnectTryTimer * 1000 > m_reconnectTryIntervalMS)
                {
#if DEBUG_LOG
                    Debug.Log("Trying to reconnect to server...");
#endif //DEBUG_LOG
                    Reconnect();
                    m_reconnectTryTimer = 0;
                }
            }
        }

        public void SetPlayerID(int id)
        {
            m_playerID = id;
        }

        private void SendUDPIdentificationPing()
        {
            m_UDPClient.Send(m_identificationMessageBytes, m_playerID.Value);
        }

        void ITCPClientReceiver.OnFailureToConnect()
        {
            OnFailureToConnect?.Invoke();
        }
    }
}

