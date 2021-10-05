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
using System;
using ubv.utils;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the state of the server during the game
    /// </summary>
    public class ClientSyncPlay : ClientSyncState, udp.client.IUDPClientReceiver, tcp.client.ITCPClientReceiver
    {
        [SerializeField] private string m_physicsScene;

        public ClientGameInfo GameInfo { get; private set; }
        
        private int m_lastReceivedRemoteTick;
        private int m_localTick;

        private float m_baseTickTime;
        private float m_fixedUpdateDeltaTime;
        private float m_meanRTT;
        
        private const ushort CLIENT_STATE_BUFFER_SIZE = 64;

        private ClientState[] m_clientStateBuffer;
        private InputFrame[] m_inputBuffer;
        private InputFrame m_lastInput;
        private ClientState m_lastReceivedServerState;

        private Dictionary<int, long> m_packetSendTimeStamps;

        private int m_goalOffset;
        [SerializeField]
        private float m_clockOffsetCorrectionIncrement = 0.05f;
        [SerializeField]
        private int m_offsetErrorTolerance = 1;
        [SerializeField]
        private int m_offsetErrorMaxDuration = 3;
        private int m_offsetErrorDuration;

        private PhysicsScene2D m_clientPhysics;

        [SerializeField] private float m_defaultRTTEstimate = 0.050f;
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

        public void Init(List<int> playerIDs, ClientGameInfo gameInfo)
        {
            m_localTick = 0;
            m_goalOffset = 0;
            m_offsetErrorDuration = 0;
            m_packetSendTimeStamps = new Dictionary<int, long>();
            m_meanRTT = m_defaultRTTEstimate;
            m_clientStateBuffer = new ClientState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer = new InputFrame[CLIENT_STATE_BUFFER_SIZE];

            m_clientPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_lastReceivedServerState = null;
            m_lastReceivedRemoteTick = 0;

            m_fixedUpdateDeltaTime = Time.fixedDeltaTime;
            m_baseTickTime = m_fixedUpdateDeltaTime;
            
            Initialized = false;
            
            GameInfo = gameInfo;

            List<PlayerState> playerStates = new List<PlayerState>();
            foreach (int id in playerIDs)
            {
                playerStates.Add(new PlayerState(id));
            }

            for (ushort i = 0; i < CLIENT_STATE_BUFFER_SIZE; i++)
            {
                m_clientStateBuffer[i] = new ClientState(playerStates);
                m_clientStateBuffer[i].PlayerGUID = PlayerID.Value;

                m_inputBuffer[i] = new InputFrame();
                m_inputBuffer[i].SetToNeutral();
            }

            foreach (ClientStateUpdater updater in m_updaters)
            {
                updater.Init(m_clientStateBuffer[0], PlayerID.Value);
            }

            m_UDPClient.Subscribe(this);
            m_TCPClient.Subscribe(this);

            UpdateClockOffset(LatencyFromRTT(m_meanRTT));

            Initialized = true;
            OnInitializationDone?.Invoke();
        }

        protected override void StateFixedUpdate()
        {
            lock (m_lock)
            {
                Time.fixedDeltaTime = m_fixedUpdateDeltaTime;
            }

            if (!ShouldUpdate())
                return;


            UpdateClockOffset(LatencyFromRTT(m_meanRTT));

            int bufferIndex = m_localTick % CLIENT_STATE_BUFFER_SIZE;

            UpdateInput(bufferIndex);

            if (Time.frameCount % 4 == 0)
            {
                ManageRTTCheck(m_localTick);
            }

            UpdateClientState(bufferIndex);
            
            ++m_localTick;
            ClientCorrection(m_lastReceivedRemoteTick % CLIENT_STATE_BUFFER_SIZE);

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

        private void UpdateStateFromWorldAndStep(ref ClientState state, InputFrame input, float deltaTime)
        {
            if (!ShouldUpdate())
                return;

            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].UpdateStateFromWorld(ref state);
                m_updaters[i].Step(input, deltaTime);
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

        private int MeanLatencyFromRTTMessage(RTTMessage msg)
        {
            long RTTInSystemTicks = DateTime.UtcNow.Ticks - m_packetSendTimeStamps[msg.Tick.Value];
            m_packetSendTimeStamps.Remove(msg.Tick.Value);
            float RTT = (float)RTTInSystemTicks / TimeSpan.TicksPerSecond;
                    
            const float weight = 0.3f;
            m_meanRTT = (m_meanRTT * weight) + (RTT * (1f - weight));

            return LatencyFromRTT(m_meanRTT);
        }

        private int LatencyFromRTT(float RTT)
        {
            return Mathf.RoundToInt(RTT / m_fixedUpdateDeltaTime);
        }

        private void UpdateClockOffset(int latencyInTicks, int baseOffset = ubv.server.logic.ServerNetworkingManager.SERVER_TICK_BUFFER_SIZE)
        {
            m_goalOffset = baseOffset + latencyInTicks;

            int clockOffset = m_localTick - m_lastReceivedRemoteTick;

            if(clockOffset < 0)
            {
                for (int i = m_localTick; i < m_lastReceivedRemoteTick + m_goalOffset; i++)
                {
                    InputFrame frame = m_inputBuffer[i % CLIENT_STATE_BUFFER_SIZE];
                    frame.Info.Tick.Value = i;
                    frame.SetToNeutral();
                }
                m_localTick = m_lastReceivedRemoteTick + m_goalOffset;
#if DEBUG_LOG
                Debug.Log("CLIENT Client is late. New clock offset : " + m_goalOffset + ". Local tick is now " + m_localTick);
#endif // DEBUG_LOG
            }
            else
            {
                int offsetError = m_goalOffset - clockOffset;
                
                if(offsetError < -m_offsetErrorTolerance)
                {
                    m_offsetErrorDuration++;
                    if (m_offsetErrorDuration > m_offsetErrorMaxDuration)
                    {
#if DEBUG_LOG
                        if (m_fixedUpdateDeltaTime < m_baseTickTime * (1f + m_clockOffsetCorrectionIncrement))
                        {
                            Debug.Log("CLIENT Client too far behind. Speeding up in order to reach offset " + m_goalOffset);
                        }
#endif // DEBUG_LOG
                        m_fixedUpdateDeltaTime = m_baseTickTime * (1f + m_clockOffsetCorrectionIncrement);
                    }
                }
                else if (offsetError > m_offsetErrorTolerance)
                {
                    m_offsetErrorDuration++;
                    if (m_offsetErrorDuration > m_offsetErrorMaxDuration)
                    {
#if DEBUG_LOG
                        if (m_fixedUpdateDeltaTime > m_baseTickTime * (1f - m_clockOffsetCorrectionIncrement))
                        {
                            Debug.Log("CLIENT Client too far in future. Slowing down in order to reach offset " + m_goalOffset);
                        }
#endif // DEBUG_LOG
                        m_fixedUpdateDeltaTime = m_baseTickTime * (1f - m_clockOffsetCorrectionIncrement);
                    }
                }
                else
                {
                    m_offsetErrorDuration = 0;
#if DEBUG_LOG
                    if (m_fixedUpdateDeltaTime != m_baseTickTime)
                    {
                        Debug.Log("CLIENT Back in sync with server at offset " + clockOffset);
                    }
#endif // DEBUG_LOG
                    m_fixedUpdateDeltaTime = m_baseTickTime;
                }

                Mathf.Clamp(m_fixedUpdateDeltaTime, m_baseTickTime * 0.5f, m_baseTickTime * 2f);
            }
        }

        private void ManageRTTCheck(int tick)
        {
            m_packetSendTimeStamps[tick] = DateTime.UtcNow.Ticks;
            m_UDPClient.Send(new RTTMessage(m_packetSendTimeStamps[tick], tick).GetBytes(), PlayerID.Value);
        }

        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            if (!ShouldUpdate() )
                return;
            
            lock (m_lock)
            {
                ClientStateMessage stateMessage = common.serialization.IConvertible.CreateFromBytes<ClientStateMessage>(packet.Data.ArraySegment());
                
                if (stateMessage != null)// && state.Tick.Value > m_lastReceivedRemoteTick)
                {
                    m_lastReceivedRemoteTick = stateMessage.Info.Tick.Value;
                    
                    ClientState state = stateMessage.State;
                    state.PlayerGUID = PlayerID.Value;
                    m_lastReceivedServerState = state;
                    
#if DEBUG_LOG
                    //Debug.Log("Received server state tick " + state.Tick.Value + ", local tick is " + m_localTick);
#endif //DEBUG_LOG
                }
                else
                {
                    RTTMessage rttMsg = common.serialization.IConvertible.CreateFromBytes<RTTMessage>(packet.Data.ArraySegment());
                    if(rttMsg != null)
                    {
                        UpdateClockOffset(MeanLatencyFromRTTMessage(rttMsg));
                    }
                }
            }
        }

        private void UpdateInput(int bufferIndex)
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

            m_inputBuffer[bufferIndex].Info.Tick.Value = m_localTick;

            m_lastInput = null;
            
            List<InputFrame> frames = new List<InputFrame>();
            for (int tick = m_lastReceivedRemoteTick + 1; tick <= m_localTick; tick++)
            {
                frames.Add(m_inputBuffer[tick % CLIENT_STATE_BUFFER_SIZE]);
            }

            InputMessage inputMessage = new InputMessage();

            inputMessage.PlayerID.Value = PlayerID.Value;
            inputMessage.InputFrames.Value = frames;

#if NETWORK_SIMULATE
            if (UnityEngine.Random.Range(0f, 1f) > m_packetLossChance)
            {
                //Debug.Log("CLIENT Sending ticks " + (m_lastReceivedRemoteTick  + 1) + " to " + m_localTick);
                m_UDPClient.Send(inputMessage.GetBytes(), PlayerID.Value);
            }
            else
            {
                Debug.Log("SIMULATING PACKET LOSS");
            }
#else
            m_udpClient.Send(inputMessage.GetBytes(), PlayerID.Value);
#endif //NETWORK_SIMULATE       
                    
        }

        private bool ShouldUpdate()
        {
            return Initialized && ConnectedToServer;
        }

        private void UpdateClientState(int bufferIndex)
        {
            if (!ShouldUpdate())
                return;
            // set current client state to last one then updating it
            UpdateStateFromWorldAndStep(
                ref m_clientStateBuffer[bufferIndex],
                m_inputBuffer[bufferIndex],
                Time.fixedDeltaTime);
        }

        private void ClientCorrection(int remoteIndex)
        {
            if (!ShouldUpdate())
                return;
            
            // receive a state from server
            // check what tick it corresponds to
            // rewind client state to the tick
            // replay up to local tick by stepping every tick
            lock (m_lock)
            {
                if (m_lastReceivedServerState != null)
                {
                    // on devrait quand même reset les shits qui ont pas besoin de correction
                    // parce qu'on call phys simulate et ça ça affecte des updaters
                    // qui ne devraient peut-être pas être affectés
                    // sol 1: reset all
                    // sol 2: corriger direct sans simulate pour les updaters qui n'ont pas besoin de phy sim
                    // et quand on doit caller phy sim, on reset tous les physical updaters
                    List<ClientStateUpdater> updaters = UpdatersNeedingCorrection(m_clientStateBuffer[remoteIndex], m_lastReceivedServerState);
                    if (updaters.Count > 0) 
                    {
                        int rewindTicks = m_lastReceivedRemoteTick;
                                
                        // reset world state to last server-sent state
                        // pour le moment, on prend l'option 1 et on reset all
                        for (int i = 0; i < m_updaters.Count; i++)
                        {
                            m_updaters[i].UpdateWorldFromState(m_lastReceivedServerState);
                        }

                        Debug.Log("CORRECTION : Remote ticks : " + m_lastReceivedRemoteTick + ". Local ticks : " + m_localTick + ". Diff = " + (m_localTick - m_lastReceivedRemoteTick));

                        while (rewindTicks < m_localTick)
                        {
                            int rewindIndex = rewindTicks++ % CLIENT_STATE_BUFFER_SIZE;

                            for (int i = 0; i < m_updaters.Count; i++)
                            {
                                m_updaters[i].UpdateStateFromWorld(ref m_clientStateBuffer[rewindIndex]);
                                m_updaters[i].Step(m_inputBuffer[rewindIndex], Time.fixedDeltaTime);
                            }

                            m_clientPhysics.Simulate(Time.fixedDeltaTime);
                        }
                    }
                            
                    m_lastReceivedServerState = null;
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
