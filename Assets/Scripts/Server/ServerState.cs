using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;
using ubv.common.data;
using ubv.common.serialization;
using ubv.udp.server;
using ubv.tcp.server;

namespace ubv.server.logic
{
    // TO READ https://fabiensanglard.net/quake3/network.php
    // and maybe https://thad.frogley.info/w/gfg08/gfg08.pdf

    abstract public class ServerState : MonoBehaviour
    {
        static protected ServerState m_currentState = null;
        static protected GameplayState m_gameplayState;
        static protected GameCreationState m_gameCreationState;
        
        private void Awake()
        {
            StateAwake();
        }

        private void Start()
        {
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
        
    }
}
