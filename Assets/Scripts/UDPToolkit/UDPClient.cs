using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using ubv.client.logic;

namespace ubv
{
    namespace udp
    {
        namespace client
        {
            /// <summary>
            /// Wrapper around System.Net.Sockets.UdpClient. Manages client-side connection with server, with timeout and packet loss
            /// </summary>
            public class UDPClient : MonoBehaviour
            {
                [Header("Connection parameters")]
                [SerializeField] private float m_maximumPacketsPerSecond = 60;

                private float m_currentTime;
                private float m_lastPacketSentTime;
                
                private Dictionary<uint, float> m_sequencesSendTime;
                private UDPToolkit.ConnectionData m_connectionData;
                private UdpClient m_client;
                private IPEndPoint m_server;

                private List<IUDPClientReceiver> m_receivers = new List<IUDPClientReceiver>();

                private void Awake()
                {
                    m_connectionData = new UDPToolkit.ConnectionData();
                    m_sequencesSendTime = new Dictionary<uint, float>();
                    
                    m_lastPacketSentTime = 0;

                    m_client = new UdpClient();
                }

                public void SetTargetServer(string address, int port)
                {
                    m_server = new IPEndPoint(IPAddress.Parse(address), port);
                }

                private void Start()
                {
                    m_client.BeginReceive(EndReceiveCallback, m_client);
                }

                // Update is called once per frame
                void Update()
                {
                    m_currentTime = Time.realtimeSinceStartup;
                }

                /// <summary>
                /// Tries to send a packet with a data payload.
                /// if too many packets are sent, the packet is dropped.
                /// </summary>
                /// <param name="data"></param>
                public void Send(byte[] data, int playerID)
                {
                    /*
                     * We should eventually find a way to queue up packets or something like that.
                     * Hard dropping packets is not the solution but we may need to find something
                     * if we realize we are sending too many packets per second (causing a 
                     * network flood).
                     * if (m_currentTime - m_lastPacketSentTime < 1.0f / m_maximumPacketsPerSecond)
                    {
                        return;
                    }*/

                    try
                    {
                        UDPToolkit.Packet packet = m_connectionData.Send(data, playerID);
                        uint seq = packet.Sequence;

                        m_sequencesSendTime.Add(seq, m_currentTime);

                        byte[] bytes = packet.RawBytes;
                        m_client.BeginSend(bytes, bytes.Length, m_server, EndSendCallback, m_client);
                        m_lastPacketSentTime = m_currentTime;
                    }
                    catch (SocketException e)
                    {
                        Debug.Log("Client socket exception: " + e);
                    }
                }

                private void EndSendCallback(System.IAsyncResult ar)
                {
                    UdpClient c = (UdpClient)ar.AsyncState;
                }

                private void EndReceiveCallback(System.IAsyncResult ar)
                {
                    UdpClient c = (UdpClient)ar.AsyncState;
                    IPEndPoint temp = new IPEndPoint(0, 0);
                    byte[] bytes = null;
                    try
                    {
                        bytes = c.EndReceive(ar, ref temp);
                    }
                    catch (SocketException e)
                    {
                        Debug.Log("Socket error: " + e.ToString());
                    }

                    if (bytes != null)
                    {
                        UDPToolkit.Packet packet = UDPToolkit.Packet.PacketFromBytes(bytes);
                        
                        if (m_connectionData.Receive(packet))
                        {
                            Distribute(packet);
                        }
                    }

                    c.BeginReceive(EndReceiveCallback, c);
                }

                public void Subscribe(IUDPClientReceiver receiver)
                {
                    if (!m_receivers.Contains(receiver))
                        m_receivers.Add(receiver);
                }

                public void Unsubscribe(IUDPClientReceiver receiver)
                {
                    m_receivers.Remove(receiver);
                }

                private void Distribute(UDPToolkit.Packet packet)
                {
                    for (int i = 0; i < m_receivers.Count; i++)
                    {
                        m_receivers[i].ReceivePacket(packet);
                    }
                }

                public void Disconnect()
                {
                    m_client.Close();
                }
            }
        }
    }
}
