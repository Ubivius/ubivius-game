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
        public class ServerUpdate : MonoBehaviour, udp.server.IServerReceiver
        {
            private readonly object lock_ = new object();

            // TEMPORARY for now, TODO: make work with multiple players
            [SerializeField] private common.StandardMovementSettings m_movementSettings;

            // takes a player GameObject as a parameter 
            // instantiates a player when it connects
            // 
            [SerializeField] GameObject m_playerPrefab;   
            private Dictionary<uint, Rigidbody2D> m_bodies; 
            private List<common.PlayerState> m_players;

            private struct IPID
            {
                public IPEndPoint ClientIP;
                public uint PlayerID;

                public IPID(IPEndPoint clientIP, uint playerID) : this()
                {
                    this.ClientIP = clientIP;
                    this.PlayerID = playerID;
                }
            }

            private Queue<IPID> m_playersPending;

            [SerializeField] private string m_physicsScene;
            private PhysicsScene2D m_serverPhysics;

            [SerializeField] private udp.server.UDPServer m_server;

            private Dictionary<IPEndPoint, ClientConnection> m_IPConnections;
            private Dictionary<ClientConnection, common.data.InputMessage> m_clientInputs;

            [SerializeField] uint m_snapshotDelay = 5; // We send back client data every m_snapshotRate tick

            private uint m_tickAccumulator;

            private class ClientConnection
            {
                public uint ServerTick;
                public client.ClientState State;

                public uint PlayerID { get; private set; }

                public ClientConnection(List<common.PlayerState> players, uint playerID)
                {
                    State = new client.ClientState();
                    PlayerID = playerID;
                    // loop through all connection players and add them
                    for(int i = 0; i < players.Count; i++)
                    {
                        State.AddPlayer(players[i]);
                    }
                }
            }

            private void Awake()
            {
                m_playersPending = new Queue<IPID>();
                m_players = new List<common.PlayerState>();
                m_bodies = new Dictionary<uint, Rigidbody2D>();
                m_IPConnections = new Dictionary<IPEndPoint, ClientConnection>();
                m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
                m_tickAccumulator = 0;
                m_clientInputs = new Dictionary<ClientConnection, common.data.InputMessage>();
            }

            // Use this for initialization
            void Start()
            {
                m_server.AddReceiver(this);
                StartCoroutine(PlayerInstantiator());
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
                                Rigidbody2D body = m_bodies[client.PlayerID];

                                common.logic.PlayerMovement.Execute(ref body, m_movementSettings, frame, Time.fixedDeltaTime);

                                client.State.GetPlayer(client.PlayerID).Position.Set(body.position);
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
            }
            
            public void Receive(udp.UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
            {
                common.data.InputMessage inputs = udp.Serializable.FromBytes<common.data.InputMessage>(packet.Data);
                if (inputs != null && m_IPConnections.ContainsKey(clientEndPoint))
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
                uint playerID = 1; //  (uint)clientIP.Address.GetHashCode(); // for now

                lock (lock_) 
                {
                    m_playersPending.Enqueue( new IPID ( clientIP, playerID  ));
                }
            }

            public void OnDisconnect(IPEndPoint clientIP)
            {
                m_IPConnections.Remove(clientIP);
            }

            private IEnumerator PlayerInstantiator()
            {
                while (true)
                {
                   while (m_playersPending.Count > 0)
                    {
                        IPID ipid = m_playersPending.Dequeue();
                        common.PlayerState newPlayer = new common.PlayerState();
                        newPlayer.ID.Set(ipid.PlayerID);
                        m_players.Add(newPlayer);

                        m_bodies[ipid.PlayerID] = Instantiate(m_playerPrefab).GetComponent<Rigidbody2D>();

                        m_IPConnections.Add(ipid.ClientIP, new ClientConnection(m_players, ipid.PlayerID));
                    }
                    yield return new WaitForFixedUpdate();
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
    }
}

