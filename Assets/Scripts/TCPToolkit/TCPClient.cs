using UnityEngine;
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
        [SerializeField] int m_connectionTimeoutInMS = 500000;
        protected readonly object m_lock = new object();

        private string m_serverAddress;
        private int m_serverPort;

        protected bool m_exitSignal;

        private TcpClient m_client;
        private IPEndPoint m_server;

        private List<ITCPClientReceiver> m_receivers;
        private List<ITCPClientReceiver> m_receiversAwaitingSubscription;
        private List<ITCPClientReceiver> m_receiversAwaitingUnsubscription;
        private bool m_iteratingTroughReceivers;

        private const int DATA_BUFFER_SIZE = 1024*1024*4;
        private Queue<byte[]> m_dataToSend;

        private bool m_activeEndpoint;
        [SerializeField] private float m_connectionCheckTimerIntervalMS = 1000;
        [SerializeField] private float m_coonnectionKeepAliveTimerIntervalMS = 250;
        private float m_endpointLastTimeSeen;
        private byte[] m_keepAlivePacketBytes;

        private float m_connectionCheckTimer;
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
            m_endpointLastTimeSeen = 0;

            m_iteratingTroughReceivers = false;
            m_keepAlivePacketBytes = new byte[0];

            m_connectionCheckTimer = 0;
            m_connectionKeepAliveTimer = 0;
            
            m_requestToSendEvent = new ManualResetEvent(false);
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
                }

                if (!m_client.Connected)
                        return;

                m_iteratingTroughReceivers = true;
                foreach (ITCPClientReceiver receiver in m_receivers)
                {
                    receiver.OnSuccessfulConnect();
                }
                m_iteratingTroughReceivers = false;
#if DEBUG_LOG
                Debug.Log("Connected to server.");
#endif // DEBUG_LOG

                using (NetworkStream stream = m_client.GetStream())
                {
                    m_activeEndpoint = true;
                    HandleConnection(stream);
                    stream.Close();
                }
                m_activeEndpoint = false;

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

            int bytesRead = 0;

            byte[] bytes = new byte[DATA_BUFFER_SIZE];
            int lastPacketEnd = 0;
            byte[] packetBytes = new byte[0];
            int bufferOffset = 0;
            int totalPacketBytes = 0;

            bool readyToReadPacket = true;

            while (!m_exitSignal && m_activeEndpoint && bufferOffset >= 0)
            {
                if (readyToReadPacket)
                {
                    bytesRead = 0;
                    totalPacketBytes = 0;
                    readyToReadPacket = false;
                }
                
                // read from stream until we read a full packet
                try
                {
                    bytesRead += stream.Read(bytes, bufferOffset % DATA_BUFFER_SIZE, DATA_BUFFER_SIZE - bufferOffset);
                }
                catch (IOException ex)
                {
                    Debug.Log(ex.Message);
                    return;
                }

                if (bytesRead > 0)
                {
                    TCPToolkit.Packet packet = TCPToolkit.Packet.FirstPacketFromBytes(bytes);
                    while (packet != null && totalPacketBytes < bytesRead)
                    {
                        readyToReadPacket = true;
                        lastPacketEnd = packet.RawBytes.Length;
                        totalPacketBytes += lastPacketEnd;
                        lock (m_lock)
                        {
                            m_endpointLastTimeSeen = 0;
                        }
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
                        packet = TCPToolkit.Packet.FirstPacketFromBytes(bytes.ArrayFrom(totalPacketBytes));
                    }

                    // on a un restant de bytes
                    // we shift
                    bufferOffset = bytesRead - totalPacketBytes;
                    for (int i = 0; i < bufferOffset; i++)
                    {
                        bytes[i] = bytes[i + totalPacketBytes];
                    }
                }

                try
                {
                    Task.Delay(50, new CancellationToken(m_exitSignal)).Wait();
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
#if DEBUG_LOG
            Debug.Log("State at client receiving thread exit : Active endpoint ? " + m_activeEndpoint.ToString() + ", Exit signal ?" + m_exitSignal + ", Buffer offset :" + bufferOffset);
#endif // DEBUG_LOG
        }

        public void Reconnect()
        {
#if DEBUG_LOG
            Debug.Log("Trying to reconnect...");
#endif // DEBUG_LOG

            if(m_serverAddress == null || m_serverPort == 0)
            {
#if DEBUG_LOG
                Debug.Log("No previous connection has been made.");
#endif // DEBUG_LOG
            }
            else
            {
                Connect(m_serverAddress, m_serverPort);
            }
        }

        private void Update()
        {
            if (m_activeEndpoint)
            {
                m_endpointLastTimeSeen += Time.deltaTime;

                if (m_connectionCheckTimer > m_connectionCheckTimerIntervalMS)
                {
                    m_connectionCheckTimer = 0;
                    m_activeEndpoint = CheckConnection();
                }
                
                if (m_connectionKeepAliveTimer > m_coonnectionKeepAliveTimerIntervalMS && m_activeEndpoint)
                {
                    m_connectionKeepAliveTimer = 0;
                    Send(m_keepAlivePacketBytes);
                }
            }


            lock (m_lock)
            {
                if (!m_iteratingTroughReceivers)
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
        
        private void SendingThread(object streamObj)
        {
            NetworkStream stream = (NetworkStream)streamObj;

            if (!stream.CanWrite)
                return;
            
            while (!m_exitSignal && m_activeEndpoint)
            {
                m_requestToSendEvent.WaitOne();
                m_requestToSendEvent.Reset();
                // write to stream (send to client)
                lock (m_lock)
                {
                    while (m_dataToSend.Count > 0)
                    {
                        if(m_playerID == null)
                        {
#if DEBUG_LOG
                            Debug.Log("Player ID is not set. Cannot send to server.");
                            return;
#endif // DEBUG_LOG
                        }

                        byte[] bytesToWrite = tcp.TCPToolkit.Packet.DataToPacket(m_dataToSend.Dequeue(), m_playerID.Value).RawBytes;
                        try
                        {
                            stream.Write(bytesToWrite, 0, bytesToWrite.Length);
                        }
                        catch (IOException ex)
                        {
                            Debug.Log(ex.Message);
                            return;
                        }
                    }
                }
            }
            Debug.Log("State at client sending thread exit : Active endpoint ? " + m_activeEndpoint.ToString() + ", Exit signal ?" + m_exitSignal);
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
        
        private bool CheckConnection()
        {
            return (m_endpointLastTimeSeen * 1000 < m_connectionTimeoutInMS);
        }

        public bool IsConnected()
        {
            return m_activeEndpoint;
        }
    }
}
