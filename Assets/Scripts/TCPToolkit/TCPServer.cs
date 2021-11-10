using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
        [SerializeField] int m_maxConcurrentListeners = 12;
                
        private TcpListener m_tcpListener;

        protected volatile bool m_exitSignal;

        // Un gros buffer c'est le fun, on est pas très limités en taille
        private const int DATA_BUFFER_SIZE = 1024*1024*4;
        // pour workaround le unix shit
        private const int MAX_BYTES_READ = 32768;

        protected List<Task> m_tcpClientTasks;

        private Dictionary<int, IPEndPoint> m_clientEndpoints;
        private Dictionary<IPEndPoint, int> m_clientIDs;

        private Dictionary<IPEndPoint, TcpClient> m_clientConnections;
        private Dictionary<IPEndPoint, Queue<byte[]>> m_dataToSend;
        private volatile Dictionary<IPEndPoint, bool> m_activeEndpoints;

        private List<ITCPServerReceiver> m_receivers;

        [SerializeField] int m_connectionTimeoutInMS;

        [SerializeField] private float m_coonnectionKeepAliveTimerIntervalMS = 1000;
        private float m_connectionKeepAliveTimer;
        private byte[] m_keepAlivePacketBytes;

        private ManualResetEvent m_requestToSendEvent;

        private void Awake()
        {
            m_receivers = new List<ITCPServerReceiver>();
            m_clientConnections = new Dictionary<IPEndPoint, TcpClient>();
            m_dataToSend = new Dictionary<IPEndPoint, Queue<byte[]>>();
            m_activeEndpoints = new Dictionary<IPEndPoint, bool>();

            m_clientIDs = new Dictionary<IPEndPoint, int>();
            m_clientEndpoints = new Dictionary<int, IPEndPoint>();
            
            m_exitSignal = false;
            m_tcpClientTasks = new List<Task>();

            m_keepAlivePacketBytes = new byte[0];
            m_connectionKeepAliveTimer = 0;

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);
#if DEBUG_LOG
            Debug.Log("Launching TCP server at " + localEndPoint.ToString());
#endif // DEBUG_LOG

            m_tcpListener = new TcpListener(localEndPoint);
            m_requestToSendEvent = new ManualResetEvent(false);
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
                    try
                    {
                        Task.Delay(50, new CancellationToken(m_exitSignal)).Wait();
                    }
                    catch (AggregateException ex)
                    {
#if DEBUG_LOG
                        Debug.Log(ex.Message);
#endif // DEBUG_LOG
                    }
                }

                int removeAtIndex = Task.WaitAny(m_tcpClientTasks.ToArray(), m_connectionTimeoutInMS);
                if (removeAtIndex >= 0)
                {
#if DEBUG_LOG
                    Debug.Log("Removing TCP task " + removeAtIndex);
#endif // DEBUG_LOG
                    m_tcpClientTasks.RemoveAt(removeAtIndex);
                }

                try
                {
                    Task.Delay(50, new CancellationToken(m_exitSignal)).Wait();
                }
                catch (AggregateException ex)
                {
#if DEBUG_LOG
                    Debug.Log(ex.Message);
#endif // DEBUG_LOG
                }
            }
        }

        private void Update()
        {
            ConnectionsKeepAlive();
        }

        private void ConnectionsKeepAlive()
        {
            m_connectionKeepAliveTimer += Time.deltaTime;
            if (m_connectionKeepAliveTimer * 1000 > m_coonnectionKeepAliveTimerIntervalMS)
            {
                lock (m_lock)
                {
                    m_connectionKeepAliveTimer = 0;

                    foreach (int id in m_clientEndpoints.Keys)
                    {
                        Send(m_keepAlivePacketBytes, id);
                    }
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

                IPEndPoint ep = (IPEndPoint)(connection.Client.RemoteEndPoint);
                m_clientConnections[ep] = connection;
                m_activeEndpoints[ep] = true;
                m_dataToSend[ep] = new Queue<byte[]>();

                using (NetworkStream stream = connection.GetStream())
                {
                    HandleConnection(stream, ep);
                    stream.Close();
                }
                m_activeEndpoints[ep] = false;

                RemoveClient(m_clientIDs[ep]);

#if DEBUG_LOG
                Debug.Log("Removing " + ep.ToString() + " TCP connection");
#endif // DEBUG_LOG
                m_clientConnections.Remove(ep);
                connection.Close();
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

        private void AddNewClient(int playerID, IPEndPoint source)
        {
            m_clientEndpoints.Add(playerID, source);
            m_clientIDs.Add(source, playerID);
            lock (m_lock)
            {
                foreach (ITCPServerReceiver receiver in m_receivers)
                {
                    receiver.OnTCPConnect(playerID);
                }
            }
        }

        private void UpdateClientEndpoint(int playerID, IPEndPoint source)
        {
            m_clientIDs.Remove(m_clientEndpoints[playerID]);
            m_clientEndpoints[playerID] = source;
            m_clientIDs.Add(source, playerID);
        }

        private void RemoveClient(int playerID)
        {
            m_clientIDs.Remove(m_clientEndpoints[playerID]);
            m_clientEndpoints.Remove(playerID);
            lock (m_lock)
            {
                foreach (ITCPServerReceiver receiver in m_receivers)
                {
                    receiver.OnTCPDisconnect(playerID);
                }
            }
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
            
            byte[] bytes = new byte[DATA_BUFFER_SIZE];
            int totalPacketBytes = 0;
            int totalBytesReadBeforePacket = 0;

            bool readyToReadPacket = true;
            stream.ReadTimeout = m_connectionTimeoutInMS;

            while (!m_exitSignal && m_activeEndpoints[source] && totalBytesReadBeforePacket >= 0)
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
                    totalBytesReadBeforePacket += stream.Read(bytes, totalBytesReadBeforePacket % DATA_BUFFER_SIZE, MAX_BYTES_READ);
                }
                catch (IOException ex)
                {
                    Debug.Log(ex.Message);
                    lock (m_lock)
                    {
                        m_activeEndpoints[source] = false;
                        break;
                    }
                }
                
                if (totalBytesReadBeforePacket > 0 && m_activeEndpoints[source])
                {
                    TCPToolkit.Packet packet = TCPToolkit.Packet.FirstPacketFromBytes(bytes);
                    while (packet != null)
                    {
                        int playerID = packet.PlayerID;

                        if (!m_clientEndpoints.ContainsKey(playerID))
                        {
                            // we have a new client
                            AddNewClient(playerID, source);
                        }
                        else
                        {
                            // we replace the previous endpoint
                            UpdateClientEndpoint(playerID, source);   
                        }

                        readyToReadPacket = true;
                        totalPacketBytes += packet.RawBytes.Length;

                        // broadcast reception to listeners
                        if (packet.Data.Length > 0) // if it's not a keep-alive packet
                        {
                            lock (m_lock)
                            {
                                foreach (ITCPServerReceiver receiver in m_receivers)
                                {
                                    receiver.TCPReceive(packet, playerID);
                                }
                            }
                        }
                        if (totalBytesReadBeforePacket > totalPacketBytes)
                        {
                            packet = TCPToolkit.Packet.FirstPacketFromBytes(bytes.ArrayFrom(totalPacketBytes));
                        }
                        else
                        {
                            packet = null;
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
                m_requestToSendEvent.WaitOne(m_connectionTimeoutInMS);
                m_requestToSendEvent.Reset();
                // write to stream (send to client)lock (m_lock)
                lock (m_lock)
                {
                    while (m_dataToSend[source].Count > 0)
                    {
                        byte[] bytesToWrite = tcp.TCPToolkit.Packet.DataToPacket(m_dataToSend[source].Dequeue(), m_clientIDs[source]).RawBytes;
                        try
                        {
                            stream.Write(bytesToWrite, 0, bytesToWrite.Length);
                        }
                        catch (IOException ex)
                        {
                            Debug.Log(ex.Message);
                            lock (m_lock)
                            {
                                m_activeEndpoints[source] = false;
                            }
                            return;
                        }
                    }
                }
            }
        }
        
        public void Send(byte[] bytes, int playerID)
        {
            m_requestToSendEvent.Set();
            lock (m_lock)
            {
                m_dataToSend[m_clientEndpoints[playerID]].Enqueue(bytes);
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
