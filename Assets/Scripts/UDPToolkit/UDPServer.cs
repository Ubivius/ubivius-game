using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine.SceneManagement;

namespace ubv {

    /// <summary>
    /// Wrapper around System.Net.Sockets.UdpClient. 
    /// Manages server-side UDP connections with other clients
    /// + manages input messages from clients and computes their positions, then sends them back
    /// https://www.winsocketdotnetworkprogramming.com/clientserversocketnetworkcommunication8d.html
    /// </summary>
    public class UDPServer : MonoBehaviour
    {
        // TEMPORARY, for test purposes because running on same program
        [SerializeField] private StandardMovementSettings   m_movementSettings;
        [SerializeField] private Rigidbody2D                m_rigidBody;
        
        private readonly object lock_ = new object();

        [SerializeField] private string m_physicsScene; 
        private PhysicsScene2D          m_serverPhysics;

        [SerializeField] int m_port = 9050;
        [SerializeField] float m_connectionTimeout = 10f;
        [SerializeField] uint m_snapshotRate = 5; // We send back client data every m_snapshotRate tick

        private uint m_tickAccumulator;

        private Dictionary<IPEndPoint, UdpClient> m_endPoints;
        private Dictionary<UdpClient, ClientConnection> m_clientConnections;
        private Dictionary<ClientConnection, common.data.InputMessage> m_clientInputMessages;
        private UdpClient m_server;
        private float m_serverUptime = 0;

        /// <summary>
        /// Manages a specific client connection 
        /// </summary>
        private class ClientConnection
        {
            public float                        LastConnectionTime;
            public uint                         ServerTick;
            public UDPToolkit.ConnectionData    ConnectionData;

            public client.ClientState          State;
            
            public ClientConnection()
            {
                ConnectionData =    new UDPToolkit.ConnectionData();
                State =             new client.ClientState();
            }
        }
        
        private void Awake()
        {
            m_endPoints =               new Dictionary<IPEndPoint, UdpClient>();
            m_clientConnections =       new Dictionary<UdpClient, ClientConnection>();
            IPEndPoint localEndPoint =  new IPEndPoint(IPAddress.Any, m_port);

            m_serverPhysics =   SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_tickAccumulator = 0;

            m_clientInputMessages = new Dictionary<ClientConnection, common.data.InputMessage>();

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
#if DEBUG_LOG
            Debug.Log("Server sent " + c.EndSend(ar).ToString() + " bytes");
#endif // DEBUG_LOG
        }

        private void EndReceiveCallback(System.IAsyncResult ar)
        {
            // TODO: authentication
            IPEndPoint clientEndPoint = new IPEndPoint(0, 0);
            UdpClient server = (UdpClient)ar.AsyncState;
            byte[] bytes = server.EndReceive(ar, ref clientEndPoint);
            
            // If client is not registered, create a new Socket 
            lock (lock_)
            {
                if (!m_endPoints.ContainsKey(clientEndPoint))
                {
                    m_endPoints.Add(clientEndPoint, new UdpClient());
                    m_endPoints[clientEndPoint].Connect(clientEndPoint);

                    m_clientConnections.Add(m_endPoints[clientEndPoint], new ClientConnection());
                }
            }

            m_clientConnections[m_endPoints[clientEndPoint]].LastConnectionTime = m_serverUptime;

            UDPToolkit.Packet packet = UDPToolkit.Packet.PacketFromBytes(bytes);
            
            lock (lock_)
            {
                if (m_clientConnections[m_endPoints[clientEndPoint]].ConnectionData.Receive(packet))
                {
                    OnReceive(packet, clientEndPoint);
                }
            }

            server.BeginReceive(EndReceiveCallback, server);
        }

        public void OnReceive(UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
        {
            // TODO (maybe) : give up ticks and use only packet sequence number?
            
            common.data.InputMessage inputs = Serializable.FromBytes<common.data.InputMessage>(packet.Data); 
            if (inputs != null)
            {
#if DEBUG_LOG
                Debug.Log("Input received in server: " + inputs.StartTick);
#endif // DEBUG
                lock (lock_)
                {
                    m_clientInputMessages[m_clientConnections[m_endPoints[clientEndPoint]]] = inputs;
                }
            }
        }
        
        private void FixedUpdate()
        {
            // we update the state of the world server side
            // at a reduced pace. Ex: 10 times/second

            // move to different class: ServerSync?

            uint framesToSimulate = 0;
            lock (lock_)
            {
                foreach (ClientConnection client in m_clientInputMessages.Keys)
                {
                    common.data.InputMessage message = m_clientInputMessages[client];
                    int messageCount = message.InputFrames.Value.Count;
                    uint maxTick = message.StartTick + (uint)(messageCount - 1);
#if DEBUG_LOG
                Debug.Log("max tick to simulate = " + maxTick.ToString());
#endif // DEBUG_LOG

                    // on recule jusqu'à ce qu'on trouve le  tick serveur le plus récent
                    uint missingFrames = (maxTick > client.ServerTick) ? maxTick - client.ServerTick : 0;

                    if (framesToSimulate < missingFrames) framesToSimulate = missingFrames;
                }

                for (uint f = framesToSimulate; f > 0; f--)
                {
                    foreach (ClientConnection client in m_clientInputMessages.Keys)
                    {
                        common.data.InputMessage message = m_clientInputMessages[client];
                        int messageCount = message.InputFrames.Value.Count;
                        if (messageCount > f)
                        {
                            common.data.InputFrame frame = message.InputFrames.Value[messageCount - (int)f - 1];

                            // must be called in main unity thread
                            common.logic.PlayerMovement.Execute(ref m_rigidBody, m_movementSettings, frame, Time.fixedDeltaTime);

                            client.State.Position.Set(m_rigidBody.position);
                            client.State.Tick.Set(client.ServerTick);
                            client.ServerTick++;
                        }
                    }

                    m_serverPhysics.Simulate(Time.fixedDeltaTime);
                }

                m_clientInputMessages.Clear();

                if (++m_tickAccumulator > m_snapshotRate)
                {
                    m_tickAccumulator = 0;
                    foreach (UdpClient client in m_endPoints.Values)
                    {
                        Send(m_clientConnections[client].State.GetBytes(), client);
                    }
                }
            }

            // Lerp between wanted position and current position to smooth it?
        }
    }
}