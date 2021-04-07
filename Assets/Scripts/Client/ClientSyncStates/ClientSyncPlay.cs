using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using ubv.common;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the state of the server during the game
    /// </summary>
    public class ClientSyncPlay : ClientSyncState, udp.client.IUDPClientReceiver, tcp.client.ITCPClientReceiver
    {
        [SerializeField] private string m_physicsScene;

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

        private bool m_initialized;
        private bool ConnectedToServer { get { return m_TCPClient.IsConnected(); } }

        [SerializeField] private float m_reconnectTryDelayMS = 2000;
        private float m_reconnectTryTimer;

#if NETWORK_SIMULATE
        [SerializeField] private float m_packetLossChance = 0.15f;
#endif // NETWORK_SIMULATE

        protected override void StateAwake()
        {
            ClientSyncState.m_playState = this;
        }

        public void Init(int simulationBuffer, List<PlayerState> playerStates)
        {
            m_localTick = 0;
            m_reconnectTryTimer = 0;
            m_clientStateBuffer = new ClientState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer = new InputFrame[CLIENT_STATE_BUFFER_SIZE];

            m_clientPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_lastServerState = null;
            
            m_initialized = false;
            
            m_simulationBuffer = simulationBuffer;

            foreach (ClientStateUpdater updater in m_updaters)
            {
                updater.Init(playerStates, m_playerID.Value);
            }
            m_UDPClient.Subscribe(this);
            m_TCPClient.Subscribe(this);

            for (ushort i = 0; i < CLIENT_STATE_BUFFER_SIZE; i++)
            {
                m_clientStateBuffer[i] = new ClientState();
                m_clientStateBuffer[i].PlayerGUID = m_playerID.Value;

                foreach (PlayerState playerState in playerStates)
                {
                    PlayerState player = new PlayerState(playerState);

                    m_clientStateBuffer[i].AddPlayer(player);
                }

                m_inputBuffer[i] = new InputFrame();
            }
            m_initialized = true;
        }

        protected override void StateFixedUpdate()
        {
            if (!m_initialized && !ConnectedToServer)
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
            if (!m_initialized)
                return;

            if (ConnectedToServer)
            {
                m_lastInput = InputController.CurrentFrame();
            }
            else
            {
                // if not connected : try to reconnect every x second with ID message 
                m_reconnectTryTimer += Time.deltaTime;
                if(m_reconnectTryTimer > m_reconnectTryDelayMS)
                {
#if DEBUG_LOG
                    Debug.Log("Trying to reconnect to server...");
#endif //DEBUG_LOG
                    m_TCPClient.Reconnect();
                    m_reconnectTryTimer = 0;
                }
            }

        }

        public void RegisterUpdater(ClientStateUpdater updater)
        {
            if (!m_initialized)
                return;

            m_updaters.Add(updater);
        }

        private void StoreCurrentStateAndStep(ref ClientState state, InputFrame input, float deltaTime)
        {
            if (!m_initialized && !ConnectedToServer)
                return;

            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].SetStateAndStep(ref state, input, deltaTime);
            }
                    
            m_clientPhysics.Simulate(deltaTime);
        }
                
        private List<ClientStateUpdater> UpdatersNeedingCorrection(ClientState localState, ClientState remoteState)
        {
            if (!m_initialized && !ConnectedToServer)
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
            if (!m_initialized && !ConnectedToServer)
                return;

            // TODO remove tick from ClientSTate and add it to custom server state packet?
            // client doesnt need its own client state ticks
            lock (m_lock)
            {
                ClientState state = common.serialization.IConvertible.CreateFromBytes<ClientState>(packet.Data.ArraySegment());
                if (state != null)
                {
                    state.PlayerGUID = m_playerID.Value;
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
                    if(m_localTick > m_remoteTick + (uint)m_simulationBuffer)
                    {
                        m_localTick = m_remoteTick ;
                    }
                }
            }
        }

        private void UpdateInput(uint bufferIndex)
        {
            if (!m_initialized && !ConnectedToServer)
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
            for (uint tick = (uint)Mathf.Max((int)m_remoteTick, (int)m_localTick - (m_simulationBuffer * 2)); tick <= m_localTick; tick++)
            {
                frames.Add(m_inputBuffer[tick % CLIENT_STATE_BUFFER_SIZE]);
            }

            InputMessage inputMessage = new InputMessage();

            inputMessage.PlayerID.Value = m_playerID.Value;
            inputMessage.StartTick.Value = m_remoteTick;
            inputMessage.InputFrames.Value = frames;

#if NETWORK_SIMULATE
            if (Random.Range(0f, 1f) > m_packetLossChance)
            {
                m_UDPClient.Send(inputMessage.GetBytes(), m_playerID.Value);
            }
            else
            {
                Debug.Log("SIMULATING PACKET LOSS");
            }
#else
            m_udpClient.Send(inputMessage.ToBytes());
#endif //NETWORK_SIMULATE       
                    
        }

        private void UpdateClientState(uint bufferIndex)
        {
            if (!m_initialized && !ConnectedToServer)
                return;
            // set current client state to last one then updating it
            StoreCurrentStateAndStep(
                ref m_clientStateBuffer[bufferIndex],
                m_inputBuffer[bufferIndex],
                Time.fixedDeltaTime);
        }

        private void ClientCorrection(uint remoteIndex)
        {
            if (!m_initialized)
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

        public void OnSuccessfulConnect()
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
