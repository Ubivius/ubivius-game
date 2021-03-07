using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;
using ubv.common.data;
using ubv.common.serialization;

namespace ubv.server.logic
{
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
                
        List<int> m_toRemoveCache;

        public GameplayState(udp.server.UDPServer UDPServer,
            GameObject playerPrefab, 
            Dictionary<IPEndPoint, ClientConnection> UDPClientConnections, 
            common.StandardMovementSettings movementSettings, 
            int snapshotDelay, 
            int simulationBuffer,
            string physicsScene)
        {
            m_tickAccumulator = 0;
            m_masterTick = 0;
            m_bufferedMasterTick = 0;
            m_simulationBuffer = simulationBuffer;
            m_UDPClientConnections = UDPClientConnections;

            m_movementSettings = movementSettings;

            m_snapshotTicks = snapshotDelay;
            m_toRemoveCache = new List<int>();

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
                player.GUID.Value = m_UDPClientConnections[ip].PlayerGUID;
                player.Position.Value = m_bodies[m_UDPClientConnections[ip].PlayerGUID].position;

                m_UDPClientConnections[ip].State.AddPlayer(player);
                m_UDPClientConnections[ip].State.PlayerGUID = player.GUID.Value;
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
                    
            m_UDPserver = UDPServer;
            m_UDPserver.Subscribe(this);
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
#if DEBUG_LOG
                            Debug.Log("Missed a player input from " + client.PlayerGUID + " for tick " + m_masterTick);
#endif //DEBUG_LOG
                            frame = new common.data.InputFrame(); // create a default frame to not move player
                        }
                        else
                        {
                            frame = m_clientInputBuffers[client][m_masterTick];
                        }

                        // must be called in main unity thread
                        Rigidbody2D body = m_bodies[client.PlayerGUID];
                        common.logic.PlayerMovement.Execute(ref body, m_movementSettings, frame, Time.fixedDeltaTime);
                                
                        m_toRemoveCache.Clear();
                        foreach(int tick in m_clientInputBuffers[client].Keys)
                        {
                            if(tick <= m_masterTick)
                            {
                                m_toRemoveCache.Add(tick);
                            }
                        }

                        for(int i = 0; i < m_toRemoveCache.Count; i++)
                        {
                            m_clientInputBuffers[client].Remove(m_toRemoveCache[i]);
                        }
                    }
                            
                    m_serverPhysics.Simulate(Time.fixedDeltaTime);

                    m_masterTick++;
                            
                    foreach (ClientConnection client in m_clientInputBuffers.Keys)
                    {
                        Rigidbody2D body = m_bodies[client.PlayerGUID];
                        common.data.PlayerState player = client.State.GetPlayer();
                        player.Position.Value = body.position;
                        player.Rotation.Value = body.rotation;
                        client.State.Tick.Value = (uint)m_masterTick;
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
            common.data.InputMessage inputs = common.serialization.IConvertible.CreateFromBytes<common.data.InputMessage>(packet.Data);
            if (inputs != null && m_UDPClientConnections.ContainsKey(clientEndPoint))
            {
                lock (m_lock)
                {
                    ClientConnection conn = m_UDPClientConnections[clientEndPoint];
                    List<common.data.InputFrame> inputFrames = inputs.InputFrames.Value;
#if DEBUG_LOG
                    Debug.Log("(NOW = " + m_masterTick + ") Received tick " + inputs.StartTick.Value + " to " +  (inputs.StartTick.Value + inputFrames.Count) +  " from " + conn.PlayerGUID);
#endif //DEBUG_LOG

                    int frameIndex = 0;
                    for (int i = 0; i < inputFrames.Count; i++)
                    {
                        frameIndex = (int)inputs.StartTick.Value + i;
                        if (frameIndex >= m_masterTick)
                        {
                            m_clientInputBuffers[conn][frameIndex] = inputFrames[i];
                        }
                    }
                }
            }
        }
    }
}
