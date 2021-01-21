using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading;

namespace ubv
{
    namespace client
    {
        /// <summary>
        /// Client-side synchronisation with server information
        /// </summary>
        public class ClientSync : MonoBehaviour, udp.client.IPacketReceiver
        {
            // TO CHECK:: https://www.codeproject.com/Articles/311944/BinaryFormatter-or-Manual-serializing
            // https://github.com/spectre1989/unity_physics_csp/blob/master/Assets/Logic.cs


#if NETWORK_SIMULATE
            public float PacketLossChance = 0.15f;
#endif // NETWORK_SIMULATE
            
            static private Mutex m_threadLocker = new Mutex();

            [SerializeField]
            private udp.client.UDPClient m_udpClient;

            // has an input buffer to recreate inputs after server correction
            private ClientState[] m_clientStateBuffer;
            private common.data.InputFrame[] m_inputBuffer;
            private common.data.InputFrame m_lastInput;

            private ClientState m_lastServerState;

            private uint m_remoteTick;
            private uint m_localTick;
            private bool m_isServerBind;

            private const ushort CLIENT_STATE_BUFFER_SIZE = 256;

            [SerializeField] private string m_physicsScene;
            private PhysicsScene2D m_clientPhysics;
            
            public bool IsServerBind { get => m_isServerBind; set => m_isServerBind = value; }

            private void Awake()
            {
                m_localTick = 0;
                m_clientStateBuffer = new ClientState[CLIENT_STATE_BUFFER_SIZE];
                m_inputBuffer = new common.data.InputFrame[CLIENT_STATE_BUFFER_SIZE];

                m_clientPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
                m_lastServerState = null;
                udp.client.UDPClient.RegisterReceiver(this);

                for (ushort i = 0; i < CLIENT_STATE_BUFFER_SIZE; i++)
                {
                    m_clientStateBuffer[i] = new ClientState();
                    m_inputBuffer[i] = new common.data.InputFrame();
                }
            }

            public void AddInput(common.data.InputFrame input)
            {
                m_lastInput = input;
            }

            private void FixedUpdate()
            {
                
                uint bufferIndex = m_localTick % CLIENT_STATE_BUFFER_SIZE;

                UpdateInput(bufferIndex);

                UpdateClientState(bufferIndex);

                ++m_localTick;
                
                if (IsServerBind)
                {
                    ClientCorrection();
                }                
            }

            private void UpdateInput(uint bufferIndex)
            {
                if (m_lastInput != null)
                {
                    m_inputBuffer[bufferIndex].Movement.Set(m_lastInput.Movement.Value);
                    m_inputBuffer[bufferIndex].Sprinting.Set(m_lastInput.Sprinting.Value);
                }
                else
                {
                    m_inputBuffer[bufferIndex].SetToNeutral();
                }

                m_inputBuffer[bufferIndex].Tick.Set(m_localTick);

                m_lastInput = null;

                // TODO: Cap max input queue size
                // (under the hood, send multiple packets?)
                List<common.data.InputFrame> frames = new List<common.data.InputFrame>();
                for (uint tick = m_remoteTick; tick <= m_localTick; tick++)
                {
                    frames.Add(m_inputBuffer[tick % CLIENT_STATE_BUFFER_SIZE]);
                }

                common.data.InputMessage inputMessage = new common.data.InputMessage();
                inputMessage.StartTick.Set(m_remoteTick);
                inputMessage.InputFrames.Set(frames);

                if (IsServerBind)
                {
#if NETWORK_SIMULATE
                    if (Random.Range(0f, 1f) > PacketLossChance)
                    {
                        m_udpClient.Send(inputMessage.GetBytes());
                    }
                    else
                    {
                        Debug.Log("SIMULATING PACKET LOSS");
                    }
#else
                        m_udpClient.Send(inputMessage.ToBytes());
#endif //NETWORK_SIMULATE       
                }
            }

            private void UpdateClientState(uint bufferIndex)
            {
                // set current client state to last one then updating it
                m_clientStateBuffer[bufferIndex].StoreCurrentStateAndStep(
                    m_inputBuffer[bufferIndex],
                    Time.fixedDeltaTime,
                    ref m_clientPhysics);
            }

            private void ClientCorrection()
            {
                // receive a state from server
                // check what tick it corresponds to
                // rewind client state to the tick
                // replay up to local tick by stepping every tick

                // we reset every updater for now
                // TODO maybe later: reset only those who need correcting?

                if (m_lastServerState != null)
                {
                    if (ClientState.StatesNeedingCorrection(m_lastServerState).Count > 0)
                    {
                        uint rewindTicks = m_remoteTick;
#if DEBUG_LOG
                        Debug.Log("Client: rewinding " + (m_localTick - rewindTicks) + " ticks");
#endif // DEBUG_LOG
                        
                        // reset world state to last server-sent state
                        ClientState.SetToState(m_lastServerState);

                        while (rewindTicks < m_localTick)
                        {
                            uint rewindIndex = rewindTicks++ % CLIENT_STATE_BUFFER_SIZE;

                            m_clientStateBuffer[rewindIndex].StoreCurrentStateAndStep(
                                m_inputBuffer[rewindIndex],
                                Time.fixedDeltaTime,
                                ref m_clientPhysics);
                        }

                        // hard reset to server state if error is too big  ?
                    }

                    m_lastServerState = null;
                }
            }

            public void ReceivePacket(udp.UDPToolkit.Packet packet)
            {
                // TODO remove tick from ClientSTate and add it to custom server state packet?
                // client doesnt need its own client state ticks
                ClientState state = ClientState.FromBytes<ClientState>(packet.Data);
                if (state != null)
                {
                    m_lastServerState = state;
#if DEBUG_LOG
                    Debug.Log("Received server state tick " + state.Tick);
#endif //DEBUG_LOG
                    m_remoteTick = state.Tick;
                }
            }
        }
    }
}
