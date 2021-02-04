using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace ubv
{
    namespace tcp
    {
        namespace client
        {
            /// <summary>
            /// Wrapper around System.Net.Sockets.UdpClient. Manages client-side connection with server, with timeout and packet loss
            /// </summary>
            public class TCPClient : MonoBehaviour
            {
                [Header("Connection parameters")]
                [SerializeField] private string m_serverAddress;
                [SerializeField] private int m_port;
                
                private TcpClient m_client;
                private IPEndPoint m_server;

                private List<ITCPClientReceiver> m_receivers = new List<ITCPClientReceiver>();
            }
        }
    }
}
