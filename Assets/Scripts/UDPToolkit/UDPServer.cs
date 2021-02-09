using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine.SceneManagement;

namespace ubv.udp.server
{
    /// <summary>
    /// Wrapper around System.Net.Sockets.UdpClient. 
    /// Manages server-side UDP connections with other clients
    /// + manages input messages from clients and computes their positions, then sends them back
    /// https://www.winsocketdotnetworkprogramming.com/clientserversocketnetworkcommunication8d.html
    /// </summary>
    public class UDPServer : MonoBehaviour
    {
        [SerializeField] int m_port = 9050;
        [SerializeField] float m_connectionTimeout = 10f;

        private Dictionary<IPEndPoint, UdpClient> m_endPoints;
        private Dictionary<UdpClient, ClientConnection> m_clientConnections;
        private UdpClient m_server;
        private float m_serverUptime = 0;

<<<<<<< HEAD
        private List<IServerReceiver> m_receivers = new List<IServerReceiver>();
=======
                private List<IUDPServerReceiver> m_receivers = new List<IUDPServerReceiver>();

                private List<IPAddress> m_registeredClients;
>>>>>>> origin/master

        /// <summary>
        /// Manages a specific client connection 
        /// </summary>
        private class ClientConnection
        {
            public float LastConnectionTime;
            public UDPToolkit.ConnectionData ConnectionData;

            public ClientConnection()
            {
                ConnectionData = new UDPToolkit.ConnectionData();
            }
        }

<<<<<<< HEAD
        private void Awake()
        {
            m_endPoints = new Dictionary<IPEndPoint, UdpClient>();
            m_clientConnections = new Dictionary<UdpClient, ClientConnection>();
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);

            m_server = new UdpClient(localEndPoint);

            Debug.Log("Launching server at " + localEndPoint.ToString());
=======
                private void Awake()
                {
                    m_registeredClients = new List<IPAddress>();
                    m_endPoints = new Dictionary<IPEndPoint, UdpClient>();
                    m_clientConnections = new Dictionary<UdpClient, ClientConnection>();
                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);

                    m_server = new UdpClient(localEndPoint);
#if DEBUG_LOG
                    Debug.Log("Launching UDP server at " + localEndPoint.ToString());
#endif // DEBUG_LOG
>>>>>>> origin/master

            m_server.BeginReceive(EndReceiveCallback, m_server);
        }

<<<<<<< HEAD
        private void Update()
        {
            // TODO: Adapt to use a better time tracking? System time?  
            m_serverUptime += Time.deltaTime;
            if (Time.frameCount % 10 == 0)
            {
                RemoveTimedOutClients();
            }
        }
=======
                private void Update()
                {
                    // TODO: Adapt to use a better time tracking? System time?  
                    m_serverUptime += Time.deltaTime;
                    if (Time.frameCount % 10 == 0)
                    {
                        // RemoveTimedOutClients();
                    }
                }
>>>>>>> origin/master

        private void RemoveTimedOutClients()
        {
            List<IPEndPoint> toRemove = new List<IPEndPoint>();
            // check if any client has disconnected (has not sent a packet in TIMEOUT seconds)
            foreach (IPEndPoint ep in m_endPoints.Keys)
            {
                if (m_serverUptime - m_clientConnections[m_endPoints[ep]].LastConnectionTime > m_connectionTimeout)
                {
<<<<<<< HEAD
                    Debug.Log("Client timed out. Disconnecting.");
                    toRemove.Add(ep);
=======
                    List<IPEndPoint> toRemove = new List<IPEndPoint>();
                    // check if any client has disconnected (has not sent a packet in TIMEOUT seconds)
                    foreach (IPEndPoint ep in m_endPoints.Keys)
                    {
                        if (m_serverUptime - m_clientConnections[m_endPoints[ep]].LastConnectionTime > m_connectionTimeout)
                        {
                            Debug.Log("Client timed out. Disconnecting.");
                            toRemove.Add(ep);
                        }
                    }

                    for (int i = 0; i < toRemove.Count; i++)
                    {
                        m_clientConnections.Remove(m_endPoints[toRemove[i]]);
                        m_endPoints.Remove(toRemove[i]);
                    }
>>>>>>> origin/master
                }
            }

<<<<<<< HEAD
            for (int i = 0; i < toRemove.Count; i++)
            {
                m_clientConnections.Remove(m_endPoints[toRemove[i]]);
                OnClientDisconnect(toRemove[i]);
                m_endPoints.Remove(toRemove[i]);
            }
        }

        public void Send(byte[] data, IPEndPoint clientIP)
        {
            Send(data, m_endPoints[clientIP]);
        }

        private void Send(byte[] data, UdpClient clientConnection)
        {
            try
            {
                byte[] bytes = m_clientConnections[clientConnection].ConnectionData.Send(data).ToBytes();
                clientConnection.BeginSend(bytes, bytes.Length, EndSendCallback, clientConnection);
            }
            catch (SocketException e)
            {
#if DEBUG
                Debug.Log("Server socket exception: " + e);
#endif // DEBUG
            }
        }

        private void EndSendCallback(System.IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
#if DEBUG_LOG
    Debug.Log("Server sent " + c.EndSend(ar).ToString() + " bytes");
=======
                public void Send(byte[] data, IPEndPoint clientIP)
                {
                    if (m_endPoints.ContainsKey(clientIP))
                    {
                        try
                        {
                            byte[] bytes = m_clientConnections[m_endPoints[clientIP]].ConnectionData.Send(data).RawBytes;
                            m_server.BeginSend(bytes, bytes.Length, clientIP, EndSendCallback, m_server);
                        }
                        catch (SocketException e)
                        {
#if DEBUG_LOG
                            Debug.Log("Server socket exception: " + e);
#endif // DEBUG_LOG
                        }
                    }
                    else
                    {
#if DEBUG_LOG
                        Debug.Log("Client " + clientIP.ToString() + " is not registered. Ignoring data send.");
#endif // DEBUG_LOG
                    }
                }

                private void EndSendCallback(System.IAsyncResult ar)
                {
                    UdpClient server = (UdpClient)ar.AsyncState;
#if DEBUG_LOG
            Debug.Log("Server sent " + server.EndSend(ar).ToString() + " bytes");
>>>>>>> origin/master
#endif // DEBUG_LOG
        }

<<<<<<< HEAD
        private void EndReceiveCallback(System.IAsyncResult ar)
        {
            // TODO: authentication
            IPEndPoint clientEndPoint = new IPEndPoint(0, 0);
            UdpClient server = (UdpClient)ar.AsyncState;
            byte[] bytes = server.EndReceive(ar, ref clientEndPoint);

            // If client is not registered, create a new Socket 
                    
            if (!m_endPoints.ContainsKey(clientEndPoint))
            {
                m_endPoints.Add(clientEndPoint, new UdpClient());
                m_endPoints[clientEndPoint].Connect(clientEndPoint);

                m_clientConnections.Add(m_endPoints[clientEndPoint], new ClientConnection());

                OnClientConnect(clientEndPoint);
            }
                    
            m_clientConnections[m_endPoints[clientEndPoint]].LastConnectionTime = m_serverUptime;

            UDPToolkit.Packet packet = UDPToolkit.Packet.PacketFromBytes(bytes);

                    
            if (m_clientConnections[m_endPoints[clientEndPoint]].ConnectionData.Receive(packet))
            {
                OnReceive(packet, clientEndPoint);
            }
                    

            server.BeginReceive(EndReceiveCallback, server);
        }
=======
                public void RegisterClient(IPAddress client)
                {
                    m_registeredClients.Add(client);
                }

                public void UnregisterClient(IPAddress client)
                {
                    m_registeredClients.Remove(client);
                }

                private void EndReceiveCallback(System.IAsyncResult ar)
                {
                    IPEndPoint clientEndPoint = new IPEndPoint(0, 0);
                    UdpClient server = (UdpClient)ar.AsyncState;
                    
                    byte[] bytes = server.EndReceive(ar, ref clientEndPoint);

                    if (!m_endPoints.ContainsKey(clientEndPoint) && m_registeredClients.Contains(clientEndPoint.Address))
                    {
                        m_endPoints.Add(clientEndPoint, new UdpClient());
                        m_endPoints[clientEndPoint].Connect(clientEndPoint);

                        m_clientConnections.Add(m_endPoints[clientEndPoint], new ClientConnection());
                    }

                    if (m_endPoints.ContainsKey(clientEndPoint))
                    {
                        m_clientConnections[m_endPoints[clientEndPoint]].LastConnectionTime = m_serverUptime;

                        UDPToolkit.Packet packet = UDPToolkit.Packet.PacketFromBytes(bytes);
                        
                        if (m_clientConnections[m_endPoints[clientEndPoint]].ConnectionData.Receive(packet))
                        {
                            OnReceive(packet, clientEndPoint);
                        }
                    }
                    else
                    {
#if DEBUG_LOG
                        Debug.Log("Received data from unregistered client. Ignoring.");
#endif // DEBUG_LOG
                    }
                    
                    server.BeginReceive(EndReceiveCallback, server);
                }
>>>>>>> origin/master

        private void OnReceive(UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
        {
            // TODO (maybe) : give up ticks and use only packet sequence number?

            for (int i = 0; i < m_receivers.Count; i++)
            {
                m_receivers[i].Receive(packet, clientEndPoint);
            }
        }

<<<<<<< HEAD
        public void AddReceiver(IServerReceiver receiver)
        {
            m_receivers.Add(receiver);
        }

        private void OnClientConnect(IPEndPoint clientIP)
        {
            for (int i = 0; i < m_receivers.Count; i++)
            {
                m_receivers[i].OnConnect(clientIP);
            }
        }

        private void OnClientDisconnect(IPEndPoint clientIP)
        {
            for (int i = 0; i < m_receivers.Count; i++)
            {
                m_receivers[i].OnDisconnect(clientIP);
=======
                public void Subscribe(IUDPServerReceiver receiver)
                {
                    m_receivers.Add(receiver);
                }

                public void Unsubscribe(IUDPServerReceiver receiver)
                {
                    m_receivers.Remove(receiver);
                }
>>>>>>> origin/master
            }
        }
    }
}
