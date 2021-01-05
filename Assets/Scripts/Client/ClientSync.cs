using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading;

namespace ubv
{
    public class InputMessage : Serializable
    {
        // public float DeliveryTime;
        public SerializableTypes.Uint32 StartTick;
        public SerializableTypes.List<InputFrame> InputFrames;
        
        protected override void InitSerializableMembers()
        {
            StartTick = new SerializableTypes.Uint32(this, 0);
            InputFrames = new SerializableTypes.List<InputFrame>(this, new List<InputFrame>());
        }

        protected override byte SerializationID()
        {
            return (byte)Serialization.BYTE_TYPE.INPUT_MESSAGE;
        }
    }

    /// <summary>
    /// Client-side synchronisation with server information
    /// </summary>
    public class ClientSync : MonoBehaviour, IPacketReceiver
    {
        // TO CHECK:: https://www.codeproject.com/Articles/311944/BinaryFormatter-or-Manual-serializing
        // https://github.com/spectre1989/unity_physics_csp/blob/master/Assets/Logic.cs


#if NETWORK_SIMULATE
        public float PacketLossChance = 0.15f;
#endif // NETWORK_SIMULATE


#if PROTOTYPING
        public bool AlwaysCorrectClient = true;
#endif // PROTOTYPING

        static private Mutex m_threadLocker = new Mutex();

        [SerializeField]
        private UDPClient m_udpClient;
       
        // has an input buffer to recreate inputs after server correction
        private ClientState[] m_clientStateBuffer;
        private InputFrame[] m_inputBuffer;
        private InputFrame m_lastInput;

        private ClientState m_lastServerState;

        private uint m_remoteTick;
        private uint m_localTick;

        private const ushort CLIENT_STATE_BUFFER_SIZE = 256;

        [SerializeField] private string m_physicsScene;
        private PhysicsScene2D m_clientPhysics;

        private void Awake()
        {
            m_localTick = 0;
            m_clientStateBuffer =   new ClientState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer =         new InputFrame[CLIENT_STATE_BUFFER_SIZE];
            
            m_clientPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_lastServerState = null;
            ClientState.RegisterReceiver(this);

            for(ushort i = 0; i < CLIENT_STATE_BUFFER_SIZE; i++)
            {
                m_clientStateBuffer[i] = new ClientState();
                m_inputBuffer[i] = new InputFrame();
            }
        }

        public void AddInput(InputFrame input)
        {
            // queue up plusieurs inputs pour un tick?
            // pour pouvoir send plusieurs inputs dans un seul packet? si la vitesse de la connexion baisse
            m_lastInput = input;
        }

        private void FixedUpdate()
        {
            uint bufferIndex = m_localTick % CLIENT_STATE_BUFFER_SIZE;

            UpdateInput(bufferIndex);

            UpdateClientState(bufferIndex);

            ++m_localTick;

            ClientCorrection();
            
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
            List<InputFrame> frames = new List<InputFrame>();
            for (uint tick = m_remoteTick; tick <= m_localTick; tick++)
            {
                frames.Add(m_inputBuffer[tick % CLIENT_STATE_BUFFER_SIZE]);
            }

            InputMessage inputMessage = new InputMessage();
            inputMessage.StartTick.Set(m_remoteTick);
            inputMessage.InputFrames.Set(frames);

#if NETWORK_SIMULATE
            if (Random.Range(0f, 1f) > PacketLossChance)
            {
                m_udpClient.Send(inputMessage.GetBytes());
            }
            else
                Debug.Log("SIMULATING PACKET LOSS");
#else
            m_udpClient.Send(inputMessage.ToBytes());
#endif
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
            
            if (m_lastServerState != null)
            {
                // check if correction/rewind is needed (if local and remote state are too different)
                if (
#if PROTOTYPING
                    AlwaysCorrectClient || 
#endif// PROTOTYPING
                    ClientState.NeedsCorrection(m_clientStateBuffer[m_remoteTick % CLIENT_STATE_BUFFER_SIZE], m_lastServerState))
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
                    // hard reset to server position if error is too big 
                }
                
                m_lastServerState = null;
            }
        }
        
        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            // TODO remove tick from ClientSTate and add it to custom server state packet?
            // client doesnt need its own client state ticks
            ClientState state = ClientState.FromBytes(packet.Data);
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