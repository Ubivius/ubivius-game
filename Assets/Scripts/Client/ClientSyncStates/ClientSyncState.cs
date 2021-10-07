using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using ubv.microservices;

namespace ubv.client.logic
{
    abstract public class ClientSyncState : MonoBehaviour
    {
        static public int? PlayerID { get; protected set; }

        static public UserService.UserInfo UserInfo { get; protected set; }

        static protected ClientSyncState m_currentState = null;

        static protected ClientSyncLogin    m_loginState;
        static protected ClientSyncInit     m_initState;
        static protected ClientSyncLobby    m_lobbyState;
        static protected ClientSyncPlay     m_playState;

        static protected tcp.client.TCPClient m_TCPClient;
        static protected udp.client.UDPClient m_UDPClient;
        static protected http.client.HTTPClient m_HTTPClient;
        static protected DispatcherMicroservice m_dispatcherService;
        static protected AuthenticationService m_authenticationService;
        static protected CharacterDataService m_characterService;
        static protected UserService m_userService;

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
                m_characterService = ClientNetworkingManager.Instance.CharacterData;
                m_userService = ClientNetworkingManager.Instance.User;

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

        public UserService.UserInfo GetActiveUser()
        {
            return UserInfo;
        }
    }
}
