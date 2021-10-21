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
        static private ServerState m_currentState = null;
        static protected GameplayState m_gameplayState;
        static protected GameCreationState m_gameCreationState;

        static protected ServerConnectionManager m_serverConnection;

        protected object m_lock = new object();
        
        private void Awake()
        {
            m_serverConnection = ServerNetworkingManager.Instance.ServerConnection;
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

        protected void ChangeState(ServerState newState)
        {
            if (m_currentState != null)
            {
                m_serverConnection.OnPlayerConnect -= m_currentState.OnPlayerConnect;
                m_serverConnection.OnPlayerDisconnect -= m_currentState.OnPlayerDisconnect;
                m_serverConnection.OnTCPPacketFor -= m_currentState.OnTCPReceiveFrom;
                m_serverConnection.OnUDPPacketFor -= m_currentState.OnUDPReceiveFrom;
            }

            m_currentState = newState;

            m_serverConnection.OnPlayerConnect += m_currentState.OnPlayerConnect;
            m_serverConnection.OnPlayerDisconnect += m_currentState.OnPlayerDisconnect;
            m_serverConnection.OnTCPPacketFor += m_currentState.OnTCPReceiveFrom;
            m_serverConnection.OnUDPPacketFor += m_currentState.OnUDPReceiveFrom;
        }

        protected virtual void StateAwake() { }
        protected virtual void StateStart() { }
        protected virtual void StateUpdate() { }
        protected virtual void StateFixedUpdate() { }

        protected abstract void OnPlayerConnect(int playerID);
        protected abstract void OnPlayerDisconnect(int playerID);

        protected virtual void OnTCPReceiveFrom(TCPToolkit.Packet packet, int playerID) { }
        protected virtual void OnUDPReceiveFrom(UDPToolkit.Packet packet, int playerID) { }

    }
}
