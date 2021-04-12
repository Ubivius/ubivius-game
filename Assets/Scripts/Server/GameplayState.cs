using System.Collections.Generic;
using System.Net;
using ubv.common;
using ubv.common.data;
using ubv.common.serialization;
using ubv.tcp;
using UnityEngine;

namespace ubv.server.logic
{
    /// <summary>
    /// Represents the state of the server during the game
    /// </summary>
    public class GameplayState : ServerState, udp.server.IUDPServerReceiver, tcp.server.ITCPServerReceiver
    {
        private HashSet<int> m_clients;
        private Dictionary<int, ClientState> m_clientStates;
        private Dictionary<int, bool> m_connectedClients;
                
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

        public void Init(Dictionary<int, common.serialization.types.String> clients, int simulationBuffer)
        {
            m_tickAccumulator = 0;
            m_masterTick = 0;
            m_bufferedMasterTick = 0;
            m_simulationBuffer = simulationBuffer;
            m_clientStates = new Dictionary<int, ClientState>();
            m_clients = new HashSet<int>(clients.Keys);

            m_connectedClients = new Dictionary<int, bool>();

            m_toRemoveCache = new List<int>();

            m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(m_physicsSceneName).GetPhysicsScene2D();
            
            m_clientInputBuffers = new Dictionary<ClientState, Dictionary<int, InputFrame>>();
                   
            // add each player to client states
            foreach (int id in m_clients)
            {
                PlayerState player = new PlayerState();
                player.GUID.Value = id;
                m_clientStates[id] = new ClientState(id);
                m_clientStates[id].AddPlayer(player);
                m_connectedClients.Add(id, true);
            }

            // add each player to each other client state
            foreach (ClientState baseState in m_clientStates.Values)
            {
                m_clientInputBuffers[baseState] = new Dictionary<int, common.data.InputFrame>();
                foreach (ClientState otherState in m_clientStates.Values)
                {
                    PlayerState currentPlayer = otherState.GetPlayer();
                    if (baseState.PlayerGUID != otherState.PlayerGUID)
                    {
                        baseState.AddPlayer(currentPlayer);
                    }
                }
            }
            
            foreach (ServerGameplayStateUpdater updater in m_updaters)
            {
                updater.Setup();
            }

            foreach (ClientState state in m_clientStates.Values)
            {
                foreach(ServerGameplayStateUpdater updater in m_updaters)
                {
                    updater.InitClient(state);
                }
            }

            foreach (ClientState state in m_clientStates.Values)
            {
                foreach (ServerGameplayStateUpdater updater in m_updaters)
                {
                    updater.InitPlayer(state.GetPlayer());
                }
            }
            
            m_UDPServer.Subscribe(this);
            m_TCPServer.Subscribe(this);
        }
                
        protected override void StateFixedUpdate()
        {
            lock (m_lock)
            {
                if (++m_bufferedMasterTick > m_simulationBuffer + m_masterTick)
                {
                    foreach (int id in m_clientStates.Keys)
                    {
                        if (!m_connectedClients[id])
                            continue;

                        ClientState client = m_clientStates[id];
                        // if input buffer has a frame corresponding to this tick
                        InputFrame frame = null;
                        if(!m_clientInputBuffers[client].ContainsKey(m_masterTick))
                        {
#if DEBUG_LOG
                            Debug.Log("Missed a player input from " + id + " for tick " + m_masterTick);
#endif //DEBUG_LOG
                            // TODO : Cache new default frame ?
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

                    foreach (int id in m_clientStates.Keys)
                    {
                        if (!m_connectedClients[id])
                            continue;

                        ClientState client = m_clientStates[id];
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
                    foreach (int id in m_connectedClients.Keys)
                    {
                        if (m_connectedClients[id])
                        {
                            m_UDPServer.Send(m_clientStates[id].GetBytes(), id);
                        }
                    }
                }
            }
        }

        public void Receive(udp.UDPToolkit.Packet packet, int playerID)
        {
            if (!m_connectedClients[playerID])
            {
#if DEBUG_LOG
                Debug.Log("Received UDP packet from unconnected client (ID " + playerID + "). Ignoring.");
                return;
#endif //DEBUG_LOG
            }

            InputMessage inputs = IConvertible.CreateFromBytes<InputMessage>(packet.Data.ArraySegment());
            if (inputs != null && m_clients.Contains(playerID))
            {
                lock (m_lock)
                {
                    ClientState clientState = m_clientStates[playerID];
                    List<InputFrame> inputFrames = inputs.InputFrames.Value;
#if DEBUG_LOG
                    Debug.Log("(NOW = " + m_masterTick + ") Received tick " + inputs.StartTick.Value + " to " +  (inputs.StartTick.Value + inputFrames.Count) +  " from " + clientState.PlayerGUID);
#endif //DEBUG_LOG

                    int frameIndex = 0;
                    for (int i = 0; i < inputFrames.Count; i++)
                    {
                        frameIndex = (int)inputs.StartTick.Value + i;
                        if (frameIndex >= m_masterTick)
                        {
                            m_clientInputBuffers[clientState][frameIndex] = inputFrames[i];
                        }
                    }
                }
            }
        }

        public void ReceivePacket(TCPToolkit.Packet packet, int playerID)
        {
            // if we receive ID message from player (who's trying to reconnect)
            // ...
            IdentificationMessage identification = Serializable.CreateFromBytes<IdentificationMessage>(packet.Data.ArraySegment());
            if (identification != null)
            {
#if DEBUG_LOG
                Debug.Log("Player " + playerID + " successfully connected and identified. Rejoining.");
#endif // DEBUG_LOG
                m_connectedClients[playerID] = true;
            }
        }

        public void OnConnect(int playerID)
        {
#if DEBUG_LOG
            Debug.Log("Player " + playerID + " just connected. Awaiting identification packet.");
#endif // DEBUG_LOG
        }

        public void OnDisconnect(int playerID)
        {
#if DEBUG_LOG
            Debug.Log("Player " + playerID + " disconnected");
            m_connectedClients[playerID] = false;
#endif // DEBUG_LOG
        }
    }
}
