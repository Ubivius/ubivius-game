using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine.SceneManagement;

namespace ubv {

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
        [SerializeField] uint m_snapshotRate = 5; // We send back client data every m_snapshotRate tick

        private uint m_tickAccumulator;
        private uint m_localTick;

        private Dictionary<IPEndPoint, UdpClient> m_endPoints;
        private Dictionary<UdpClient, ClientConnection> m_clientConnections;
        private UdpClient m_server;
        private float m_serverUptime = 0;

        /// <summary>
        /// Manages a specific client connection 
        /// </summary>
        private class ClientConnection
        {
            public float LastConnectionTime;
            public UDPToolkit.ConnectionData ConnectionData;

            public ClientState State;
            public Queue<InputFrame> InputFrames;
            
            public ClientConnection()
            {
                ConnectionData = new UDPToolkit.ConnectionData();
                State = new ClientState();
                InputFrames = new Queue<InputFrame>();
            }
        }
        
        private void Awake()
        {
            m_endPoints = new Dictionary<IPEndPoint, UdpClient>();
            m_clientConnections = new Dictionary<UdpClient, ClientConnection>();
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, m_port);

            m_serverPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_localTick = 0;
            m_tickAccumulator = 0;

            m_server = new UdpClient(localEndPoint);
            m_server.BeginReceive(EndReceiveCallback, m_server);
        }

        private void Update()
        {
            // TODO: Adapt to use a better time tracking? System time?  
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
#if DEBUG
                Debug.Log("Server socket exception: " + e);
#endif // DEBUG
            }
        }

        private void EndSendCallback(System.IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
#if DEBUG
            Debug.Log("Server sent " + c.EndSend(ar).ToString() + " bytes");
#endif // DEBUG
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
                //Debug.Log("Server received " + packet.ToString());

                // Send back to client for ACK

                // introdude random delay
                //Thread.Sleep(50); // Latency of 50 ms

                // design pattern decorator ?


                OnReceive(packet, clientEndPoint);
            }

            server.BeginReceive(EndReceiveCallback, server);
        }

        public void OnReceive(UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
        {
            m_threadLocker.WaitOne();
            //Debug.Log("Received in server " + packet.ToString());

            InputFrame input = InputFrame.FromBytes(packet.Data); 
            if (input != null)
            {
#if DEBUG
                Debug.Log("Input received in server: " + input.Sprinting + ", " + input.Movement + ", " + input.Tick);
#endif // DEBUG
                m_clientConnections[m_endPoints[clientEndPoint]].InputFrames.Enqueue(input);
            }
            m_threadLocker.ReleaseMutex();
        }
        
        private void FixedUpdate()
        {
            // we update the state of the world server side
            // at a reduced pace. Ex: 10 times/second

            // move to different class: ServerSync?

            if (++m_tickAccumulator >= m_snapshotRate)
            {
                m_threadLocker.WaitOne();
                Debug.Log("Starting server snapshot at tick " + m_localTick);
                // on retourne en arrière jusqu'au state qui date du dernier snapshot
                // à partir de là, on applique les inputs reçus
                // on veut appliquer les bons inputs:
                //  on applique donc les inputs à partir de celui qui a un tick
                //  correspondant à celui du state (m_localTick - m_tickAccumulator)
                for (uint tick = 0; tick < m_tickAccumulator; tick++)
                {
                    foreach (ClientConnection conn in m_clientConnections.Values)
                    {
                        InputFrame frame = null;
                        if (conn.InputFrames.Count > 0)
                        {
                            do
                            {
                                frame = conn.InputFrames.Dequeue();
                                Debug.Log("Dequeuing frame with tick " + frame.Tick);
                            }
                            while (frame.Tick < m_localTick - m_tickAccumulator + tick && conn.InputFrames.Count > 0);
                        }
                        /*if (conn.InputFrames.Count > 0)
                        {
                            InputFrame frame = conn.InputFrames.Dequeue();
                            m_rigidBody.MovePosition(m_rigidBody.position + // must be called in main unity thread
                                frame.Movement * (frame.Sprinting ? m_movementSettings.SprintVelocity : m_movementSettings.WalkVelocity) * Time.fixedDeltaTime);

                            conn.State.Position = m_rigidBody.position;
                            conn.State.Tick = frame.Tick;
                        }*/
                        if (frame != null)
                        {
                            m_rigidBody.MovePosition(m_rigidBody.position + // must be called in main unity thread
                                    frame.Movement *
                                    (frame.Sprinting ? m_movementSettings.SprintVelocity : m_movementSettings.WalkVelocity) *
                                    Time.fixedDeltaTime);

                            conn.State.Position = m_rigidBody.position;
                            
                            //conn.State.Tick = frame.Tick;
                        }

                    }
                    m_serverPhysics.Simulate(Time.fixedDeltaTime);
                }
                
                m_threadLocker.ReleaseMutex();

                m_tickAccumulator = 0;
                foreach (UdpClient client in m_endPoints.Values)
                {
                    Send(m_clientConnections[client].State.ToBytes(), client);
                }
            }
            m_localTick++;
        }
    }
}