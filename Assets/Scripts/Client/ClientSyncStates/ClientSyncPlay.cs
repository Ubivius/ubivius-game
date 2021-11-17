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
    public class ClientSyncPlay : ClientSyncState
    {
        private enum SubState
        {
            SUBSTATE_WAITING_FOR_SERVER_GO,
            SUBSTATE_PLAY,
            SUBSTATE_OPTIONS,
            SUBSTATE_LEAVING,
            SUBSTATE_TRANSITION,
            SUBSTATE_END
        }

        [SerializeField] private string m_menuScene;
        [SerializeField] private string m_EndScene;
        [SerializeField] private string m_physicsScene;
        
        private int m_lastReceivedRemoteTick;
        private int m_localTick;

        private float m_baseTickTime;
        private float m_fixedUpdateDeltaTime;
        private float m_meanRTT;
        
        private const ushort CLIENT_STATE_BUFFER_SIZE = 64;

        private WorldState[] m_clientStateBuffer;

        private InputFrame[] m_inputBuffer;
        private InputFrame m_lastInput;
        private WorldState m_lastReceivedServerState;

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

        private SubState m_currentSubState;
        
        private bool ConnectedToServer { get { return m_server.IsConnected(); } }
        
        public UnityAction OnInitializationDone;

#if NETWORK_SIMULATE
        [SerializeField] private float m_packetLossChance = 0.15f;
#endif // NETWORK_SIMULATE

        protected override void StateLoad()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_GO;

            m_localTick = 0;
            m_goalOffset = 0;
            m_offsetErrorDuration = 0;
            m_packetSendTimeStamps = new Dictionary<int, long>();
            m_meanRTT = m_defaultRTTEstimate;
            m_clientStateBuffer = new WorldState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer = new InputFrame[CLIENT_STATE_BUFFER_SIZE];

            m_clientPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_lastReceivedServerState = null;
            m_lastReceivedRemoteTick = 0;

            m_fixedUpdateDeltaTime = Time.fixedDeltaTime;
            m_baseTickTime = m_fixedUpdateDeltaTime;
            m_server.OnTCPReceive += ReceiveTCP;
            m_server.OnUDPReceive += ReceiveUDP;
            m_server.OnServerDisconnect += OnDisconnect;
        }

        public override void OnStart()
        {
            Init(data.LoadingData.ServerInit);
        }

        private void Init(ServerInitMessage serverInit)
        {   
            for (ushort i = 0; i < CLIENT_STATE_BUFFER_SIZE; i++)
            {
                List<PlayerState> playerStates = new List<PlayerState>();
                foreach (int id in serverInit.PlayerCharacters.Value.Keys)
                {
                    playerStates.Add(new PlayerState(id));
                }
                m_clientStateBuffer[i] = new WorldState(playerStates);

                m_inputBuffer[i] = new InputFrame();
                m_inputBuffer[i].SetToNeutral();
            }

            foreach (ClientStateUpdater updater in m_updaters)
            {
                updater.Init(m_clientStateBuffer[0], CurrentUser.ID);
            }
            
            UpdateClockOffset(LatencyFromRTT(m_meanRTT));

            ClientWorldLoadedMessage worldLoaded = new ClientWorldLoadedMessage();
            m_server.TCPSend(worldLoaded.GetBytes());

            OnInitializationDone?.Invoke();
        }

        public override void StateFixedUpdate()
        {
            lock (m_lock)
            {
                Time.fixedDeltaTime = m_fixedUpdateDeltaTime;
            }

            switch (m_currentSubState)
            {
                case SubState.SUBSTATE_PLAY:
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
                    break;
                default:
                    break;
            }
        }

        public override void StateUpdate()
        {
            switch (m_currentSubState)
            {
                case SubState.SUBSTATE_PLAY:
                    m_lastInput = InputController.CurrentFrame();
                    break;
                case SubState.SUBSTATE_LEAVING:
                    ClientStateManager.Instance.BackToScene(m_menuScene);
                    m_currentSubState = SubState.SUBSTATE_TRANSITION;
                    break;
                case SubState.SUBSTATE_END:
                    ClientStateManager.Instance.PushScene(m_EndScene);
                    m_currentSubState = SubState.SUBSTATE_TRANSITION;
                    break;
                default:
                    break;
            }
        }
        
        private void UpdateStateFromWorldAndStep(ref WorldState state, InputFrame input, float deltaTime)
        {
            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].SaveSimulationInState(ref state);
                m_updaters[i].Step(input, deltaTime);
            }
            
            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].FixedStateUpdate(deltaTime);
            }

            m_clientPhysics.Simulate(deltaTime);
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
            m_server.UDPSend(new RTTMessage(m_packetSendTimeStamps[tick], tick).GetBytes());
        }

        private void ReceiveUDP(UDPToolkit.Packet packet)
        {
            lock (m_lock)
            {
                ClientStateMessage stateMessage = common.serialization.IConvertible.CreateFromBytes<ClientStateMessage>(packet.Data.ArraySegment());
                
                if (stateMessage != null)// && state.Tick.Value > m_lastReceivedRemoteTick)
                {
                    m_lastReceivedRemoteTick = stateMessage.Info.Tick.Value;
                    
                    WorldState state = stateMessage.State;
                    m_lastReceivedServerState = state;
                    
#if DEBUG_LOG
                    //Debug.Log("Received server state tick " + m_lastReceivedRemoteTick + ", local tick is " + m_localTick);
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
            if (m_lastInput != null)
            {
                m_inputBuffer[bufferIndex].Movement.Value = m_lastInput.Movement.Value;
                m_inputBuffer[bufferIndex].Sprinting.Value = m_lastInput.Sprinting.Value;
                m_inputBuffer[bufferIndex].Interact.Value = m_lastInput.Interact.Value;
                m_inputBuffer[bufferIndex].Shooting.Value = m_lastInput.Shooting.Value;
                m_inputBuffer[bufferIndex].ShootingDirection.Value = m_lastInput.ShootingDirection.Value;
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

            inputMessage.PlayerID.Value = CurrentUser.ID;
            inputMessage.InputFrames.Value = frames;

#if NETWORK_SIMULATE
            if (UnityEngine.Random.Range(0f, 1f) > m_packetLossChance)
            {
                //Debug.Log("CLIENT Sending ticks " + (m_lastReceivedRemoteTick  + 1) + " to " + m_localTick);
                m_server.UDPSend(inputMessage.GetBytes());
            }
            else
            {
                Debug.Log("SIMULATING PACKET LOSS");
            }
#else
            m_server.UDPSend(inputMessage.GetBytes(), PlayerID.Value);
#endif //NETWORK_SIMULATE       
                    
        }

        private void UpdateClientState(int bufferIndex)
        {
            // set current client state to last one then update it
            UpdateStateFromWorldAndStep(
                ref m_clientStateBuffer[bufferIndex],
                m_inputBuffer[bufferIndex],
                Time.fixedDeltaTime);
        }

        private void ClientCorrection(int remoteIndex)
        {
            // receive a state from server
            // check what tick it corresponds to
            // rewind client state to the tick
            // replay up to local tick by stepping every tick
            lock (m_lock)
            {
                if (m_lastReceivedServerState != null)
                {
                    HashSet<ClientStateUpdater> rightUpdaters = new HashSet<ClientStateUpdater>();
                    WorldState localState = m_clientStateBuffer[m_lastReceivedRemoteTick % CLIENT_STATE_BUFFER_SIZE];
                    for (int i = 0; i < m_updaters.Count; i++)
                    {
                        if (m_updaters[i].IsPredictionWrong(localState, m_lastReceivedServerState))
                        {
                            m_updaters[i].ResetSimulationToState(m_lastReceivedServerState);
                        }
                        else
                        {
                            m_updaters[i].UpdateSimulationFromState(localState, m_lastReceivedServerState);
                            rightUpdaters.Add(m_updaters[i]);
                        }
                    }
                    
                    foreach (ClientStateUpdater u in rightUpdaters)
                    {
                        u.DisableSimulation();
                    }

                    int rewindTicks = m_lastReceivedRemoteTick;
                    while (rewindTicks < m_localTick)
                    {
                        int rewindIndex = rewindTicks++ % CLIENT_STATE_BUFFER_SIZE;

                        for (int i = 0; i < m_updaters.Count; i++)
                        {
                            if(!rightUpdaters.Contains(m_updaters[i]))
                            {
                                m_updaters[i].SaveSimulationInState(ref m_clientStateBuffer[rewindIndex]);
                                m_updaters[i].Step(m_inputBuffer[rewindIndex], Time.fixedDeltaTime);
                            }
                        }

                        m_clientPhysics.Simulate(Time.fixedDeltaTime);
                    }

                    foreach (ClientStateUpdater u in rightUpdaters)
                    {
                        u.EnableSimulation();
                    }

                    m_lastReceivedServerState = null;
                }
            }
        }

        private void ReceiveTCP(TCPToolkit.Packet packet)
        {
            ServerStartsMessage ready = common.serialization.IConvertible.CreateFromBytes<ServerStartsMessage>(packet.Data.ArraySegment());
            if (ready != null)
            {
#if DEBUG_LOG
                Debug.Log("Received server start message.");
#endif // DEBUG_LOG
                m_currentSubState = SubState.SUBSTATE_PLAY;
                return;
            }
            ServerEndsMessage finish = common.serialization.IConvertible.CreateFromBytes<ServerEndsMessage>(packet.Data.ArraySegment());
            if (finish != null)
            {
#if DEBUG_LOG
                Debug.Log("Received server end message.");
#endif // DEBUG_LOG
                m_currentSubState = SubState.SUBSTATE_END;
                return;
            }
        }

        private void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Disconnected from server.");
#endif // DEBUG_LOG
            m_currentSubState = SubState.SUBSTATE_LEAVING;
        }

        protected override void StateUnload()
        {
            m_server.OnTCPReceive += ReceiveTCP;
            m_server.OnUDPReceive += ReceiveUDP;
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_GO;
        }

        protected override void StatePause()
        { }

        protected override void StateResume()
        {
        }
    }
}
