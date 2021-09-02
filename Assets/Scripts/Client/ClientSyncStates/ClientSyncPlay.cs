using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using ubv.common;
using UnityEngine.Events;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the state of the server during the game
    /// </summary>
    public class ClientSyncPlay : ClientSyncState, udp.client.IUDPClientReceiver, tcp.client.ITCPClientReceiver
    {
        [SerializeField] private string m_physicsScene;

        public ClientGameInfo GameInfo { get; private set; }

        private int m_simulationBuffer;
        private uint m_remoteTick;
        private uint m_localTick;

        private const ushort CLIENT_STATE_BUFFER_SIZE = 64;

        private ClientState[] m_clientStateBuffer;
        private InputFrame[] m_inputBuffer;
        private InputFrame m_lastInput;
        private ClientState m_lastServerState;

        private PhysicsScene2D m_clientPhysics;

        [SerializeField] private List<ClientStateUpdater> m_updaters;

        public bool Initialized { get; private set; }
        private bool ConnectedToServer { get { return m_TCPClient.IsConnected(); } }
        
        public UnityAction OnInitializationDone;

#if NETWORK_SIMULATE
        [SerializeField] private float m_packetLossChance = 0.15f;
#endif // NETWORK_SIMULATE

        protected override void StateAwake()
        {
            ClientSyncState.m_playState = this;
        }

        public void Init(int simulationBuffer, List<int> playerIDs, ClientGameInfo gameInfo)
        {
            m_localTick = 0;
            m_clientStateBuffer = new ClientState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer = new InputFrame[CLIENT_STATE_BUFFER_SIZE];

            m_clientPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_lastServerState = null;
            
            Initialized = false;
            
            m_simulationBuffer = simulationBuffer;

            GameInfo = gameInfo;

            List<PlayerState> playerStates = new List<PlayerState>();
            foreach (int id in playerIDs)
            {
                playerStates.Add(new PlayerState(id));
            }

            foreach (ClientStateUpdater updater in m_updaters)
            {
                updater.Init(playerStates, PlayerID.Value);
            }

            m_UDPClient.Subscribe(this);
            m_TCPClient.Subscribe(this);

            for (ushort i = 0; i < CLIENT_STATE_BUFFER_SIZE; i++)
            {
                m_clientStateBuffer[i] = new ClientState();
                m_clientStateBuffer[i].PlayerGUID = PlayerID.Value;

                foreach (PlayerState playerState in playerStates)
                {
                    PlayerState player = new PlayerState(playerState);

                    m_clientStateBuffer[i].AddPlayer(player);
                }

                m_inputBuffer[i] = new InputFrame();
            }
            Initialized = true;
            OnInitializationDone?.Invoke();
        }

        protected override void StateFixedUpdate()
        {
            if (!ShouldUpdate())
                return;

            uint bufferIndex = m_localTick % CLIENT_STATE_BUFFER_SIZE;

            UpdateInput(bufferIndex);

            UpdateClientState(bufferIndex);

            ++m_localTick;

            ClientCorrection(m_remoteTick % CLIENT_STATE_BUFFER_SIZE);

            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].FixedStateUpdate(Time.deltaTime);
            }
        }

        protected override void StateUpdate()
        {
            if (!Initialized)
                return;

            if (ConnectedToServer)
            {
                m_lastInput = InputController.CurrentFrame();
            }
        }

        public void RegisterUpdater(ClientStateUpdater updater)
        {
            if (!Initialized)
                return;

            m_updaters.Add(updater);
        }

        private void StoreCurrentStateAndStep(ref ClientState state, InputFrame input, float deltaTime)
        {
            if (!ShouldUpdate())
                return;

            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].SetStateAndStep(ref state, input, deltaTime);
            }
                    
            m_clientPhysics.Simulate(deltaTime);
        }
                
        private List<ClientStateUpdater> UpdatersNeedingCorrection(ClientState localState, ClientState remoteState)
        {
            if (!ShouldUpdate())
                return null;

            List<ClientStateUpdater> needCorrection = new List<ClientStateUpdater>();

            for (int i = 0; i < m_updaters.Count; i++)
            {
                if (m_updaters[i].NeedsCorrection(localState, remoteState))
                {
                    needCorrection.Add(m_updaters[i]);
                }
            }

            return needCorrection;
        }

        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            if (!ShouldUpdate())
                return;

            // TODO remove tick from ClientSTate and add it to custom server state packet?
            // client doesnt need its own client state ticks
            lock (m_lock)
            {
                ClientState state = common.serialization.IConvertible.CreateFromBytes<ClientState>(packet.Data.ArraySegment());
                if (state != null)
                {
                    state.PlayerGUID = PlayerID.Value;
                    m_lastServerState = state;
#if DEBUG_LOG
                    Debug.Log("Received server state tick " + state.Tick.Value);
#endif //DEBUG_LOG
                    m_remoteTick = state.Tick.Value;

                    if(m_localTick < m_remoteTick)
                    {
#if DEBUG_LOG
                        Debug.Log("Client has fallen behind by " + (m_remoteTick - m_localTick) + ". Fast-forwarding.");
#endif //DEBUG_LOG
                        m_localTick = m_remoteTick + (uint)m_simulationBuffer;
                    }

                    // PATCH FOR JITTER (too many phy simulate calls)
                    // TODO: investigate (si le temps le permet)
                    // ClientCorrection()
                    /*if(m_localTick > m_remoteTick + (uint)m_simulationBuffer)
                    {
                        m_localTick = m_remoteTick ;
                    }*/
                }
            }
        }

        private void UpdateInput(uint bufferIndex)
        {
            if (!ShouldUpdate())
                return;

            if (m_lastInput != null)
            {
                m_inputBuffer[bufferIndex].Movement.Value = m_lastInput.Movement.Value;
                m_inputBuffer[bufferIndex].Sprinting.Value = m_lastInput.Sprinting.Value;
            }
            else
            {
                m_inputBuffer[bufferIndex].SetToNeutral();
            }

            m_inputBuffer[bufferIndex].Tick.Value = m_localTick;

            m_lastInput = null;
            
            List<common.data.InputFrame> frames = new List<common.data.InputFrame>();
            for (uint tick = m_remoteTick; tick <= m_localTick; tick++)
            {
                frames.Add(m_inputBuffer[tick % CLIENT_STATE_BUFFER_SIZE]);
            }

            InputMessage inputMessage = new InputMessage();

            inputMessage.PlayerID.Value = PlayerID.Value;
            inputMessage.StartTick.Value = m_remoteTick;
            inputMessage.InputFrames.Value = frames;

#if NETWORK_SIMULATE
            if (Random.Range(0f, 1f) > m_packetLossChance)
            {
                m_UDPClient.Send(inputMessage.GetBytes(), PlayerID.Value);
            }
            else
            {
                Debug.Log("SIMULATING PACKET LOSS");
            }
#else
            m_udpClient.Send(inputMessage.ToBytes());
#endif //NETWORK_SIMULATE       
                    
        }

        private bool ShouldUpdate()
        {
            return Initialized && ConnectedToServer;
        }

        private void UpdateClientState(uint bufferIndex)
        {
            if (!ShouldUpdate())
                return;
            // set current client state to last one then updating it
            StoreCurrentStateAndStep(
                ref m_clientStateBuffer[bufferIndex],
                m_inputBuffer[bufferIndex],
                Time.fixedDeltaTime);
        }

        private void ClientCorrection(uint remoteIndex)
        {
            if (!ShouldUpdate())
                return;
            
            // receive a state from server
            // check what tick it corresponds to
            // rewind client state to the tick
            // replay up to local tick by stepping every tick
            lock (m_lock)
            {
                if (m_lastServerState != null)
                {
                    List<ClientStateUpdater> updaters = UpdatersNeedingCorrection(m_clientStateBuffer[remoteIndex], m_lastServerState);
                    if (updaters.Count > 0)
                    {
                        uint rewindTicks = m_remoteTick;
                                
                        // reset world state to last server-sent state
                        for (int i = 0; i < updaters.Count; i++)
                        {
                            updaters[i].UpdateFromState(m_lastServerState);
                        }

                        Debug.Log("Remote ticks : " + m_remoteTick + ". Local ticks : " + m_localTick + ". Diff = " + (m_localTick - m_remoteTick));

                        while (rewindTicks < m_localTick)
                        {
                            uint rewindIndex = rewindTicks++ % CLIENT_STATE_BUFFER_SIZE;

                            for (int i = 0; i < updaters.Count; i++)
                            {
                                updaters[i].SetStateAndStep(
                                ref m_clientStateBuffer[rewindIndex],
                                m_inputBuffer[rewindIndex],
                                Time.fixedDeltaTime);
                            }

                            m_clientPhysics.Simulate(Time.fixedDeltaTime);
                        }
                    }
                            
                    m_lastServerState = null;
                }
            }
        }

        public void OnSuccessfulTCPConnect()
        {
#if DEBUG_LOG
            Debug.Log("Successful connection to server.");
#endif // DEBUG_LOG
            m_TCPClient.Send(new IdentificationMessage().GetBytes());
        }

        public void ReceivePacket(TCPToolkit.Packet packet)
        {
            // empty for now
        }

        public void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Disconnected from server.");
#endif // DEBUG_LOG
        }
    }
}
