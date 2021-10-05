using UnityEngine;
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
            /// + manages input messages from clients and computes their positions, then sends them back
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

                public bool AcceptNewClients;
                

                private void Awake()
                {
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
                    if (m_playerEndpoints.ContainsKey(playerID))
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
                    else
                    {
#if DEBUG_LOG
                        Debug.Log("Client " + playerID + " is not registered. Ignoring data send request.");
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

                private void EndReceiveCallback(System.IAsyncResult ar)
                {
                    IPEndPoint clientEndPoint = new IPEndPoint(0, 0);
                    UdpClient server = (UdpClient)ar.AsyncState;
                    
                    byte[] bytes = server.EndReceive(ar, ref clientEndPoint);

                    UDPToolkit.Packet packet = UDPToolkit.Packet.PacketFromBytes(bytes);
                    int playerID = packet.PlayerID;
                    if (packet.HasValidProtocolID())
                    {
                        if (!m_clients.ContainsKey(playerID))
                        {
                            if (AcceptNewClients)
                            {
#if DEBUG_LOG
                                Debug.Log("Received data from unregistered client(" + playerID.ToString() + "). Adding to clients.");
#endif // DEBUG_LOG
                                AddNewClient(playerID, clientEndPoint);
                            }
                            else
                            {
#if DEBUG_LOG
                                Debug.Log("Received data from unregistered client(" + playerID.ToString() + "). Not accepting new connections.");
#endif // DEBUG_LOG
                            }
                        }

                        if (m_clients.ContainsKey(playerID))
                        {
                            if (!m_endpointsSet.Contains(clientEndPoint))
                            {
                                // this means the player client doesnt have the same UDP endpoint (probably because he DC'd)
                                
                                // we delete the endpoint previously paired with the playerID (if any)
                                if(m_playerEndpoints.ContainsKey(playerID))
                                {
                                    if(m_playerEndpoints[playerID] != null)
                                    {
                                        m_endpointsSet.Remove(m_playerEndpoints[playerID]);
                                    }
                                }

                                m_playerEndpoints[playerID] = clientEndPoint;
                                m_endpointsSet.Add(clientEndPoint);

                                m_clients[playerID].Close();
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
                    server.BeginReceive(EndReceiveCallback, server);
                }

                private void OnReceive(UDPToolkit.Packet packet, int playerID)
                {
                    // TODO (maybe) : give up ticks and use only packet sequence number?

                    for (int i = 0; i < m_receivers.Count; i++)
                    {
                        m_receivers[i].Receive(packet, playerID);
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
            }
        }
    }
}
