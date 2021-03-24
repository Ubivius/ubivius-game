﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.IO;

namespace ubv.tcp.server
{
    /// <summary>
    /// Wrapper around System.Net.Sockets.TcpClient. 
    /// Manages server-side TCP connections with other clients
    /// https://www.codeproject.com/Articles/5270779/High-Performance-TCP-Client-Server-using-TCPListen 
    /// https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=net-5.0
    /// </summary>
    public class TCPServer : MonoBehaviour
    {
        protected readonly object m_lock = new object();

        [SerializeField] int m_port = 9051;
        [SerializeField] int m_connectionTimeoutInMS;
        [SerializeField] int m_maxConcurrentListeners = 6;
                
        private TcpListener m_tcpListener;

        protected bool m_exitSignal;

        private const int DATA_BUFFER_SIZE = 1024*1024;
                
        protected List<Task> m_tcpClientTasks;

        private Dictionary<IPEndPoint, TcpClient> m_clientConnections;
        private Dictionary<IPEndPoint, Queue<byte[]>> m_dataToSend;
        private Dictionary<IPEndPoint, bool> m_activeEndpoints;
        private Dictionary<IPEndPoint, float> m_endpointLastTimeSeen;

        private List<ITCPServerReceiver> m_receivers;

        [SerializeField] private int m_connectionCheckRate = 61;
        [SerializeField] private int m_connectionKeepAliveRate = 33;
        private byte[] m_keepAlivePacketBytes;
              
        private void Awake()
        {
            m_receivers = new List<ITCPServerReceiver>();
            m_clientConnections = new Dictionary<IPEndPoint, TcpClient>();
            m_dataToSend = new Dictionary<IPEndPoint, Queue<byte[]>>();
            m_activeEndpoints = new Dictionary<IPEndPoint, bool>();
            m_endpointLastTimeSeen = new Dictionary<IPEndPoint, float>();
            
            m_exitSignal = false;
            m_tcpClientTasks = new List<Task>();

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);
#if DEBUG_LOG
            Debug.Log("Launching TCP server at " + localEndPoint.ToString());
#endif // DEBUG_LOG
            m_keepAlivePacketBytes = new byte[0];

            m_tcpListener = new TcpListener(localEndPoint);
        }

        private void Start()
        {
            m_tcpListener.Start();
            Thread t1 = new Thread(new ThreadStart(CommThread));
            t1.Start();
        }

        private void CommThread()
        {
            while (!m_exitSignal)
            {
                while (m_tcpClientTasks.Count < m_maxConcurrentListeners)
                {
                    Task awaiterTask = Task.Run(async () =>
                    {
                        ProcessMessagesFromClient(await m_tcpListener.AcceptTcpClientAsync());
                    });

                    m_tcpClientTasks.Add(awaiterTask);
                }

                int removeAtIndex = Task.WaitAny(m_tcpClientTasks.ToArray(), m_connectionTimeoutInMS);
                if (removeAtIndex > 0)
                {
                    m_tcpClientTasks.RemoveAt(removeAtIndex);
                }
            }
        }

        private void Update()
        {
            lock (m_lock)
            {
                List<IPEndPoint> keys = new List<IPEndPoint>(m_endpointLastTimeSeen.Keys);
                foreach (IPEndPoint ep in keys)
                {
                    m_endpointLastTimeSeen[ep] += Time.deltaTime;
                }
            }

            if(Time.frameCount % m_connectionCheckRate == 0)
            {
                foreach (IPEndPoint ep in m_clientConnections.Keys)
                {
                    lock (m_lock)
                    {
                        m_activeEndpoints[ep] = CheckConnection(ep);
                    }
                }
            }

            if (Time.frameCount % m_connectionKeepAliveRate == 0)
            {
                foreach (IPEndPoint ep in m_clientConnections.Keys)
                {
                    Send(m_keepAlivePacketBytes, ep);
                }
            }
        }



        private void OnDestroy()
        {
            m_exitSignal = true;
        }

        private void ProcessMessagesFromClient(TcpClient connection)
        {
            using (connection)
            {
                if (!connection.Connected)
                    return;

                IPEndPoint key = (IPEndPoint)(connection.Client.RemoteEndPoint);
                m_clientConnections[key] = connection;
                m_activeEndpoints[key] = true;
                lock (m_lock)
                {
                    m_endpointLastTimeSeen[key] = 0;
                }
                m_dataToSend[key] = new Queue<byte[]>();

                foreach (ITCPServerReceiver receiver in m_receivers)
                {
                    receiver.OnConnect(key);
                }

                using (NetworkStream stream = connection.GetStream())
                {
                    HandleConnection(stream, key);
                }

                foreach (ITCPServerReceiver receiver in m_receivers)
                {
                    receiver.OnDisconnect(key);
                }
#if DEBUG_LOG
                Debug.Log("Removing " + key.ToString() + " TCP connection");
#endif // DEBUG_LOG
                m_clientConnections.Remove(key);
            }
        }

        private void HandleConnection(NetworkStream stream, IPEndPoint source)
        {
#if DEBUG_LOG
            Debug.Log("Starting to handle " + source.ToString() + " TCP connection");
#endif // DEBUG_LOG
            Thread send = new Thread(SendingThread);
            send.Start(new Tuple<NetworkStream, IPEndPoint>(stream, source));

            Thread receive = new Thread(ReceivingThread);
            receive.Start(new Tuple<NetworkStream, IPEndPoint>(stream, source));

            receive.Join();
            send.Join();
        }

        private void ReceivingThread(object streamSourcePair)
        {
            NetworkStream stream = ((Tuple<NetworkStream, IPEndPoint>)streamSourcePair).Item1;
            IPEndPoint source = ((Tuple<NetworkStream, IPEndPoint>)streamSourcePair).Item2;
#if DEBUG_LOG
            Debug.Log("Starting reception from " + source.ToString());
#endif // DEBUG_LOG
            if (!stream.CanRead)
                return;

            int bytesRead = 0;

            byte[] bytes = new byte[DATA_BUFFER_SIZE];
            TcpClient connection = m_clientConnections[source];
            
            while (!m_exitSignal && m_activeEndpoints[source])
            {
                // read from stream
                try
                {
                    bytesRead = stream.Read(bytes, 0, DATA_BUFFER_SIZE);
                }
                catch (IOException ex)
                {
                    Debug.Log(ex.Message);
                    m_activeEndpoints[source] = false;
                    return;
                }

                if (bytesRead > 0)
                {
                    TCPToolkit.Packet packet = TCPToolkit.Packet.PacketFromBytes(bytes.SubArray(0, bytesRead));
                    if (packet.HasValidProtocolID())
                    {
                        lock (m_lock)
                        {
                            m_endpointLastTimeSeen[source] = 0;
                        }
                        // broadcast reception to listeners
                        if (packet.Data.Length > 0) // if it's not a keep-alive packet
                        {
                            // broadcast reception to listeners
                            foreach (ITCPServerReceiver receiver in m_receivers)
                            {
                                receiver.ReceivePacket(packet, source);
                            }
                        }
                    }
                }
            }
        }

        private void SendingThread(object streamSourcePair)
        {
            NetworkStream stream = ((Tuple<NetworkStream, IPEndPoint>)streamSourcePair).Item1;
            IPEndPoint source = ((Tuple<NetworkStream, IPEndPoint>)streamSourcePair).Item2;
#if DEBUG_LOG
            Debug.Log("Starting sending to " + source.ToString());
#endif
            if (!stream.CanWrite)
                return;
            
            while (!m_exitSignal && m_activeEndpoints[source])
            {
                // write to stream (send to client)lock (m_lock)
                lock (m_lock)
                {
                    while (m_dataToSend[source].Count > 0)
                    {
                        byte[] bytesToWrite = tcp.TCPToolkit.Packet.DataToPacket(m_dataToSend[source].Dequeue()).RawBytes;
                        try
                        {
                            stream.Write(bytesToWrite, 0, bytesToWrite.Length);
                        }
                        catch (IOException ex)
                        {
                            Debug.Log(ex.Message);
                            m_activeEndpoints[source] = false;
                            return;
                        }
                    }
                }
            }
        }

        private bool CheckConnection(IPEndPoint endpoint)
        {
            return (m_endpointLastTimeSeen[endpoint] * 1000 < m_connectionTimeoutInMS) ;
        }

        public void Send(byte[] bytes, IPEndPoint target)
        {
            lock (m_lock)
            {
                m_dataToSend[target].Enqueue(bytes);
            }
        }

        public void Subscribe(ITCPServerReceiver receiver)
        {
            if (!m_receivers.Contains(receiver))
                m_receivers.Add(receiver);
        }

        public void Unsubscribe(ITCPServerReceiver receiver)
        {
            m_receivers.Remove(receiver);
        }
    }
}
