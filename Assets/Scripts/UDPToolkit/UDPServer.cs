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

        private Dictionary<IPEndPoint, UdpClient> m_endPoints;
        private Dictionary<UdpClient, ClientConnection> m_clientConnections;
        private Dictionary<ClientConnection, InputMessage> m_clientInputMessages;
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

            public ClientState          State;
            
            public ClientConnection()
            {
                ConnectionData =    new UDPToolkit.ConnectionData();
                State =             new ClientState();
            }
        }
        
        private void Awake()
        {
            m_endPoints =               new Dictionary<IPEndPoint, UdpClient>();
            m_clientConnections =       new Dictionary<UdpClient, ClientConnection>();
            IPEndPoint localEndPoint =  new IPEndPoint(IPAddress.Any, m_port);

            m_serverPhysics =   SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_tickAccumulator = 0;

            m_clientInputMessages = new Dictionary<ClientConnection, InputMessage>();

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

            m_threadLocker.WaitOne();
            // If client is not registered, create a new Socket 
            if (!m_endPoints.ContainsKey(clientEndPoint))
            {
                m_endPoints.Add(clientEndPoint, new UdpClient());
                m_endPoints[clientEndPoint].Connect(clientEndPoint);

                m_clientConnections.Add(m_endPoints[clientEndPoint], new ClientConnection());
            }
            m_threadLocker.ReleaseMutex();

            m_clientConnections[m_endPoints[clientEndPoint]].LastConnectionTime = m_serverUptime;

            UDPToolkit.Packet packet = UDPToolkit.Packet.PacketFromBytes(bytes);
            m_threadLocker.WaitOne();
            if (m_clientConnections[m_endPoints[clientEndPoint]].ConnectionData.Receive(packet))
            {
                OnReceive(packet, clientEndPoint);
            }
            m_threadLocker.ReleaseMutex();

            server.BeginReceive(EndReceiveCallback, server);
        }

        public void OnReceive(UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
        {
            //Debug.Log("Received in server " + packet.ToString());

            InputMessage inputs = InputMessage.FromBytes(packet.Data); 
            if (inputs != null)
            {
#if DEBUG_LOG
                Debug.Log("Input received in server: " + inputs.StartTick);
#endif // DEBUG
                m_threadLocker.WaitOne();
                m_clientInputMessages[m_clientConnections[m_endPoints[clientEndPoint]]] = inputs;
                m_threadLocker.ReleaseMutex();
            }
        }
        
        private void FixedUpdate()
        {
            // we update the state of the world server side
            // at a reduced pace. Ex: 10 times/second

            // move to different class: ServerSync?

            // pour tous les clients
            // pour une frame: est-ce qu'ils ont envoyé de l'input pour cette frame?
            // rendre tout ça tick-agnostique? 
            // => faire que les clients n'aient à se baser que sur leurs ticks à eux

            // ex:
            /* 
             On recoit, à l'intérieur d'une frame de FixedUpdate côté serveur:

            C1 Input XYZ Tick 23
            C2 Input XYZ Tick 41

            On pose que le temps maître est celui du serveur, et que les deux joueurs
            on envoyé leurs inputs en même temps (donc qu'ils ne sont pas synchros au
            niveau de leurs ticks) et que leur RTT est ==.

            On simule la position pour une master frame puis on renvoie ça aux clients.
             
             
             */

            // on recoit un message : input-client
            // on stock dans une structure qui nous permet de faire le tour des inputs?
            // foreach( lastInputsReceived ? )

            // on veut savoir comment de frames on doit resimuler
            // on parcourt les messages
            /*
             *
             *  -3  -2  -1  0   
             *  x   x   x   x   C1
             *      x   x   x   C2
             *              x   C3
             *          x   x   C4
             *  Donc on prends 4 frames à resimuler
             *  On 
             * */

            uint framesToSimulate = 0;
           
            m_threadLocker.WaitOne();
            foreach (ClientConnection client in m_clientInputMessages.Keys)
            {
                InputMessage message = m_clientInputMessages[client];
                int messageCount = message.InputFrames.Count;
                uint maxTick =  message.StartTick + (uint)(messageCount - 1);
#if DEBUG_LOG
                Debug.Log("max tick to simulate = " + maxTick.ToString());
#endif // DEBUG_LOG

                // on recule jusqu'à ce qu'on trouve le  tick serveur le plus récent
                uint missingFrames = (maxTick > client.ServerTick) ? maxTick - client.ServerTick : 0;
                
                if (framesToSimulate < missingFrames) framesToSimulate = missingFrames;
            }
            
            for(uint f = framesToSimulate; f > 0; f--)
            {
                foreach (ClientConnection client in m_clientInputMessages.Keys)
                {
                    InputMessage message = m_clientInputMessages[client];
                    int messageCount = message.InputFrames.Count;
                    if (messageCount > f)
                    {
                        InputFrame frame = message.InputFrames[messageCount - (int)f - 1];

                        m_rigidBody.MovePosition(m_rigidBody.position + // must be called in main unity thread
                            frame.Movement *
                            (frame.Sprinting ? m_movementSettings.SprintVelocity : m_movementSettings.WalkVelocity) *
                            Time.fixedDeltaTime);

                        client.State.Position = m_rigidBody.position;
                        client.ServerTick++;
                        client.State.Tick = client.ServerTick;
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
                    Send(m_clientConnections[client].State.ToBytes(), client);
                }
            }
            m_threadLocker.ReleaseMutex();

            // Lerp between wanted position and current position to smooth it?
        }
    }
}