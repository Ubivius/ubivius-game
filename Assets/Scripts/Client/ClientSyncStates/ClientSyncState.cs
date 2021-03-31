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
        
        protected tcp.client.TCPClient  m_TCPClient;
        protected udp.client.UDPClient  m_UDPClient;
        protected http.HTTPClient       m_HTTPClient;

        private void Awake()
        {
            StateAwake();
        }

        private void Start()
        {
            m_TCPClient = ClientNetworkingManager.Instance.TCPClient;
            m_UDPClient = ClientNetworkingManager.Instance.UDPClient;
            m_HTTPClient = ClientNetworkingManager.Instance.HTTPClient;
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
