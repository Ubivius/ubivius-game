using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;
using ubv.common.data;
using ubv.common.serialization;
using ubv.common;

namespace ubv.server.logic
{
    /// <summary>
    /// Represents the state of the server during the game
    /// </summary>
    public class GameplayState : ServerState, udp.server.IUDPServerReceiver
    {
        private Dictionary<IPEndPoint, ClientState> m_UDPClientStates;
                
        private Dictionary<ClientState, Dictionary<int, InputFrame>> m_clientInputBuffers;
        
        [SerializeField] private int m_snapshotTicks;
        [SerializeField] private string m_physicsSceneName;

        private uint m_tickAccumulator;
        private int m_masterTick;
        private int m_bufferedMasterTick;
        private int m_simulationBuffer;
        
        private PhysicsScene2D m_serverPhysics;
        
        [SerializeField] private List<ServerGameplayStateUpdater> m_updaters;
                
        private List<int> m_toRemoveCache;

        protected override void StateAwake()
        {
            ServerState.m_gameplayState = this;
        }

        public void Init(Dictionary<IPEndPoint, ClientState> UDPClientStates, int simulationBuffer)
        {
            m_tickAccumulator = 0;
            m_masterTick = 0;
            m_bufferedMasterTick = 0;
            m_simulationBuffer = simulationBuffer;
            m_UDPClientStates = UDPClientStates;
            
            m_toRemoveCache = new List<int>();

            m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(m_physicsSceneName).GetPhysicsScene2D();
            
            m_clientInputBuffers = new Dictionary<ClientState, Dictionary<int, InputFrame>>();
                   
            // add each player to client states
            foreach (IPEndPoint ip in m_UDPClientStates.Keys)
            {
                PlayerState player = new PlayerState();
                player.GUID.Value = m_UDPClientStates[ip].PlayerGUID;
                
                m_UDPClientStates[ip].AddPlayer(player);
                m_UDPClientStates[ip].PlayerGUID = player.GUID.Value;
            }

            // add each player to each other client state
            foreach (ClientState baseState in m_UDPClientStates.Values)
            {
                m_clientInputBuffers[baseState] = new Dictionary<int, common.data.InputFrame>();
                foreach (ClientState conn in m_UDPClientStates.Values)
                {
                    PlayerState currentPlayer = conn.GetPlayer();
                    if (baseState.PlayerGUID != conn.PlayerGUID)
                    {
                        baseState.AddPlayer(currentPlayer);
                    }
                }
            }


            foreach (ServerGameplayStateUpdater updater in m_updaters)
            {
                updater.Setup();
            }

            foreach (ClientState state in m_UDPClientStates.Values)
            {
                foreach(ServerGameplayStateUpdater updater in m_updaters)
                {
                    updater.InitClient(state);
                }
            }

            foreach (ClientState state in m_UDPClientStates.Values)
            {
                foreach (ServerGameplayStateUpdater updater in m_updaters)
                {
                    updater.InitPlayer(state.GetPlayer());
                }
            }
            
            m_UDPServer.Subscribe(this);
        }
                
        protected override void StateFixedUpdate()
        {
            lock (m_lock)
            {
                if (++m_bufferedMasterTick > m_simulationBuffer + m_masterTick)
                {
                    foreach (ClientState client in m_clientInputBuffers.Keys)
                    {
                        // if input buffer has a frame corresponding to this tick
                        InputFrame frame = null;
                        if(!m_clientInputBuffers[client].ContainsKey(m_masterTick))
                        {
#if DEBUG_LOG
                            Debug.Log("Missed a player input from " + client.PlayerGUID + " for tick " + m_masterTick);
#endif //DEBUG_LOG
                            frame = new InputFrame(); // create a default frame to not move player
                        }
                        else
                        {
                            frame = m_clientInputBuffers[client][m_masterTick];
                        }

                        // must be called in main unity thread
                        foreach (ServerGameplayStateUpdater updater in m_updaters)
                        {
                            updater.FixedUpdateFromClient(client, frame, Time.fixedDeltaTime);
                        }

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
                            
                    foreach (ClientState client in m_clientInputBuffers.Keys)
                    {
                        foreach (ServerGameplayStateUpdater updater in m_updaters)
                        {
                            updater.UpdateClient(client);
                        }
                        client.Tick.Value = (uint)m_masterTick;
                    }
                }

                if (++m_tickAccumulator > m_snapshotTicks)
                {
                    m_tickAccumulator = 0;
                    foreach (IPEndPoint ip in m_UDPClientStates.Keys)
                    {
                        m_UDPServer.Send(m_UDPClientStates[ip].GetBytes(), ip);
                    }
                }
            }
        }

        public void Receive(udp.UDPToolkit.Packet packet, IPEndPoint clientEndPoint)
        {
            InputMessage inputs = IConvertible.CreateFromBytes<InputMessage>(packet.Data);
            if (inputs != null && m_UDPClientStates.ContainsKey(clientEndPoint))
            {
                lock (m_lock)
                {
                    ClientState conn = m_UDPClientStates[clientEndPoint];
                    List<InputFrame> inputFrames = inputs.InputFrames.Value;
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
