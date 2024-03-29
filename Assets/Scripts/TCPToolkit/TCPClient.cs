﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System;
using System.Threading.Tasks;

namespace ubv.tcp.client
{
    /// <summary>
    /// Wrapper around System.Net.Sockets.TcpClient. Manages client-side connection with server, with timeout and packet loss
    /// </summary>
    public class TCPClient : MonoBehaviour
    {
        protected readonly object m_lock = new object();

        private string m_serverAddress;
        private int m_serverPort;

        protected volatile bool m_exitSignal;

        private TcpClient m_client;
        private IPEndPoint m_server;

        private List<ITCPClientReceiver> m_receivers;
        private List<ITCPClientReceiver> m_receiversAwaitingSubscription;
        private List<ITCPClientReceiver> m_receiversAwaitingUnsubscription;
        private bool m_iteratingTroughReceivers;
        
        // Un gros buffer c'est le fun, on est pas très limités en taille
        private const int DATA_BUFFER_SIZE = 1024 * 1024 * 4;
        // pour workaround le unix shit
        private const int MAX_BYTES_READ = 4096;
        private Queue<byte[]> m_dataToSend;

        private volatile bool m_activeEndpoint;
        [SerializeField] int m_connectionTimeoutInMS = 5000;
        
        [SerializeField] private float m_connectionKeepAliveIntervalMS = 250;
        private byte[] m_keepAlivePacketBytes;
        private float m_connectionKeepAliveTimer;

        private int? m_playerID;

        private ManualResetEvent m_requestToSendEvent;

        private void Awake()
        {
            m_receivers = new List<ITCPClientReceiver>();
            m_receiversAwaitingSubscription = new List<ITCPClientReceiver>();
            m_receiversAwaitingUnsubscription = new List<ITCPClientReceiver>();
            m_exitSignal = false;
            m_dataToSend = new Queue<byte[]>();
            m_activeEndpoint = false;

            m_iteratingTroughReceivers = false;

            m_requestToSendEvent = new ManualResetEvent(false);

            m_keepAlivePacketBytes = new byte[0];
            m_connectionKeepAliveTimer = 0;
        }
        
        public void SetPlayerID(int playerID)
        {
            m_playerID = playerID;
        }

        private void CommThread()
        {
            m_client = new TcpClient();
            m_exitSignal = false;
            using (m_client)
            {
                try
                {
#if DEBUG_LOG
                    Debug.Log("Trying to connect to server...");
#endif // DEBUG_LOG
                    m_client.Connect(m_server);
                }
                catch (SocketException ex)
                {
                    Debug.Log(ex.Message);
                    m_iteratingTroughReceivers = true;
                    foreach (ITCPClientReceiver receiver in m_receivers)
                    {
                        receiver.OnFailureToConnect();
                    }
                    m_iteratingTroughReceivers = false;
                    return;
                }

                if (!m_client.Connected)
                {
                    m_iteratingTroughReceivers = true;
                    foreach (ITCPClientReceiver receiver in m_receivers)
                    {
                        receiver.OnFailureToConnect();
                    }
                    m_iteratingTroughReceivers = false;
                    return;
                }

                m_iteratingTroughReceivers = true;
                foreach (ITCPClientReceiver receiver in m_receivers)
                {
                    receiver.OnSuccessfulTCPConnect();
                }
                m_iteratingTroughReceivers = false;

                using (NetworkStream stream = m_client.GetStream())
                {
                    m_activeEndpoint = true;
                    HandleConnection(stream);
                    m_exitSignal = true;
                    stream.Close();
                }

                m_iteratingTroughReceivers = true;
                foreach (ITCPClientReceiver receiver in m_receivers)
                {
                    receiver.OnDisconnect();
                }
                m_iteratingTroughReceivers = false;
                m_client.Close();
            }
        }

        private void HandleConnection(NetworkStream stream)
        {
            Thread send = new Thread(SendingThread);
            send.Start(stream);

            Thread receive = new Thread(ReceivingThread);
            receive.Start(stream);

            receive.Join();
            send.Join();
        }

        private void ReceivingThread(object streamObj)
        {
            NetworkStream stream = (NetworkStream)streamObj;

            if (!stream.CanRead)
                return;
            
            byte[] bytes = new byte[DATA_BUFFER_SIZE];
            byte[] packetBytes = new byte[0];
            int totalPacketBytes = 0;
            int totalBytesReadBeforePacket = 0;

            bool readyToReadPacket = true;

            stream.ReadTimeout = m_connectionTimeoutInMS;

            while (!m_exitSignal && m_activeEndpoint && totalBytesReadBeforePacket >= 0)
            {
                if (readyToReadPacket)
                {
                    totalBytesReadBeforePacket = 0;
                    totalPacketBytes = 0;
                    readyToReadPacket = false;
                }
                
                // read from stream until we read a full packet
                try
                {
                    totalBytesReadBeforePacket += stream.Read(bytes, (totalBytesReadBeforePacket) % DATA_BUFFER_SIZE, MAX_BYTES_READ); ;
                }
                catch (IOException ex)
                {
                    Debug.Log(ex.Message);
                    m_activeEndpoint = false;
                    stream.Close();
                    break;
                }
                
                if (totalBytesReadBeforePacket > 0 && m_activeEndpoint)
                {
                    TCPToolkit.Packet packet = TCPToolkit.Packet.FirstPacketFromBytes(bytes.SubArray(0, totalBytesReadBeforePacket));
                    while (packet != null)
                    {
                        readyToReadPacket = true;
                        totalPacketBytes += packet.RawBytes.Length;
                        // broadcast reception to listeners
                        if (packet.Data.Length > 0) // if it's not a keep-alive packet
                        {
                            lock (m_lock)
                            {
                                m_iteratingTroughReceivers = true;
                                foreach (ITCPClientReceiver receiver in m_receivers)
                                {
                                    receiver.ReceivePacket(packet);
                                }
                                m_iteratingTroughReceivers = false;
                            }
                        }

                        if (totalBytesReadBeforePacket > totalPacketBytes)
                        {
                            packet = TCPToolkit.Packet.FirstPacketFromBytes(bytes.SubArray(totalPacketBytes, totalBytesReadBeforePacket - totalPacketBytes));
                        }
                        else
                        {
                            packet = null;
                        }
                    }
                }
            }
#if DEBUG_LOG
            Debug.Log("State at client receiving thread exit : Active endpoint ? " + m_activeEndpoint.ToString() + ", Exit signal ?" + m_exitSignal);
#endif // DEBUG_LOG
            m_requestToSendEvent.Set();
            m_activeEndpoint = false;
        }


        private void UpdateSubscriptions()
        {
            lock (m_lock)
            {
                if (!m_iteratingTroughReceivers && m_activeEndpoint)
                {
                    if (m_receiversAwaitingSubscription.Count > 0)
                    {
                        for (int i = 0; i < m_receiversAwaitingSubscription.Count; i++)
                        {
                            Subscribe(m_receiversAwaitingSubscription[i]);
                        }
                        m_receiversAwaitingSubscription.Clear();
                    }

                    if (m_receiversAwaitingUnsubscription.Count > 0)
                    {
                        for (int i = 0; i < m_receiversAwaitingUnsubscription.Count; i++)
                        {
                            Unsubscribe(m_receiversAwaitingUnsubscription[i]);
                        }
                        m_receiversAwaitingUnsubscription.Clear();
                    }
                }
            }
        }

        private void Update()
        {
            UpdateSubscriptions();
            ConnectionKeepAlive();
        }

        private void ConnectionKeepAlive()
        {
            m_connectionKeepAliveTimer += Time.deltaTime;
            if (m_connectionKeepAliveTimer > m_connectionKeepAliveIntervalMS / 1000f && m_activeEndpoint)
            {
                m_connectionKeepAliveTimer = 0;

                Send(m_keepAlivePacketBytes);
            }
        }

        private void SendingThread(object streamObj)
        {
            NetworkStream stream = (NetworkStream)streamObj;

            if (!stream.CanWrite)
                return;
            
            while (!m_exitSignal && m_activeEndpoint)
            {
                m_requestToSendEvent.WaitOne(m_connectionTimeoutInMS);
                m_requestToSendEvent.Reset();
                // write to stream (send to client)
                lock (m_lock)
                {
                    while (m_dataToSend.Count > 0 && m_activeEndpoint)
                    {
                        if(m_playerID == null)
                        {
#if DEBUG_LOG
                            Debug.Log("Player ID is not set. Cannot send to server.");
#endif // DEBUG_LOG
                            m_activeEndpoint = false;
                            break;
                        }

                        byte[] bytesToWrite = tcp.TCPToolkit.Packet.DataToPacket(m_dataToSend.Dequeue(), m_playerID.Value).RawBytes;
                        try
                        {
                            stream.Write(bytesToWrite, 0, bytesToWrite.Length);
                        }
                        catch (IOException ex)
                        {
                            Debug.Log(ex.Message);
                            m_activeEndpoint = false;
                            stream.Close();
                            break;
                        }
                    }
                }
            }
            Debug.Log("State at client sending thread exit : Active endpoint ? " + m_activeEndpoint.ToString() + ", Exit signal ?" + m_exitSignal);
            m_activeEndpoint = false;
        }

        private void OnDestroy()
        {
            m_exitSignal = true;
        }

        public void Send(byte[] data)
        {
            m_requestToSendEvent.Set();
            lock (m_lock)
            {
                m_dataToSend.Enqueue(data);
            }
        }

        public void Subscribe(ITCPClientReceiver receiver)
        {
            lock (m_lock)
            {
                if (m_iteratingTroughReceivers)
                {
                    m_receiversAwaitingSubscription.Add(receiver);
                }
                else if (!m_receivers.Contains(receiver))
                {
                    m_receivers.Add(receiver);
                }
            }
        }

        public void Unsubscribe(ITCPClientReceiver receiver)
        {
            lock (m_lock)
            {
                if (m_iteratingTroughReceivers)
                {
                    m_receiversAwaitingUnsubscription.Add(receiver);
                }
                else
                {
                    m_receivers.Remove(receiver);
                }
            }
        }

        public void Connect(string address, int port)
        {
            m_serverAddress = address;
            m_serverPort = port;
            m_server = new IPEndPoint(IPAddress.Parse(address), port);

            Thread thread = new Thread(new ThreadStart(CommThread));
            thread.Start();
        }

        public void Disconnect()
        {
            m_exitSignal = true;
        }
        
        public bool IsConnected()
        {
            return m_activeEndpoint;
        }
    }
}
