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

namespace ubv
{
    namespace tcp
    {
        namespace server
        {
            /// <summary>
            /// Wrapper around System.Net.Sockets.TcpClient. 
            /// Manages server-side TCP connections with other clients
            /// https://www.codeproject.com/Articles/5270779/High-Performance-TCP-Client-Server-using-TCPListen 
            /// https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=net-5.0
            /// </summary>
            public class TCPServer : MonoBehaviour
            {
                [SerializeField] int m_port = 9051;
                [SerializeField] int m_connectionTimeoutInMS;
                [SerializeField] int m_maxConcurrentListeners = 6;
                
                private TcpListener m_tcpListener;
                private TcpClient tcpClient; //temp

                protected bool m_exitSignal;

                private const int DATA_BUFFER_SIZE = 1024;
                
                protected List<Task> m_tcpClientTasks;
                
                private void Awake()
                {
                    // OnConnection = null
                    m_exitSignal = false;
                    m_tcpClientTasks = new List<Task>();

                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);

                    Debug.Log("Launching TCP server at " + localEndPoint.ToString());

                    m_tcpListener = new TcpListener(localEndPoint);
                }

                private void Start()
                {
                    m_tcpListener.Start();
                    Thread t1 = new Thread(new ThreadStart(TestThread));
                    t1.Start();
                }

                private void TestThread()
                {
                    //tcpClient = m_tcpListener.AcceptTcpClient();

                    //ProcessMessagesFromClient(tcpClient);
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

                private void ProcessMessagesFromClient(TcpClient connection)
                {
                    using (connection) // automatically disposes of the connection async
                    {
                        if (!connection.Connected)
                            return;

                        using (NetworkStream stream = connection.GetStream())
                        {
                            HandleConnection(stream);
                        }
                    }
                }

                private void HandleConnection(NetworkStream stream)
                {
                    if (!stream.CanRead && !stream.CanWrite)
                        return;
                    
                    int bytesRead = 0;
                    
                    byte[] data = new byte[DATA_BUFFER_SIZE];
                    while (!m_exitSignal)
                    {
                        bytesRead = stream.Read(data, 0, DATA_BUFFER_SIZE);
                        if (bytesRead > 0)
                        {
                            TCPToolkit.Packet packet = TCPToolkit.Packet.PacketFromBytes(data.SubArray(0, bytesRead));
                            //if (packet.HasValidProtocolID())
                            {
                                // broadcast reception to listeners
                                Debug.Log("Received following in server : " + System.Text.Encoding.ASCII.GetString(data));
                            }
                        }
                    }
                }
            }
        }
    }
}
