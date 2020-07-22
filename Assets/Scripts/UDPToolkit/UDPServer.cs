using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine.SceneManagement;

namespace UBV {

    // NOTE: Server and client must use same functions for client state simulation steps ?

    /// <summary>
    /// Wrapper around System.Net.Sockets.UdpClient. Manage server-side UDP connections with other clients
    /// https://www.winsocketdotnetworkprogramming.com/clientserversocketnetworkcommunication8d.html
    /// </summary>
    public class UDPServer : MonoBehaviour
    {
        // TEMPORARY, for test purposes because running on same program
        [SerializeField] private StandardMovementSettings m_movementSettings;
        [SerializeField] private Rigidbody2D m_rigidBody;

        static private Mutex m_threadLocker = new Mutex();

        [SerializeField] private string m_physicsScene; 
        private PhysicsScene2D m_serverPhysics;

        [SerializeField] int m_port = 9050;
        [SerializeField] float m_connectionTimeout = 10f;

        /// <summary>
        /// Manages a specific client connection 
        /// </summary>
        private class ClientConnection
        {
            public float LastConnectionTime;
            public UDPToolkit.ConnectionData ConnectionData;

            public InputFrame CurrentInput;
            public bool InputFrameIsReady = false;

            public ClientConnection()
            {
                ConnectionData = new UDPToolkit.ConnectionData();
            }
        }
        
        private Dictionary<IPEndPoint, UdpClient> m_endPoints;
        private Dictionary<UdpClient, ClientConnection> m_clientConnections;
        UdpClient m_server;
        private float m_serverUptime = 0;
        
        private void Awake()
        {
            m_endPoints = new Dictionary<IPEndPoint, UdpClient>();
            m_clientConnections = new Dictionary<UdpClient, ClientConnection>();
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);

            m_serverPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();

            m_server = new UdpClient(localEndPoint);
            m_server.BeginReceive(EndReceiveCallback, m_server);
        }

        private void Update()
        {
            m_serverUptime += Time.deltaTime;
            if(Time.frameCount % 10 == 0)
            {
                RemoveTimedOutClients();
            }
        }

        private void RemoveTimedOutClients()
        {
            List<IPEndPoint> toRemove = new List<IPEndPoint>();
            // check if any client has disconnected (has not sent a packet in TIMEOUT seconds)
            foreach (IPEndPoint ep in m_endPoints.Keys)
            {
                if (m_serverUptime - m_clientConnections[m_endPoints[ep]].LastConnectionTime > m_connectionTimeout)
                {
                    Debug.Log("Client timed out. Disconnecting.");
                    toRemove.Add(ep);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                m_clientConnections.Remove(m_endPoints[toRemove[i]]);
                m_endPoints.Remove(toRemove[i]);
            }
        }

        private void Send(byte[] data, UdpClient clientConnection)
        {
            try
            {
                byte[] bytes = m_clientConnections[clientConnection].ConnectionData.Send(data).ToBytes();
                clientConnection.BeginSend(bytes, bytes.Length, EndSendCallback, clientConnection);
            }
            catch (SocketException e)
            {
                Debug.Log("Server socket exception: " + e);
            }
        }

        private void EndSendCallback(System.IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            //Debug.Log("Server sent " + c.EndSend(ar).ToString() + " bytes");
        }

        private void EndReceiveCallback(System.IAsyncResult ar)
        {
            // TODO: authentication
            IPEndPoint clientEndPoint = new IPEndPoint(0, 0);
            UdpClient server = (UdpClient)ar.AsyncState;
            byte[] bytes = server.EndReceive(ar, ref clientEndPoint);
            
            // If client is not registered, create a new Socket 
            if(!m_endPoints.ContainsKey(clientEndPoint))
            {
                m_endPoints.Add(clientEndPoint, new UdpClient());
                m_endPoints[clientEndPoint].Connect(clientEndPoint);

                m_clientConnections.Add(m_endPoints[clientEndPoint], new ClientConnection());
            }

            m_clientConnections[m_endPoints[clientEndPoint]].LastConnectionTime = m_serverUptime;

            UDPToolkit.Packet packet = UDPToolkit.Packet.PacketFromBytes(bytes);
            if (m_clientConnections[m_endPoints[clientEndPoint]].ConnectionData.Receive(packet))
            {
                //Debug.Log("Server received");
                //Debug.Log(packet.ToString());

                // Send back to client for ACK

                // introdude random delay
                Thread.Sleep(3);

                // design pattern decorator ?


                OnReceive(packet, clientEndPoint);
            }

            server.BeginReceive(EndReceiveCallback, server);
        }

        public void OnReceive(UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
        {
            //Debug.Log("Received in server " + packet.ToString());

            InputFrame input = InputFrame.FromBytes(packet.Data); 
            if (input != null)
            {
                Debug.Log("Input received in server: " + input.Sprinting + ", " + input.Movement);
                m_threadLocker.WaitOne();
                m_clientConnections[m_endPoints[clientEndPoint]].InputFrameIsReady = true;
                m_clientConnections[m_endPoints[clientEndPoint]].CurrentInput = input;

                m_threadLocker.ReleaseMutex();
            }
        }

        // temporary
        private void FixedUpdate()
        {
            foreach (IPEndPoint ep in m_endPoints.Keys)
            {
                ClientConnection conn = m_clientConnections[m_endPoints[ep]];
                if (conn.InputFrameIsReady)
                {
                    conn.InputFrameIsReady = false;
                    m_rigidBody.MovePosition(m_rigidBody.position + // must be called in main unity thread
                       conn.CurrentInput.Movement * (conn.CurrentInput.Sprinting ? m_movementSettings.SprintVelocity : m_movementSettings.WalkVelocity) * Time.fixedDeltaTime);
                    //m_rigidBody.position += new Vector2(0, -1f);

                    ClientState state = new ClientState();
                    state.Position = m_rigidBody.position;
                    state.Tick = conn.CurrentInput.Tick;
                    Send(state.ToBytes(), m_endPoints[ep]);
                }
            }
            m_serverPhysics.Simulate(Time.fixedDeltaTime);
        }
    }
}