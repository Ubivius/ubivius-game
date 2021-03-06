﻿using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using System.Net.Http;
using static ubv.microservices.DispatcherMicroservice;
using static ubv.microservices.CharacterDataService;
using UnityEngine.Events;

namespace ubv.client.logic
{
    public class ClientSyncInit : ClientSyncState, udp.client.IUDPClientReceiver, tcp.client.ITCPClientReceiver
    {
        [SerializeField] private string m_clientLobbyScene;

        private ServerInfo? m_cachedServerInfo;
        private bool m_connected;
        private bool m_waitingOnUDPResponse;
        private bool m_waitingOnTCPResponse;

        [SerializeField] private float m_reconnectTimerIntervalMS = 3000f;
        private float m_requestResendTimer;

        [SerializeField] private float m_UDPPingTimerIntervalMS = 500f;
        private float m_UPDPingTimer;
        
        private byte[] m_identificationMessageBytes;

        private bool m_readyToGoToLobby;
        private bool m_alreadyInLobby;
        private bool m_loadingLobby;

        static private CharacterData m_activeCharacter = null;
        
        protected override void StateAwake()
        {
            m_readyToGoToLobby = false;
            m_loadingLobby = false;
            ClientSyncState.m_initState = this;
            ClientSyncState.m_currentState = this;
            m_cachedServerInfo = null;
            m_alreadyInLobby = false;
            Init();
        }

        public void Init(bool clearServerInfo = true)
        {
#if DEBUG_LOG
            Debug.Log("Initializing client state [init]");
#endif // DEBUG_LOG

            m_connected = false;
            m_waitingOnUDPResponse = false;
            m_waitingOnTCPResponse = false;

            m_requestResendTimer = 0;
            m_identificationMessageBytes = new IdentificationMessage().GetBytes();
            m_TCPClient.SetPlayerID(PlayerID.Value);
            m_UDPClient.Subscribe(this);
            m_TCPClient.Subscribe(this);
            if (clearServerInfo)
            {
                m_cachedServerInfo = null;
            }
        }

        private void Start()
        {
            // try to fetch characters from microservice
            if (m_activeCharacter == null)
            { 
                m_characterService.GetCharacters(UserInfo.ID, OnCharactersFetchedFromService);
            }
        }

        private void OnCharactersFetchedFromService(CharacterData[] characters)
        {
            // for now, assume only one character 
            // take the only character available and treat it as active
            m_activeCharacter = characters[0];
#if DEBUG_LOG
            Debug.Log("Fetched character " + m_activeCharacter.Name + " from microservice.");
#endif //DEBUG_LOG
            
        }

        public CharacterData GetActiveCharacter()
        {
            return m_activeCharacter;
        }

        protected override void StateUpdate()
        {
            if (m_cachedServerInfo != null && !m_connected && !m_waitingOnUDPResponse && !m_waitingOnTCPResponse)
            {
                m_requestResendTimer += Time.deltaTime;
                if(m_requestResendTimer > m_reconnectTimerIntervalMS / 1000f)
                {
#if DEBUG_LOG
                    Debug.Log("Trying to reconnect to server : " + m_cachedServerInfo.Value.server_ip.ToString() + " ...");
#endif // DEBUG_LOG
                    m_requestResendTimer = 0;
                    EstablishConnectionToServer(m_cachedServerInfo.Value);
                }
            }

            lock (m_lock)
            {
                if (!m_waitingOnTCPResponse && !m_waitingOnUDPResponse && m_connected && !m_readyToGoToLobby)
                {
                    if (!m_loadingLobby)
                    {
                        m_readyToGoToLobby = true;
                    }
                }
            }

            if (m_connected && m_waitingOnUDPResponse)
            {
                m_UDPPingTimerIntervalMS += Time.deltaTime;
                if (m_UPDPingTimer > m_UDPPingTimerIntervalMS / 1000f)
                {
                    m_UPDPingTimer = 0;
                    m_UDPClient.Send(m_identificationMessageBytes, PlayerID.Value);
                }
            }

            if (m_readyToGoToLobby)
            {
                m_readyToGoToLobby = false;
                if (!m_alreadyInLobby)
                {
                    m_alreadyInLobby = true;
                    GoToLobby();
                }
                else if(!m_loadingLobby)
                {
                    SetLobbyStateActive();
                }
            }
        }

        public void SendConnectionRequestToServer()
        {
#if DEBUG_LOG
            Debug.Log("Sending connection request to dispatcher...");
#endif // DEBUG_LOG

            m_dispatcherService.RequestServerInfo(PlayerID.Value, OnServerInfoReceived);
        }

        private void OnServerInfoReceived(ServerInfo? info)
        {
            m_cachedServerInfo = info;
            EstablishConnectionToServer(m_cachedServerInfo.Value);
        }

        private void EstablishConnectionToServer(ServerInfo serverInfo)
        {
            if (!m_connected)
            {
#if DEBUG_LOG
                Debug.Log("Trying to establish connection to game server...");
#endif // DEBUG_LOG

                m_TCPClient.Connect(serverInfo.server_ip, serverInfo.tcp_port);
            }
#if DEBUG_LOG
            else
            {
                Debug.Log("Already connected to game server.");
            }
#endif // DEBUG_LOG
        }

        private void GoToLobby()
        {
            StartCoroutine(LoadLobbyCoroutine());
        }
        
        private IEnumerator LoadLobbyCoroutine()
        {
            m_loadingLobby = true;
            m_UDPClient.Unsubscribe(this);
            m_TCPClient.Unsubscribe(this);
            // animation petit cercle de load to scene
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync(m_clientLobbyScene);
            while (!loadLobby.isDone)
            {
                yield return null;
            }

            SetLobbyStateActive();
        }

        private void SetLobbyStateActive()
        {
            m_loadingLobby = false;
            ClientSyncState.m_lobbyState.Init(GetActiveCharacter().ID);
            ClientSyncState.m_currentState = ClientSyncState.m_lobbyState;
        }

        public void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Disconnected from server");
#endif // DEBUG_LOG
            m_connected = false;
        }

        public void OnSuccessfulConnect()
        {
#if DEBUG_LOG
            Debug.Log("Successful connection to server. Sending identification message with ID " + PlayerID.Value);
#endif // DEBUG_LOG

            lock (m_lock)
            {
                m_waitingOnUDPResponse = true;
                m_waitingOnTCPResponse = true;
                m_connected = true;
            }

            m_UDPClient.SetTargetServer(m_cachedServerInfo.Value.server_ip, m_cachedServerInfo.Value.udp_port);
            m_UDPClient.Send(m_identificationMessageBytes, PlayerID.Value);
            m_TCPClient.Send(m_identificationMessageBytes); // sends a ping to the server
        }

        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            ServerSuccessfulConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulConnectMessage>(packet.Data.ArraySegment());
            if (serverSuccessPing != null)
            {
                m_waitingOnUDPResponse = false;
#if DEBUG_LOG
                Debug.Log("Received UDP connection confirmation.");
#endif // DEBUG_LOG
            }
        }

        public void ReceivePacket(TCPToolkit.Packet packet)
        {
            ServerSuccessfulConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulConnectMessage>(packet.Data.ArraySegment());
            if (serverSuccessPing != null)
            {
                m_waitingOnTCPResponse = false;
#if DEBUG_LOG
                Debug.Log("Received TCP connection confirmation.");
#endif // DEBUG_LOG
            }
        }
    }   
}
