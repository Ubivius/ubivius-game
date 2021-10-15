using System.Collections.Generic;
using System.Net;
using ubv.common;
using ubv.common.data;
using ubv.common.serialization;
using ubv.tcp;
using ubv.utils;
using UnityEngine;

namespace ubv.server.logic
{
    /// <summary>
    /// Represents the state of the server during the game
    /// https://www.gabrielgambetta.com/client-server-game-architecture.html
    /// </summary>
    public class GameplayState : ServerState
    {
        private HashSet<int> m_clients;
        private WorldState m_currentWorldState;
        private int m_masterTick;
        private Dictionary<int, bool> m_connectedClients;
                
        private Dictionary<int, Dictionary<int, InputFrame>> m_clientInputBuffers;
        private Dictionary<int, InputFrame> m_currentInputFrames;
        
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

        public void Init(ICollection<int> clients)
        {
            m_tickAccumulator = 0;
            m_masterTick = 0;
            m_currentWorldState = new WorldState();
            m_clients = new HashSet<int>(clients);

            m_currentInputFrames = new Dictionary<int, InputFrame>();

            m_connectedClients = new Dictionary<int, bool>();

            m_zeroFrame = new InputFrame();

            m_serverPhysics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(m_physicsSceneName).GetPhysicsScene2D();
            
            m_clientInputBuffers = new Dictionary<int, Dictionary<int, InputFrame>>();
                   
            // add each player to client states
            foreach (int id in m_clients)
            {
                PlayerState player = new PlayerState();
                player.GUID.Value = id;
                m_currentWorldState.AddPlayer(player);
                m_clientInputBuffers[id] = new Dictionary<int, InputFrame>();
                m_connectedClients.Add(id, true);
            }
            
            foreach (ServerGameplayStateUpdater updater in m_updaters)
            {
                updater.Setup();
            }
            
            foreach(ServerGameplayStateUpdater updater in m_updaters)
            {
                updater.InitWorld(m_currentWorldState);
            }
            
            m_UDPServer.Subscribe(this);
            m_TCPServer.Subscribe(this);
        }
                
        protected override void StateFixedUpdate()
        {
            lock (m_lock)
            {
                // update state based on received input
                foreach (int id in m_clients)
                {
                    if (!m_connectedClients[id])
                        continue;

                    if (!m_clientInputBuffers[id].ContainsKey(m_masterTick))
                    {
                        Debug.Log("SERVER Missed input " + m_masterTick + " from client " + id);
                    }

                    // zero OR duplicate last frame ?
                    // duplicate implies future correction of inputs
                    InputFrame frame = m_clientInputBuffers[id].ContainsKey(m_masterTick) ?
                        m_clientInputBuffers[id][m_masterTick] : 
                        m_zeroFrame;

                    m_currentInputFrames[id] = frame;

                    // remove used entries in dict if we use a dict later
                    if(m_clientInputBuffers[id].ContainsKey(m_masterTick))
                       m_clientInputBuffers[id].Remove(m_masterTick);
                }

                // must be called in main unity thread
                foreach (ServerGameplayStateUpdater updater in m_updaters)
                {
                    updater.FixedUpdateFromClient(m_currentWorldState, m_currentInputFrames, Time.fixedDeltaTime);
                }

                m_serverPhysics.Simulate(Time.fixedDeltaTime);
                
                foreach (ServerGameplayStateUpdater updater in m_updaters)
                {
                    updater.UpdateWorld(m_currentWorldState);
                }
                
                m_masterTick++;
                if (++m_tickAccumulator >= m_snapshotTicks)
                {
                    foreach (int id in m_connectedClients.Keys)
                    {
                        if (m_connectedClients[id])
                        {
                            // OPTIMIZATION : Cache le message et l'info au lieu d'en recréer des new à chaque send
                            NetInfo info = new NetInfo(m_masterTick);
                            ClientStateMessage msg = new ClientStateMessage(m_currentWorldState, info);
                            //Debug.Log("SERVER Sending validated tick " + m_masterTick + " to client " + id);
                            m_UDPServer.Send(msg.GetBytes(), id);
                        }
                    }
                    m_tickAccumulator = 0;
                }
            }
        }

        public override void UDPReceive(udp.UDPToolkit.Packet packet, int playerID)
        {
            if (!m_connectedClients[playerID])
            {
#if DEBUG_LOG
                Debug.Log("Received UDP packet from unconnected client (ID " + playerID + "). Ignoring.");
#endif //DEBUG_LOG
                return;
            }

            InputMessage inputs = IConvertible.CreateFromBytes<InputMessage>(packet.Data.ArraySegment());
            if (inputs != null && m_clients.Contains(playerID))
            {
                lock (m_lock)
                {
                    List<InputFrame> inputFrames = inputs.InputFrames.Value;
                    int frameIndex = 0;
                    
                    for (int i = 0; i < inputFrames.Count; i++)
                    {
                        frameIndex = (int)inputFrames[i].Info.Tick.Value;
                        if (frameIndex >= m_masterTick)
                        {
                            m_clientInputBuffers[playerID][frameIndex] = inputFrames[i];
                        }
                    }
                }
            }
            else
            {
                RTTMessage rttMsg = common.serialization.IConvertible.CreateFromBytes<RTTMessage>(packet.Data.ArraySegment());
                if (rttMsg != null)
                {
                    // TODO cache bytes + add check "isRttMessage" that checks the object enum type value
                    m_UDPServer.Send(rttMsg.GetBytes(), playerID);
                }
            }
        }

        public override void TCPReceive(TCPToolkit.Packet packet, int playerID)
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

        protected override void OnPlayerConnect(int playerID)
        {
#if DEBUG_LOG
            Debug.Log("Player " + playerID + " just connected. Awaiting identification packet.");
#endif // DEBUG_LOG
        }

        protected void OnPlayerDisconnect(int playerID)
        {
#if DEBUG_LOG
            Debug.Log("Player " + playerID + " disconnected");
            m_connectedClients[playerID] = false;
            // DisconnectPlayer() // pour gérer la déco?
#endif // DEBUG_LOG
        }
    }
}
