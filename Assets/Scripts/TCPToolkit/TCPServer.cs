using UnityEngine;
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

        private const int DATA_BUFFER_SIZE = 1024;
                
        protected List<Task> m_tcpClientTasks;

        private Dictionary<IPEndPoint, TcpClient> m_clientConnections;
        private Dictionary<IPEndPoint, Queue<byte[]>> m_dataToSend;

        private List<ITCPServerReceiver> m_receivers;
                
        private void Awake()
        {
            m_receivers = new List<ITCPServerReceiver>();
            m_clientConnections = new Dictionary<IPEndPoint, TcpClient>();
            m_dataToSend = new Dictionary<IPEndPoint, Queue<byte[]>>();
            
            m_exitSignal = false;
            m_tcpClientTasks = new List<Task>();

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);
#if DEBUG_LOG
            Debug.Log("Launching TCP server at " + localEndPoint.ToString());
#endif // DEBUG_LOG

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
            while (!m_exitSignal)
            {
                // read from stream
                try
                {
                    bytesRead = stream.Read(bytes, 0, DATA_BUFFER_SIZE);
                }
                catch (IOException ex)
                {
                    Debug.Log(ex.Message);
                    return;
                }

                if (bytesRead > 0)
                {
                    TCPToolkit.Packet packet = TCPToolkit.Packet.PacketFromBytes(bytes.SubArray(0, bytesRead));
                    if (packet.HasValidProtocolID())
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

        private void SendingThread(object streamSourcePair)
        {
            NetworkStream stream = ((Tuple<NetworkStream, IPEndPoint>)streamSourcePair).Item1;
            IPEndPoint source = ((Tuple<NetworkStream, IPEndPoint>)streamSourcePair).Item2;
#if DEBUG_LOG
            Debug.Log("Starting sending to " + source.ToString());
#endif
            if (!stream.CanWrite)
                return;

            while (!m_exitSignal)
            {
                // write to stream (send to client)lock (m_lock)
                lock(m_lock)
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
                            return;
                        }
                    }
                }
            }
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
            m_receivers.Add(receiver);
        }

        public void Unsubscribe(ITCPServerReceiver receiver)
        {
            m_receivers.Remove(receiver);
        }
    }
}
