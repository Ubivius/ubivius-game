using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace ubv
{
    namespace server
    {
        /// <summary>
        /// Manages server specific update logic
        /// </summary>
        public class ServerUpdate : MonoBehaviour, IServerReceiver
        {
            // TEMPORARY, for test purposes because running on same program
            [SerializeField] private StandardMovementSettings m_movementSettings;
            [SerializeField] private Rigidbody2D m_rigidBody;

            [SerializeField] private string m_physicsScene;
            private PhysicsScene2D m_serverPhysics;

            [SerializeField] private UDPServer m_server;

            private Dictionary<IPEndPoint, ClientConnection> m_IPConnections;
            private Dictionary<ClientConnection, common.data.InputMessage> m_clientInputs;

            [SerializeField] uint m_snapshotRate = 5; // We send back client data every m_snapshotRate tick

            private uint m_tickAccumulator;

            private class ClientConnection
            {
                public uint ServerTick;
                public client.ClientState State;

                public ClientConnection()
                {
                    State = new client.ClientState();
                }
            }

            // List of all players (ClientStates)

            private void Awake()
            {
                m_IPConnections = new Dictionary<IPEndPoint, ClientConnection>();
                m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
                m_tickAccumulator = 0;
                m_clientInputs = new Dictionary<ClientConnection, common.data.InputMessage>();
            }

            // Use this for initialization
            void Start()
            {
                m_server.AddReceiver(this);
            }

            // Update is called once per frame
            void Update()
            {

            }

            // Updates all players rewinds if necessary
            private void FixedUpdate()
            {
                // for each player
                // check if missing frames
                // update frames

                uint framesToSimulate = 0;
                foreach (ClientConnection client in m_clientInputs.Keys)
                {
                    common.data.InputMessage message = m_clientInputs[client];
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
                    foreach (ClientConnection client in m_clientInputs.Keys)
                    {
                        common.data.InputMessage message = m_clientInputs[client];
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

                m_clientInputs.Clear();

                if (++m_tickAccumulator > m_snapshotRate)
                {
                    m_tickAccumulator = 0;
                    foreach (IPEndPoint ip in m_IPConnections.Keys)
                    {
                        m_server.Send(m_IPConnections[ip].State.GetBytes(), ip);
                    }
                }
            }
            
            public void Receive(UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
            {
                common.data.InputMessage inputs = Serializable.FromBytes<common.data.InputMessage>(packet.Data);
                if (inputs != null)
                {
#if DEBUG_LOG
                    Debug.Log("Input received in server: " + inputs.StartTick);
#endif // DEBUG     
                    m_clientInputs[m_IPConnections[clientEndPoint]] = inputs;

                }
            }

            public void OnConnect(IPEndPoint clientIP)
            {
                m_IPConnections.Add(clientIP, new ClientConnection());
            }

            public void OnDisconnect(IPEndPoint clientIP)
            {
                m_IPConnections.Remove(clientIP);
            }
        }
    }
}

