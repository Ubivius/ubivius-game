﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace UBV
{
    /// <summary>
    /// Wrapper 
    /// </summary>
    public class UDPClient : MonoBehaviour
    {
        [Header("Connection parameters")]
        [SerializeField] private string m_serverAddress;
        [SerializeField] private int m_port;
        
        private UDPToolkit.ConnectionData m_connectionData;
        private UdpClient m_client;
        private IPEndPoint m_server;

        private void Awake()
        {
            m_connectionData = new UDPToolkit.ConnectionData();

            m_client = new UdpClient();
            m_server = new IPEndPoint(IPAddress.Parse(m_serverAddress), m_port);
            m_client.Connect(m_server);
        }

        private void Start()
        {
            m_client.BeginReceive(EndReceiveCallback, m_client);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Send(byte[] data) // TODO: generic it then convert to bytes from T
        {
            
            byte[] bytes = m_connectionData.Send(data).ToBytes();
            m_client.BeginSend(bytes, bytes.Length, EndSendCallback, m_client);
            
        }

        private void EndSendCallback(System.IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            Debug.Log("Client sent " + c.EndSend(ar).ToString() + " bytes");
        }
        
        private void EndReceiveCallback(System.IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            byte[] bytes = c.EndReceive(ar, ref m_server);

            m_connectionData.Receive(UDPToolkit.Packet.PacketFromBytes(bytes));
            Debug.Log("Client received " + UDPToolkit.Packet.PacketFromBytes(bytes).ToString() + " packet bytes");

            c.BeginReceive(EndReceiveCallback, c);
        }
        
    }
}