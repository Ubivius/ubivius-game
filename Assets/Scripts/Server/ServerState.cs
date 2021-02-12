﻿using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;

namespace ubv
{
    namespace server
    {
        namespace logic
        {
            // TO READ https://fabiensanglard.net/quake3/network.php
            // and maybe https://thad.frogley.info/w/gfg08/gfg08.pdf

            abstract public class ServerState
            {
                protected readonly object m_lock = new object();
                
                public abstract ServerState Update();
                public abstract ServerState FixedUpdate();
            }


            public class ClientConnection
            {
                public client.ClientState State;

                public int PlayerGUID { get; private set; }

                public ClientConnection(int playerGUID)
                {
                    State = new client.ClientState();
                    State.SetPlayerID(playerGUID);
                    PlayerGUID = playerGUID;
                }
            }

            /// <summary>
            /// Represents the state of the server before the game.
            /// In charge of regrouping player parties, and launching 
            /// the game with a fixed number of players
            /// </summary>
            public class GameCreationState : ServerState, tcp.server.ITCPServerReceiver, udp.server.IUDPServerReceiver
            {
                private tcp.server.TCPServer m_TCPServer;
                private Dictionary<IPEndPoint, ClientConnection> m_TCPClientConnections;
                private Dictionary<IPEndPoint, ClientConnection> m_UDPClientConnections;

                // transfer to gameplay
                private readonly string m_physicsScene;
                private readonly udp.server.UDPServer m_UDPserver;
                private readonly common.StandardMovementSettings m_movementSettings;
                private readonly int m_snapshotDelay;
                private readonly GameObject m_playerPrefab;

                private List<common.data.PlayerState> m_players;

#if NETWORK_SIMULATE
                private bool m_forceStartGame;
#endif // NETWORK_SIMULATE

                public GameCreationState(udp.server.UDPServer UDPServer, 
                    tcp.server.TCPServer TCPServer,
                    GameObject playerPrefab, 
                    common.StandardMovementSettings 
                    movementSettings, 
                    int snapshotDelay, 
                    string physicsScene
#if NETWORK_SIMULATE
                    , ServerUpdate parent
#endif // NETWORK_SIMULATE 
                    )
                {
                    m_UDPserver = UDPServer;
                    m_TCPServer = TCPServer;
                    m_players = new List<common.data.PlayerState>();
                    m_TCPClientConnections = new Dictionary<IPEndPoint, ClientConnection>();
                    m_UDPClientConnections = new Dictionary<IPEndPoint, ClientConnection>();

                    m_movementSettings = movementSettings;
                    m_snapshotDelay = snapshotDelay;
                    m_physicsScene = physicsScene;
                    m_playerPrefab = playerPrefab;

                    m_forceStartGame = false;

#if NETWORK_SIMULATE
                    parent.ForceStartGameButtonEvent.AddListener(() => { m_forceStartGame = true; });
#endif // NETWORK_SIMULATE 
                    
                    m_TCPServer.Subscribe(this);
                    m_UDPserver.Subscribe(this);
                }

                public override ServerState Update()
                {
                    lock (m_lock)
                    {
                        if (m_TCPClientConnections.Count > 3
#if NETWORK_SIMULATE
                            || m_forceStartGame
#endif // NETWORK_SIMULATE
                        )
                        {
                            m_TCPServer.Unsubscribe(this);
                            m_UDPserver.Unsubscribe(this);

                            common.data.GameStartMessage message = new common.data.GameStartMessage();
                            message.Players.Set(m_players);

                            foreach (IPEndPoint ip in m_TCPClientConnections.Keys)
                            {
                                m_TCPServer.Send(message.GetBytes(), ip);
                            }

                            return new GameplayState(m_UDPserver, m_playerPrefab, m_UDPClientConnections, m_movementSettings, m_snapshotDelay, m_physicsScene);
                        }
                    }
                    return this;
                }

                public override ServerState FixedUpdate()
                {
                    return this;
                }
                
                public void ReceivePacket(TCPToolkit.Packet packet, IPEndPoint clientIP)
                {
                    return;
                }

                public void OnConnect(IPEndPoint clientIP)
                {
                    lock (m_lock)
                    {
                        int playerID = System.Guid.NewGuid().GetHashCode();

                        // TODO get rid of client connection data and only use serializable list of int after serialize rework
                        m_TCPClientConnections[clientIP] = new ClientConnection(playerID);

                        common.data.IdentificationMessage idMessage = new common.data.IdentificationMessage();
                        idMessage.PlayerID.Set(playerID);

                        common.data.PlayerState playerState = new common.data.PlayerState();
                        playerState.GUID.Set(playerID);

                        // set rotation / position according to existing players?

                        m_players.Add(playerState);
                        m_UDPserver.RegisterClient(clientIP.Address);

                        m_TCPServer.Send(idMessage.GetBytes(), clientIP);

#if DEBUG_LOG
                        Debug.Log("Received connection request from " + clientIP.ToString());
#endif // DEBUG_LOG
                    }
                }

                public void OnDisconnect(IPEndPoint clientIP)
                {
                    lock (m_lock)
                    {
                        m_TCPClientConnections.Remove(clientIP);
                    }
                }

                public void Receive(UDPToolkit.Packet packet, IPEndPoint clientIP)
                {
                    if (m_UDPClientConnections.ContainsKey(clientIP))
                    {
#if DEBUG_LOG
                        Debug.Log("Client " + clientIP.ToString() + " already connected. Ignoring.");
#endif // DEBUG_LOG
                        return;
                    }

                    common.data.IdentificationMessage identification = udp.Serializable.FromBytes<common.data.IdentificationMessage>(packet.Data);
                    if (identification != null)
                    {
                        m_UDPClientConnections[clientIP] = new ClientConnection(identification.PlayerID);
                    }
                }
            }

            /// <summary>
            /// Represents the state of the server during the game
            /// </summary>
            public class GameplayState : ServerState, udp.server.IUDPServerReceiver
            {
                private udp.server.UDPServer m_UDPserver;

                private Dictionary<IPEndPoint, ClientConnection> m_UDPClientConnections;
                private Dictionary<int, Rigidbody2D> m_bodies;
                
                private Dictionary<ClientConnection, Dictionary<int, common.data.InputFrame>> m_clientInputBuffers;

                private common.StandardMovementSettings m_movementSettings;
                private readonly int m_snapshotTicks;
                
                private uint m_tickAccumulator;
                private int m_masterTick;
                private int m_bufferedMasterTick;

                private readonly int m_simulationBuffer;

                private PhysicsScene2D m_serverPhysics;

                private GameObject m_playerPrefab;

                public GameplayState(udp.server.UDPServer UDPServer,
                    GameObject playerPrefab, 
                    Dictionary<IPEndPoint, ClientConnection> UDPClientConnections, 
                    common.StandardMovementSettings movementSettings, 
                    int snapshotDelay, 
                    string physicsScene)
                {
                    m_UDPserver = UDPServer;
                    m_UDPserver.Subscribe(this);
                    m_tickAccumulator = 0;
                    m_masterTick = 0;
                    m_bufferedMasterTick = 0;
                    m_simulationBuffer = 5; // 5 * 1/60 FPS = 83 ms de délai approx pour correction serveur
                    m_UDPClientConnections = UDPClientConnections;

                    m_movementSettings = movementSettings;

                    m_snapshotTicks = snapshotDelay;

                    m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(physicsScene).GetPhysicsScene2D();
                    m_playerPrefab = playerPrefab;

                    m_bodies = new Dictionary<int, Rigidbody2D>();
                    m_clientInputBuffers = new Dictionary<ClientConnection, Dictionary<int, common.data.InputFrame>>();
                    
                    // instantiate each player
                    foreach (IPEndPoint ip in m_UDPClientConnections.Keys)
                    {
                        int id = m_UDPClientConnections[ip].PlayerGUID;
                        Rigidbody2D body = GameObject.Instantiate(playerPrefab).GetComponent<Rigidbody2D>();
                        body.position = m_bodies.Count * Vector2.left * 3;
                        body.name = "Server player " + id.ToString();
                        m_bodies.Add(id, body);
                    }

                    // add each player to client states
                    foreach (IPEndPoint ip in m_UDPClientConnections.Keys)
                    {
                        common.data.PlayerState player = new common.data.PlayerState();
                        player.GUID.Set(m_UDPClientConnections[ip].PlayerGUID);
                        player.Position.Set(m_bodies[m_UDPClientConnections[ip].PlayerGUID].position);

                        m_UDPClientConnections[ip].State.AddPlayer(player);
                        m_UDPClientConnections[ip].State.SetPlayerID(player.GUID);
                    }

                    // add each player to each other client state
                    foreach (ClientConnection baseConn in m_UDPClientConnections.Values)
                    {
                        m_clientInputBuffers[baseConn] = new Dictionary<int, common.data.InputFrame>();
                        foreach (ClientConnection conn in m_UDPClientConnections.Values)
                        {
                            common.data.PlayerState currentPlayer = conn.State.GetPlayer();
                            if (baseConn.PlayerGUID != conn.PlayerGUID)
                            {
                                baseConn.State.AddPlayer(currentPlayer);
                            }
                        }
                    }
                }

                public override ServerState Update()
                {
                    return this;
                }
                
                public override ServerState FixedUpdate()
                {
                    lock (m_lock)
                    {
                        if (++m_bufferedMasterTick > m_simulationBuffer + m_masterTick)
                        {
                            foreach (ClientConnection client in m_clientInputBuffers.Keys)
                            {
                                // if input buffer has a frame corresponding to this tick
                                common.data.InputFrame frame = null;
                                if(!m_clientInputBuffers[client].ContainsKey(m_masterTick))
                                {
                                    Debug.Log("Missed a player input from " + client.PlayerGUID);
                                    frame = new common.data.InputFrame(); // create a default frame to not move player
                                }
                                else
                                {
                                    frame = m_clientInputBuffers[client][m_masterTick];
                                }

                                // must be called in main unity thread
                                Rigidbody2D body = m_bodies[client.PlayerGUID];
                                common.logic.PlayerMovement.Execute(ref body, m_movementSettings, frame, Time.fixedDeltaTime);
                                m_clientInputBuffers[client].Remove(m_masterTick);
                            }
                            
                            m_serverPhysics.Simulate(Time.fixedDeltaTime);

                            m_masterTick++;
                            
                            foreach (ClientConnection client in m_clientInputBuffers.Keys)
                            {
                                Rigidbody2D body = m_bodies[client.PlayerGUID];
                                common.data.PlayerState player = client.State.GetPlayer();
                                player.Position.Set(body.position);
                                player.Rotation.Set(body.rotation);
                                client.State.Tick.Set((uint)m_masterTick);
                            }
                        }

                        if (++m_tickAccumulator > m_snapshotTicks)
                        {
                            m_tickAccumulator = 0;
                            foreach (IPEndPoint ip in m_UDPClientConnections.Keys)
                            {
                                m_UDPserver.Send(m_UDPClientConnections[ip].State.GetBytes(), ip);
                            }
                        }
                    }

                    return this;
                }

                public void Receive(udp.UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
                {
                    common.data.InputMessage inputs = udp.Serializable.FromBytes<common.data.InputMessage>(packet.Data);
                    if (inputs != null && m_UDPClientConnections.ContainsKey(clientEndPoint))
                    {
                        lock (m_lock)
                        {
                            ClientConnection conn = m_UDPClientConnections[clientEndPoint];
                            List<common.data.InputFrame> inputFrames = inputs.InputFrames.Value;
                            Debug.Log("(NOW = " + m_masterTick + ") Received tick " + inputs.StartTick.Value + " to " +  (inputs.StartTick.Value + inputFrames.Count) +  " from " + conn.PlayerGUID);

                            int frameIndex = 0;
                            for (int i = 0; i < inputFrames.Count; i++)
                            {
                                frameIndex = (int)inputs.StartTick.Value + i;
                                if (frameIndex >= m_clientInputBuffers[conn].Count + m_masterTick)
                                {
                                    m_clientInputBuffers[conn][frameIndex] = inputFrames[i];
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
