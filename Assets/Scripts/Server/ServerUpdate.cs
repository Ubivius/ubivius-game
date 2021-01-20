using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace ubv
{
    namespace server
    {
        namespace logic
        {
            private readonly object lock_ = new object();

            // TEMPORARY for now, TODO: make work with multiple players
            [SerializeField] private common.StandardMovementSettings m_movementSettings;
            [SerializeField] private Rigidbody2D m_rigidBody;

            // takes a player GameObject as a parameter 
            // instantiates a player when it connects
            // 

            [SerializeField] private string m_physicsScene;
            private PhysicsScene2D m_serverPhysics;

            [SerializeField] private udp.server.UDPServer m_server;

            private Dictionary<IPEndPoint, ClientConnection> m_IPConnections;
            private Dictionary<ClientConnection, common.data.InputMessage> m_clientInputs;

            [SerializeField] uint m_snapshotRate = 5; // We send back client data every m_snapshotRate tick

            private uint m_tickAccumulator;

            private class ClientConnection
            {
                private ServerState m_currentState;

                public ClientConnection(uint playerID)
                {
                    State = new client.ClientState();
                    State.PlayerID.Set(playerID);
                    common.PlayerState player = new common.PlayerState();
                    player.ID.Set(playerID);
                    State.AddPlayer(player);
                }
            }

                [SerializeField] private udp.server.UDPServer m_server;

#if NETWORK_SIMULATE
                [HideInInspector] public UnityEngine.Events.UnityEvent ForceStartGameButtonEvent;
#endif // NETWORK_SIMULATE

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
                lock (lock_)
                {
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

                                client.State.Player().Position.Set(m_rigidBody.position);
                                client.State.Tick.Set(client.ServerTick);
                                client.ServerTick++;
                            }
                        }

                        m_serverPhysics.Simulate(Time.fixedDeltaTime);
                    }

                    m_clientInputs.Clear();
                }

                if (++m_tickAccumulator > m_snapshotRate)
                {
                    m_tickAccumulator = 0;
                    foreach (IPEndPoint ip in m_IPConnections.Keys)
                    {
                        //Debug.Log("server bytes = " + System.BitConverter.ToString(m_IPConnections[ip].State.GetBytes()));
                        //Debug.Log("Sent from server : " + m_IPConnections[ip].State.Player().Position.Value);
                        m_server.Send(m_IPConnections[ip].State.GetBytes(), ip);
                    }
                }
                
                // DRAFT SUR AUTH:
                /*
                 * Client envoie un message d'auth en TCP, qui contient
                 *  un "hello" + des credentials / un moyen de prouver 
                 *  qui'il est legit
                 * Serveur renvoie un message d'acknowledgement de
                 * connexion, qui contient le playerID qu'il a attribué 
                 * au joueur du client
                 * 
                 * En théorie, on connect et on disconnect vont être gérées par le TCP 
                 * et pas par l'UDP
                 * et OnConnect dans l'UDP/ici gérerait un nouveau joueur dont on connait
                 * déjà l'ID (qui a été donné  par le TCP)
                */
            }
            
            public void Receive(udp.UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
            {
                common.data.InputMessage inputs = udp.Serializable.FromBytes<common.data.InputMessage>(packet.Data);
                if (inputs != null)
                {
#if DEBUG_LOG
                    Debug.Log("Input received in server: " + inputs.StartTick);
#endif // DEBUG     
                    lock (lock_)
                    {
                        m_clientInputs[m_IPConnections[clientEndPoint]] = inputs;
                    }

                }
            }

            public void OnConnect(IPEndPoint clientIP)
            {
                uint playerID = 0; // clientIP.Address.GetHashCode(); // for now
                m_IPConnections.Add(clientIP, new ClientConnection(playerID));
            }

            public void OnDisconnect(IPEndPoint clientIP)
            {
                m_IPConnections.Remove(clientIP);
            }
        }
    }
}
