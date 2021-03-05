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
        static public ClientSyncState CurrentState = null;

        static public ClientSyncInit InitState;
        static public ClientSyncLoadWorld LoadWorldState;
        static public ClientSyncPlay PlayState;

        protected ClientSync m_clientSync;
        protected tcp.client.TCPClient m_TCPClient;
        protected udp.client.UDPClient m_UDPClient;

        private void Awake()
        {
            StateAwake();
        }

        private void Start()
        {
            m_clientSync = ClientNetworkingManager.Instance.ClientSync;
            m_TCPClient = ClientNetworkingManager.Instance.TCPClient;
            m_UDPClient = ClientNetworkingManager.Instance.UDPClient;
            StateStart();
        }

        private void Update()
        {
            if (CurrentState != this)
                return;

            StateUpdate();
        }

        private void FixedUpdate()
        {
            if (CurrentState != this)
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
