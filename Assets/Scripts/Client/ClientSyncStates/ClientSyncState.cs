﻿using UnityEngine;
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
        static public UserInfo UserInfo { get; protected set; }
        
        static protected tcp.client.TCPClient m_TCPClient;
        static protected udp.client.UDPClient m_UDPClient;
        static protected http.client.HTTPClient m_HTTPClient;
        static protected DispatcherMicroservice m_dispatcherService;
        static protected SocialServicesController m_socialServices;
        static protected CharacterDataService m_characterService;
        static protected ClientStateManager m_stateManager;

        static public void InitDependencies()
        {
            m_TCPClient = ClientNetworkingManager.Instance.TCPClient;
            m_UDPClient = ClientNetworkingManager.Instance.UDPClient;
            m_HTTPClient = ClientNetworkingManager.Instance.HTTPClient;
            m_dispatcherService = ClientNetworkingManager.Instance.Dispatcher;
            m_socialServices = ClientNetworkingManager.Instance.SocialServices;
            m_characterService = ClientNetworkingManager.Instance.CharacterData;
            m_stateManager = ClientStateManager.Instance;
        }

        protected readonly object m_lock = new object();
        
        public virtual void StateLoad() { }
        
        public virtual void StateUpdate() { }

        public virtual void StateFixedUpdate() { }

        public virtual void StateUnload() { }

    }
}
