using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace ubv
{
    /// <summary>
    /// Wrapper around System.Net.Sockets.UdpClient. Manages client-side connection with server, with timeout and packet loss
    /// </summary>
    public class UDPClient : MonoBehaviour
    {
        [Header("Connection parameters")]
        [SerializeField] private string m_serverAddress;
        [SerializeField] private int m_port;
        [SerializeField] private float m_serverTimeOut = 10;
        [SerializeField] private float m_lostPacketTimeOut =  1;
        [SerializeField] private float m_maximumGoodRTT = 0.15f; // the maximum round-trip-time(/ping) to consider the connection as good in seconds
        [SerializeField] private float m_minimumTimeToSwitchModes = 10; // time required to switch from bad to good connection mode
        [SerializeField] private float m_maximumPacketsPerSecond = 60;
        [SerializeField] private float m_minimumPacketsPerSecond = 10;

        private float m_packetsPerSecond;

        private float m_timeOutTimer;
        private float m_RTTTimer;
        private float m_connectionQualityTimer; // time since connection was checked as bad
        private float m_lastPacketSentTime;

        private float m_RTT;
        private bool m_connected;
        private bool m_connectionIsGood; // if good, will send more packets
       
        private Dictionary<uint, float> m_sequencesSendTime;
        private UDPToolkit.ConnectionData m_connectionData;
        private UdpClient m_client;
        private IPEndPoint m_server;

        private void Awake()
        {
            m_timeOutTimer = 0;
            m_RTT = 0;
            m_RTTTimer = 0;
            m_connectionQualityTimer = 0;
            m_connectionData = new UDPToolkit.ConnectionData();
            m_sequencesSendTime = new Dictionary<uint, float>();
            m_connected = false;

            m_connectionIsGood = true;
            m_packetsPerSecond = m_maximumPacketsPerSecond;
            m_lastPacketSentTime = 0;

            m_client = new UdpClient();
            m_server = new IPEndPoint(IPAddress.Parse(m_serverAddress), m_port);
        }

        private void Start()
        {
            m_client.BeginReceive(EndReceiveCallback, m_client);
        }

        // Update is called once per frame
        void Update()
        {
            m_RTTTimer = Time.realtimeSinceStartup;
            if (m_connected)
            {
                m_timeOutTimer += Time.deltaTime;
                m_connectionQualityTimer += Time.deltaTime;

                if (m_timeOutTimer > m_serverTimeOut)
                {
                    m_connectionData = new UDPToolkit.ConnectionData();
                    m_sequencesSendTime.Clear();
                    Debug.Log("Server timed out. Disconnecting.");
                    m_connected = false;
                    m_timeOutTimer = 0;
                }
            }
            
        }
        
        /// <summary>
        /// Tries to send a packet with a data payload. If the connection is
        /// not good enough (if too many packets are sent), the packet is dropped.
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] data) 
        {
            if(m_connectionIsGood)
            {
                if ( Time.realtimeSinceStartup - m_lastPacketSentTime < 1.0f / m_maximumPacketsPerSecond)
                {
                    return;
                }
            }
            else
            {
                if (Time.realtimeSinceStartup - m_lastPacketSentTime < 1.0f / m_minimumPacketsPerSecond)
                {
                    return;
                }
            }

            try
            {
                UDPToolkit.Packet packet = m_connectionData.Send(data);
                uint seq = packet.Sequence;

                //Debug.Log("Sending (from client) packet with local seq. " + packet.Sequence);
                m_sequencesSendTime.Add(seq, Time.realtimeSinceStartup);

                byte[] bytes = packet.ToBytes();
                m_client.BeginSend(bytes, bytes.Length, m_server, EndSendCallback, m_client);
                m_lastPacketSentTime = Time.realtimeSinceStartup;
            }
            catch (SocketException e)
            {
                Debug.Log("Client socket exception: " + e);
            }
        }

        private void EndSendCallback(System.IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            //Debug.Log("Client sent " + c.EndSend(ar).ToString() + " bytes");
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
            catch(SocketException e)
            {
                Debug.Log("Socket error: " + e.ToString());
            }

            if (bytes != null)
            {
                m_timeOutTimer = 0;

                UDPToolkit.Packet packet = UDPToolkit.Packet.PacketFromBytes(bytes);

                if (m_sequencesSendTime.ContainsKey(packet.ACK))
                {
                    m_RTT = m_RTTTimer - m_sequencesSendTime[packet.ACK];
                    m_sequencesSendTime.Remove(packet.ACK);
                }

               
                if (m_RTT > m_maximumGoodRTT)
                {
                    m_connectionIsGood = false;
                    m_connectionQualityTimer = 0;
                }
                else
                {
                    if (m_connectionQualityTimer > m_minimumTimeToSwitchModes)
                    {
                        m_connectionIsGood = true;
                    }
                }
                
                if (m_connectionData.Receive(packet))
                {
                    m_connected = true;
#if DEBUG
                    Debug.Log("Client received (RTT = " + m_RTT.ToString() + ")");
#endif // DEBUG
                    ClientState.Receive(packet);
                }
            }

            c.BeginReceive(EndReceiveCallback, c);
        }
        
    }
}