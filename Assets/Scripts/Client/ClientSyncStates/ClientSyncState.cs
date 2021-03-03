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

        [SerializeField] protected ClientSync m_clientSync;
        [SerializeField] protected tcp.client.TCPClient m_TCPClient;
        [SerializeField] protected udp.client.UDPClient m_UDPClient;
        
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

        protected virtual void StateUpdate() { }
        protected virtual void StateFixedUpdate() { }

        protected readonly object m_lock = new object();
    }
}
