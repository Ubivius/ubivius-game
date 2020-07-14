using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace UBV {
    /// <summary>
    /// UDP SERVER CLASS - Wrapper around C# sockets
    /// https://www.winsocketdotnetworkprogramming.com/clientserversocketnetworkcommunication8d.html
    /// </summary>
    public class UDPServer : MonoBehaviour
    {
        [SerializeField] int m_port = 9050;
        [SerializeField] float m_connectionTimeout = 10f;

        private class ClientConnection
        {
            public float LastConnectionTime;
            public UDPToolkit.ConnectionData ConnectionData;

            public ClientConnection()
            {
                ConnectionData = new UDPToolkit.ConnectionData();
            }
        }
        
        private Dictionary<IPEndPoint, UdpClient> m_endPoints;
        private Dictionary<UdpClient, ClientConnection> m_clientConnections;
        UdpClient m_server;
        private float m_serverUptime = 0;
        
        private void Awake()
        {
            m_endPoints = new Dictionary<IPEndPoint, UdpClient>();
            m_clientConnections = new Dictionary<UdpClient, ClientConnection>();
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);
            m_server = new UdpClient(localEndPoint);
            m_server.BeginReceive(EndReceiveCallback, m_server);
        }

        private void Update()
        {
            m_serverUptime += Time.deltaTime;
            if(Time.frameCount % 10 == 0)
            {
                RemoveTimedOutClients();
            }
        }

        private void RemoveTimedOutClients()
        {
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
                Debug.Log("Server socket exception: " + e);
            }
        }

        private void EndSendCallback(System.IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            //Debug.Log("Server sent " + c.EndSend(ar).ToString() + " bytes");
        }

        private void EndReceiveCallback(System.IAsyncResult ar)
        {
            IPEndPoint clientEndPoint = new IPEndPoint(0, 0);
            UdpClient server = (UdpClient)ar.AsyncState;
            byte[] bytes = server.EndReceive(ar, ref clientEndPoint);
            
            // If client is not registered, create a new Socket 
            if(!m_endPoints.ContainsKey(clientEndPoint))
            {
                m_endPoints.Add(clientEndPoint, new UdpClient());
                m_endPoints[clientEndPoint].Connect(clientEndPoint);

                m_clientConnections.Add(m_endPoints[clientEndPoint], new ClientConnection());
            }

            m_clientConnections[m_endPoints[clientEndPoint]].LastConnectionTime = m_serverUptime;

            UDPToolkit.Packet packet = UDPToolkit.Packet.PacketFromBytes(bytes);
            m_clientConnections[m_endPoints[clientEndPoint]].ConnectionData.Receive(packet);
            Debug.Log("Server received " + packet.ToString() + " packet bytes");

            // Send back to client for ACK

            // introdude random delay
            Thread.Sleep(100);

            Send(packet.Data, m_endPoints[clientEndPoint]);
            server.BeginReceive(EndReceiveCallback, server);
        }

        public void OnReceive(UDPToolkit.Packet packet)
        {
            Debug.Log("Received in server " + packet.ToString());
        }

        private void OnReceive(UDPToolkit.Packet packet, UdpClient source)
        {
            if (!m_clientConnections.ContainsKey(source))
            {
                m_clientConnections.Add(source, new ClientConnection());
            }
            m_clientConnections[source].LastConnectionTime = Time.time;

            m_clientConnections[source].ConnectionData.Receive(packet);

            Send(packet.Data, source);
        }
    }
}