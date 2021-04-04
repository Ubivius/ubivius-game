using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;

namespace ubv.client.logic
{
    abstract public class ClientSyncState : MonoBehaviour
    {
        static protected int? m_playerID;

        static protected ClientSyncState m_currentState = null;

        static protected ClientSyncLogin    m_loginState;
        static protected ClientSyncInit     m_initState;
        static protected ClientSyncLobby    m_lobbyState;
        static protected ClientSyncPlay     m_playState;

        static protected tcp.client.TCPClient m_TCPClient;
        static protected udp.client.UDPClient m_UDPClient;
        static protected http.HTTPClient m_HTTPClient;
        static protected microservices.DispatcherMicroservice m_dispatcherService;
        static protected microservices.AuthenticationService m_authenticationService;

        static private bool m_isSetup = false;

        private void Awake()
        {
            StateAwake();
        }

        private void Start()
        {
            if (!m_isSetup)
            {
                m_TCPClient = ClientNetworkingManager.Instance.TCPClient;
                m_UDPClient = ClientNetworkingManager.Instance.UDPClient;
                m_HTTPClient = ClientNetworkingManager.Instance.HTTPClient;
                m_dispatcherService = ClientNetworkingManager.Instance.Dispatcher;
                m_authenticationService = ClientNetworkingManager.Instance.Authentication;
                m_isSetup = true;
            }
            StateStart();
        }

        private void Update()
        {
            if (m_currentState != this)
                return;

            StateUpdate();
        }

        private void FixedUpdate()
        {
            if (m_currentState != this)
                return;

            StateFixedUpdate();
        }

        protected virtual void StateAwake() { }
        protected virtual void StateStart() { }
        protected virtual void StateUpdate() { }
        protected virtual void StateFixedUpdate() { }

        protected readonly object m_lock = new object();
    }
}
