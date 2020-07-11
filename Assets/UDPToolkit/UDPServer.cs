using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace UBV {
    /// <summary>
    /// UDP SERVER CLASS - Wrapper around C# sockets
    /// https://www.winsocketdotnetworkprogramming.com/clientserversocketnetworkcommunication8d.html
    /// </summary>
    public class UDPServer : MonoBehaviour
    {
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
        
        private void Awake()
        {
            m_endPoints = new Dictionary<IPEndPoint, UdpClient>();
            m_clientConnections = new Dictionary<UdpClient, ClientConnection>();
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 9050);
            m_server = new UdpClient(localEndPoint);
            m_server.BeginReceive(EndReceiveCallback, m_server);
        }

        private void Update()
        {

        }

        private void Send(byte[] data, UdpClient clientConnection)
        {
            byte[] bytes = m_clientConnections[clientConnection].ConnectionData.Send(data).ToBytes();
            clientConnection.BeginSend(bytes, bytes.Length, EndSendCallback, clientConnection);
        }

        private void EndSendCallback(System.IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            Debug.Log("Server sent " + c.EndSend(ar).ToString() + "bytes");
        }

        private void EndReceiveCallback(System.IAsyncResult ar)
        {
            IPEndPoint clientEndPoint = new IPEndPoint(0, 0);
            UdpClient s = (UdpClient)ar.AsyncState;
            byte[] bytes = s.EndReceive(ar, ref clientEndPoint);
            
            // If client is not registered, create a new Socket 
            if(!m_endPoints.ContainsKey(clientEndPoint))
            {
                m_endPoints.Add(clientEndPoint, new UdpClient());
                m_endPoints[clientEndPoint].Connect(clientEndPoint);

                m_clientConnections.Add(m_endPoints[clientEndPoint], new ClientConnection());
            }

            m_clientConnections[m_endPoints[clientEndPoint]].ConnectionData.Receive(UDPToolkit.Packet.PacketFromBytes(bytes));
            Debug.Log("Server received "+ UDPToolkit.Packet.PacketFromBytes(bytes).ToString() + " packet bytes");
            Send(bytes, m_endPoints[clientEndPoint]);
            s.BeginReceive(EndReceiveCallback, s);
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