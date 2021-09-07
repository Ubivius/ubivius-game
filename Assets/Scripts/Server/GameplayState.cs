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
    /// https://www.gabrielgambetta.com/client-server-game-architecture.html
    /// </summary>
    public class GameplayState : ServerState, udp.server.IUDPServerReceiver, tcp.server.ITCPServerReceiver
    {
        private HashSet<int> m_clients;
        private Dictionary<int, ClientState> m_clientStates;
        private Dictionary<int, int> m_clientTick;
        private Dictionary<int, bool> m_connectedClients;
                
        private Dictionary<int, Dictionary<int, InputFrame>> m_clientInputBuffers;
        
        [SerializeField] private uint m_snapshotTicks;
        [SerializeField] private string m_physicsSceneName;

        private uint m_tickAccumulator;

        private PhysicsScene2D m_serverPhysics;
        
        [SerializeField] private List<ServerGameplayStateUpdater> m_updaters;

        private InputFrame m_zeroFrame;

        protected override void StateAwake()
        {
            ServerState.m_gameplayState = this;
        }

        public void Init(Dictionary<int, common.serialization.types.String> clients)
        {
            m_tickAccumulator = 0;
            m_clientTick = new Dictionary<int, int>();
            m_clientStates = new Dictionary<int, ClientState>();
            m_clients = new HashSet<int>(clients.Keys);

            m_connectedClients = new Dictionary<int, bool>();

            m_zeroFrame = new InputFrame();

            m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(m_physicsSceneName).GetPhysicsScene2D();
            
            m_clientInputBuffers = new Dictionary<int, Dictionary<int, InputFrame>>();
                   
            // add each player to client states
            foreach (int id in m_clients)
            {
                PlayerState player = new PlayerState();
                player.GUID.Value = id;
                m_clientStates[id] = new ClientState(id);
                m_clientStates[id].AddPlayer(player);
                m_clientTick[id] = 0;
                m_connectedClients.Add(id, true);
            }

            // add each player to each other client state
            foreach (ClientState baseState in m_clientStates.Values)
            {
                m_clientInputBuffers[baseState.PlayerGUID] = new Dictionary<int, InputFrame>();
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
                if (++m_tickAccumulator > m_snapshotTicks)
                {
                    for(int t = 0; t < m_tickAccumulator; t++)
                    {
                        // update state based on received input
                        foreach (int id in m_clientStates.Keys)
                        {
                            if (!m_connectedClients[id])
                                continue;

                            int missingTicks = (int)m_snapshotTicks - m_clientInputBuffers[id].Count;
                            if(missingTicks < 0)
                            {
                                missingTicks = 0;
                            }
                            
                            int tick = m_clientTick[id] + t - missingTicks;

                            // if input buffer has a frame corresponding to this tick
                            InputFrame frame = m_zeroFrame; // create a default frame to not move player;
                            if (!m_clientInputBuffers[id].ContainsKey(tick)) // soit on l'a miss soit on a pas assez d'inputs
                            {
#if DEBUG_LOG
                                Debug.Log("Missed a player input from " + id + " for tick " + tick);
#endif //DEBUG_LOG
                            }
                            else
                            {
                                frame = m_clientInputBuffers[id][tick];
                                m_clientInputBuffers[id].Remove(tick);
                            }
                            

                            ClientState client = m_clientStates[id];
                            if (tick > 0)
                            {
                                client.Tick.Value = (uint)tick;
                            }
                            // must be called in main unity thread
                            foreach (ServerGameplayStateUpdater updater in m_updaters)
                            {
                                updater.FixedUpdateFromClient(client, frame, Time.fixedDeltaTime);
                            }
                        }

                        m_serverPhysics.Simulate(Time.fixedDeltaTime);
                        
                        // update client states based on simulation
                        foreach (int id in m_clientStates.Keys)
                        {
                            ClientState client = m_clientStates[id];
                            foreach (ServerGameplayStateUpdater updater in m_updaters)
                            {
                                updater.UpdateClient(client);
                            }
                        }
                    }
                    
                    foreach (int id in m_connectedClients.Keys)
                    {
                        if (m_connectedClients[id])
                        {
                            m_UDPServer.Send(m_clientStates[id].GetBytes(), id);
                        }
                    }
                    m_tickAccumulator = 0;
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
                    Debug.Log("Received ticks " + inputs.StartTick.Value + " to " + (inputs.StartTick.Value + inputFrames.Count) + " from " + clientState.PlayerGUID);
#endif //DEBUG_LOG
                    m_clientTick[playerID] = (int)inputs.StartTick.Value;// + (uint)inputFrames.Count;
                    int frameIndex = 0;
                    for (int i = 0; i < inputFrames.Count; i++)
                    {
                        frameIndex = (int)inputs.StartTick.Value + i;
                        // fill in the blanks (the missed or new input frames)
                        if (!m_clientInputBuffers[playerID].ContainsKey(frameIndex))
                        {
                            m_clientInputBuffers[playerID][frameIndex] = inputFrames[i];
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
            // DisconnectPlayer() // pour gérer la déco?
#endif // DEBUG_LOG
        }
    }
}
