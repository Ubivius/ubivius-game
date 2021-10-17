using UnityEngine;
using System.Collections;
using ubv.udp.server;
using ubv.tcp.server;
using ubv.server.logic;
using System.Collections.Generic;
using ubv.udp;
using ubv.tcp;
using ubv.common.data;
using ubv.common.serialization;
using UnityEngine.Events;

namespace ubv.server
{
    public class ServerConnectionManager : MonoBehaviour, IUDPServerReceiver, ITCPServerReceiver
    {
        public TCPServer TCPServer;
        public UDPServer UDPServer;

        protected readonly object m_lock = new object();
        protected readonly object m_connectionLock = new object();

        protected HashSet<int> m_pendingPlayers; // players awaiting UDP + TCP identification
        protected HashSet<int> m_connectedPlayers;

        public UnityAction<int> OnPlayerConnect;
        public UnityAction<int> OnPlayerDisconnect;

        public UnityAction<UDPToolkit.Packet, int> OnUDPPacketFor;
        public UnityAction<TCPToolkit.Packet, int> OnTCPPacketFor;

        private void Awake()
        {
            m_connectedPlayers = new HashSet<int>();
            m_pendingPlayers = new HashSet<int>();
        }

        private void Start()
        {
            TCPServer = ServerNetworkingManager.Instance.TCPServer;
            UDPServer = ServerNetworkingManager.Instance.UDPServer;
            TCPServer.Subscribe(this);
            UDPServer.Subscribe(this);
            UDPServer.AcceptNewClients = true;
        }
        
        public void UDPReceive(UDPToolkit.Packet packet, int playerID)
        {
            if (m_connectedPlayers.Contains(playerID))
            {
                OnUDPPacketFor.Invoke(packet, playerID);
            }
            else if (m_pendingPlayers.Contains(playerID))
            {
                IdentificationMessage identification = Serializable.CreateFromBytes<IdentificationMessage>(packet.Data.ArraySegment());
                if (identification != null)
                {
#if DEBUG_LOG
                    Debug.Log("Player " + playerID + " successfully connected and identified with UDP and TCP. Rejoining.");
#endif // DEBUG_LOG
                    
                    ServerSuccessfulUDPConnectMessage serverSuccessPing = new ServerSuccessfulUDPConnectMessage();
                    TCPServer.Send(serverSuccessPing.GetBytes(), playerID);

                    m_connectedPlayers.Add(playerID);
                    OnPlayerConnect.Invoke(playerID);
                    m_pendingPlayers.Remove(playerID);
                }
            }
        }

        public void TCPReceive(TCPToolkit.Packet packet, int playerID)
        {
            if (m_connectedPlayers.Contains(playerID))
            {
                OnTCPPacketFor.Invoke(packet, playerID);
            }
        }

        public void OnTCPConnect(int playerID)
        {
            lock (m_connectionLock)
            {
                m_pendingPlayers.Add(playerID);
                TCPServer.Send(new ServerSuccessfulTCPConnectMessage().GetBytes(), playerID);
            }
        }

        public void OnTCPDisconnect(int playerID)
        {
            lock (m_connectionLock)
            {
                m_pendingPlayers.Remove(playerID);
                m_connectedPlayers.Remove(playerID);
            }
            OnPlayerDisconnect.Invoke(playerID);
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
    }
}
