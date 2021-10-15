using UnityEngine;
using System.Collections;
using ubv.udp.server;
using ubv.tcp.server;
using ubv.server.logic;
using System.Collections.Generic;
using ubv.udp;

namespace ubv.microservices
{
    abstract public class ServerConnectionManager : MonoBehaviour, IUDPServerReceiver, ITCPServerReceiver
    {
        protected TCPServer m_TCPServer;
        protected UDPServer m_UDPServer;

        protected readonly object m_lock = new object();
        protected readonly object m_connectionLock = new object();

        protected HashSet<int> m_pendingPlayers; // players awaiting UDP + TCP identification
        protected HashSet<int> m_connectedPlayers;

        private void Awake()
        {
            m_connectedPlayers = new HashSet<int>();
            m_pendingPlayers = new HashSet<int>();
        }

        private void Start()
        {
            m_TCPServer = ServerNetworkingManager.Instance.TCPServer;
            m_UDPServer = ServerNetworkingManager.Instance.UDPServer;
        }
        
        public void UDPReceive(UDPToolkit.Packet packet, int playerID)
        {
            if (m_connectedPlayers.Contains(playerID))
            {
                UDPConnectedReceive(packet, playerID);
            }
            else if (m_pendingPlayers.Contains(playerID))
            {
                IdentificationMessage identification = Serializable.CreateFromBytes<IdentificationMessage>(packet.Data.ArraySegment());
                if (identification != null)
                {
#if DEBUG_LOG
                    Debug.Log("Player " + playerID + " successfully connected and identified. Rejoining.");
#endif // DEBUG_LOG
                    m_connectedPlayers.Add(playerID);
                    m_currentState.OnPlayerConnect(playerID);
                    m_pendingPlayers.Remove(playerID);
                }
            }
        }

        public void TCPReceive(TCPToolkit.Packet packet, int playerID)
        {
            if (m_connectedPlayers.Contains(playerID))
            {
                m_currentState.TCPConnectedReceive(packet, playerID);
            }
        }

        public void OnTCPConnect(int playerID)
        {
            lock (m_connectionLock)
            {
                m_pendingPlayers.Add(playerID);
            }
        }

        public void OnTCPDisconnect(int playerID)
        {
            lock (m_connectionLock)
            {
                m_pendingPlayers.Remove(playerID);
                m_connectedPlayers.Remove(playerID);
            }
            m_currentState.OnPlayerDisconnect(playerID);
        }

        protected bool IsConnected(int playerID)
        {
            bool state = false;
            lock (m_connectionLock)
            {
                state = m_connectedPlayers.Contains(playerID);
            }
            return state;
        }

        protected abstract void OnPlayerConnect(int playerID);
        protected abstract void OnPlayerDisconnect(int playerID);

        protected abstract void UDPConnectedReceive(UDPToolkit.Packet packet, int playerID);
        protected abstract void TCPConnectedReceive(TCPToolkit.Packet packet, int playerID);
    }
}
