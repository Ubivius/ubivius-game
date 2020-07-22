using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace UBV
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

        private float m_timeOutTimer;
        private float m_RTTTimer;
        private float m_RTT;
        private bool m_connected;

        private Dictionary<uint, float> m_sequencesSendTime;
        private UDPToolkit.ConnectionData m_connectionData;
        private UdpClient m_client;
        private IPEndPoint m_server;

        private void Awake()
        {
            m_timeOutTimer = 0;
            m_RTT = 0;
            m_RTTTimer = 0; // modify to make it per packet
            m_connectionData = new UDPToolkit.ConnectionData();
            m_sequencesSendTime = new Dictionary<uint, float>();
            m_connected = false;

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

        public void Send(byte[] data) // TODO: generic it then convert to bytes from T or overload with standard data types (int, float, etc)
        {
            try
            {
                UDPToolkit.Packet packet = m_connectionData.Send(data);
                uint seq = packet.Sequence;

                //Debug.Log("Sending (from client) packet with local seq. " + packet.Sequence);
                m_sequencesSendTime.Add(seq, Time.realtimeSinceStartup);

                byte[] bytes = packet.ToBytes();
                m_client.BeginSend(bytes, bytes.Length, m_server, EndSendCallback, m_client);
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

                if (m_connectionData.Receive(packet))
                {
                    m_connected = true;
                    //Debug.Log("Client received (RTT = " + m_RTT.ToString() + ")");
                    //Debug.Log(packet.ToString());
                    ClientState.Receive(packet);
                }
            }

            c.BeginReceive(EndReceiveCallback, c);
        }
        
    }
}