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

                protected bool m_exitSignal;

                // public delegate void ConnectionHandlerDelegate(NetworkStream connectedAutoDisposedNetStream);
                
                protected List<Task> m_tcpClientTasks;

                // protected ConnectionHandlerDelegate OnConnection;
                
                private void Awake()
                {
                    // OnConnection = null
                    m_exitSignal = false;
                    m_tcpClientTasks = new List<Task>();
                    m_tcpListener = new TcpListener(IPAddress.Any, m_port);
                }

                private void Update()
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
                    
                    var startTime = DateTime.Now;
                    int i = 0;
                    
                    while (!m_exitSignal)
                    {
                        //byte[] data = 
                    }
                }
            }
        }
    }
}
