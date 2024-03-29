﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine.SceneManagement;
using System;

namespace ubv
{
    namespace udp
    {
        namespace server
        {
            /// <summary>
            /// Wrapper around System.Net.Sockets.UdpClient. 
            /// Manages server-side UDP connections with other clients
            /// https://www.winsocketdotnetworkprogramming.com/clientserversocketnetworkcommunication8d.html<
            /// https://gafferongames.com/ 
            /// https://gafferongames.com/post/client_server_connection/
            /// </summary>
            public class UDPServer : MonoBehaviour
            {
                [SerializeField] int m_port = 9050;

                private Dictionary<int, IPEndPoint> m_playerEndpoints;
                private Dictionary<int, UdpClient> m_clients;
                private Dictionary<int, UDPToolkit.ConnectionData> m_clientConnections;

                private HashSet<IPEndPoint> m_endpointsSet;

                private UdpClient m_server;
                private List<IUDPServerReceiver> m_receivers = new List<IUDPServerReceiver>();
                
                private byte[] m_packetBytesBuffer;

                private void Awake()
                {
                    m_packetBytesBuffer = new byte[0];
                    m_clients = new Dictionary<int, UdpClient>();
                    m_clientConnections = new Dictionary<int, UDPToolkit.ConnectionData>();
                    m_playerEndpoints = new Dictionary<int, IPEndPoint>();
                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);
                    m_endpointsSet = new HashSet<IPEndPoint>();
                    
                    m_server = new UdpClient(localEndPoint);
#if DEBUG_LOG
                    Debug.Log("Launching UDP server at " + localEndPoint.ToString());
#endif // DEBUG_LOG
                    
                    m_server.BeginReceive(EndReceiveCallback, m_server);
                }
                
                public void Send(byte[] data, int playerID)
                {
                    IPEndPoint endPoint = m_playerEndpoints[playerID];
                    try
                    {
                        byte[] bytes = m_clientConnections[playerID].Send(data, playerID).RawBytes;
                        m_server.BeginSend(bytes, bytes.Length, endPoint, EndSendCallback, m_server);
                    }
                    catch (SocketException e)
                    {
#if DEBUG_LOG
                        Debug.Log("Server socket exception: " + e);
#endif // DEBUG_LOG
                    }
                }

                private void EndSendCallback(System.IAsyncResult ar)
                { }
                
                private void AddNewClient(int playerID, IPEndPoint ep)
                {
                    m_playerEndpoints.Add(playerID, ep);
                    m_clients.Add(playerID, new UdpClient());
                    m_clientConnections.Add(playerID, new UDPToolkit.ConnectionData());
                    m_clients[playerID].Connect(ep);
                    m_endpointsSet.Add(ep);
                }

                public void RemoveClient(int playerID)
                {
                    m_endpointsSet.Remove(m_playerEndpoints[playerID]);
                    m_playerEndpoints.Remove(playerID);
                    m_clients[playerID].Close();
                    m_clients.Remove(playerID);
                    m_clientConnections.Remove(playerID);
                }

                private void EndReceiveCallback(System.IAsyncResult ar)
                {
                    IPEndPoint clientEndPoint = new IPEndPoint(0, 0);
                    UdpClient server = (UdpClient)ar.AsyncState;
                    
                    byte[] bytes = null;
                    try
                    {
                        bytes = server.EndReceive(ar, ref clientEndPoint);
                    }
                    catch (SocketException e)
                    {
                        Debug.Log("Socket error: " + e.ToString());
                    }

                    if (bytes != null && bytes.Length > 0)
                    {
                        // append to m_packetBytesBuffer
                        byte[] tmp = new byte[m_packetBytesBuffer.Length];
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            tmp[i] = m_packetBytesBuffer[i];
                        }
                        m_packetBytesBuffer = new byte[tmp.Length + bytes.Length];

                        for (int i = tmp.Length; i < m_packetBytesBuffer.Length; i++)
                        {
                            m_packetBytesBuffer[i] = bytes[i - tmp.Length];
                        }

                        UDPToolkit.Packet packet = UDPToolkit.Packet.FirstPacketFromBytes(m_packetBytesBuffer);
                        if (packet != null)
                        {
                            m_packetBytesBuffer = new byte[0];
                            int playerID = packet.PlayerID;
                            if (packet.HasValidProtocolID())
                            {
                                if (!m_clients.ContainsKey(playerID))
                                {
#if DEBUG_LOG
                                    Debug.Log("Received data from unregistered client(" + playerID.ToString() + "). Adding to clients.");
#endif // DEBUG_LOG
                                    AddNewClient(playerID, clientEndPoint);
                                }

                                if (m_clients.ContainsKey(playerID))
                                {
                                    if (!m_endpointsSet.Contains(clientEndPoint))
                                    {
                                        // this means the player client doesnt have the same UDP endpoint (probably because he DC'd)
                                        // we delete the endpoint previously paired with the playerID (if any)
                                        if (m_playerEndpoints.ContainsKey(playerID))
                                        {
                                            if (m_playerEndpoints[playerID] != null)
                                            {
                                                m_endpointsSet.Remove(m_playerEndpoints[playerID]);
                                            }
                                        }

                                        m_playerEndpoints[playerID] = clientEndPoint;
                                        m_endpointsSet.Add(clientEndPoint);

                                        m_clients[playerID].Close();
                                        m_clients[playerID] = new UdpClient();
                                        m_clients[playerID].Connect(clientEndPoint);
                                    }

                                    if (m_clientConnections[playerID].Receive(packet))
                                    {
                                        OnReceive(packet, playerID);
                                    }
                                }
                            }
                            else
                            {
#if DEBUG_LOG
                                Debug.Log("Received invalid network protocol ID packet. Rejecting.");
#endif //DEBUG_LOG
                            }
                        }
                    }
                    server.BeginReceive(EndReceiveCallback, server);
                }

                private void OnReceive(UDPToolkit.Packet packet, int playerID)
                {
                    // TODO (maybe) : give up ticks and use only packet sequence number?

                    for (int i = 0; i < m_receivers.Count; i++)
                    {
                        m_receivers[i].UDPReceive(packet, playerID);
                    }
                }

                public void Subscribe(IUDPServerReceiver receiver)
                {
                    if(!m_receivers.Contains(receiver))
                        m_receivers.Add(receiver);
                }

                public void Unsubscribe(IUDPServerReceiver receiver)
                {
                    m_receivers.Remove(receiver);
                }

                private void OnDestroy()
                {
                    List<int> playerIDs = new List<int>(m_clients.Keys);
                    foreach(int id in playerIDs)
                    {
                        RemoveClient(id);
                    }

                    List<IUDPServerReceiver> receivers = new List<IUDPServerReceiver>(m_receivers);
                    foreach(IUDPServerReceiver rcv in receivers)
                    {
                        m_receivers.Remove(rcv);
                    }
                }
            }
        }
    }
}
