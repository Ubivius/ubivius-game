using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ubv
{
    public class InputMessage
    {
        public float DeliveryTime;
        public uint StartTick;
        public List<InputFrame> InputFrames;

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[1 + 4 + 4 + 4 + (InputFrames.Count * 14)]; // TODO: un-hardcode (14 bytes is the size of a frame)

            bytes[0] = (byte)Serialization.BYTE_TYPE.INPUT_MESSAGE;

            byte[] timeBytes = System.BitConverter.GetBytes(DeliveryTime);
            byte[] tickBytes = System.BitConverter.GetBytes(StartTick);
            byte[] sizeBytes = System.BitConverter.GetBytes((uint)InputFrames.Count);

            for (ushort i = 0; i < 4; i++)
            {
                bytes[i + 1] = tickBytes[i];
                bytes[i + 1 + 4] = timeBytes[i];
                bytes[i + 1 + 4 + 4] = sizeBytes[i];
            }

            for (ushort i = 0; i < InputFrames.Count; i++)
            {
                byte[] frameBytes = InputFrames[i].ToBytes();
                for (ushort n = 0; n < frameBytes.Length; n++)
                {
                    int index = (i * frameBytes.Length) + n + 1 + 4 + 4 + 4;
                    if(index < UDPToolkit.UDP_PACKET_SIZE)
                        bytes[index] = frameBytes[n];
                }
            }

            return bytes;
        }

        static public InputMessage FromBytes(byte[] bytes)
        {
            InputMessage inputMessage = null;

            if (bytes[0] == (byte)Serialization.BYTE_TYPE.INPUT_MESSAGE)
            {
                inputMessage = new InputMessage();
                inputMessage.StartTick = System.BitConverter.ToUInt32(bytes, 1);
                inputMessage.DeliveryTime = System.BitConverter.ToSingle(bytes, 1 + 4);

                inputMessage.InputFrames = new List<InputFrame>();
                uint frameCount = System.BitConverter.ToUInt32(bytes, 1 + 4 + 4);
                for (int i = 0; i < frameCount; i++)
                {
                    int startIndex = 4 + 4 + 4 + 1 + (14 * i);
                    if (startIndex + 14 < UDPToolkit.UDP_PACKET_SIZE)
                    {
                        InputFrame frame = InputFrame.FromBytes(bytes.SubArray(startIndex, 14));
                        if (frame != null)
                            inputMessage.InputFrames.Add(frame);
                    }
                }
            }

            return inputMessage;
        }
    }

    /// <summary>
    /// Client-side synchronisation with server information
    /// </summary>
    public class ClientSync : MonoBehaviour, IPacketReceiver
    {
        

        // TO CHECK:: https://www.codeproject.com/Articles/311944/BinaryFormatter-or-Manual-serializing
        // https://github.com/spectre1989/unity_physics_csp/blob/master/Assets/Logic.cs

        [SerializeField]
        private UDPClient m_udpClient;
       
        // has an input buffer to recreate inputs after server correction
        private ClientState[] m_clientStateBuffer;
        private InputFrame[] m_inputBuffer;
        private InputFrame m_lastInput;

        private ClientState m_lastServerState;

        private uint m_remoteTick;
        private uint m_localTick;
        private uint m_previousLocalTick;

        private const ushort CLIENT_STATE_BUFFER_SIZE = 256;

        [SerializeField] private string m_physicsScene;
        private PhysicsScene2D m_clientPhysics;

        private void Awake()
        {
            m_localTick = 0;
            m_previousLocalTick = 0;
            m_clientStateBuffer =   new ClientState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer =         new InputFrame[CLIENT_STATE_BUFFER_SIZE];
            
            m_clientPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_lastServerState = null;
            ClientState.RegisterReceiver(this);
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

            m_inputBuffer[bufferIndex] =            new InputFrame();
            if ( m_lastInput != null )
            {
                m_inputBuffer[bufferIndex].Movement = m_lastInput.Movement;
                m_inputBuffer[bufferIndex].Sprinting = m_lastInput.Sprinting;
            }

            m_inputBuffer[bufferIndex].Tick =       m_localTick;

            m_lastInput = null;

            InputMessage inputMessage = new InputMessage();
            inputMessage.StartTick =    m_remoteTick;
            inputMessage.InputFrames =  new List<InputFrame>();

            for(uint tick = inputMessage.StartTick; tick <= m_localTick; tick++)
            {
                inputMessage.InputFrames.Add(m_inputBuffer[tick % CLIENT_STATE_BUFFER_SIZE]);
            }

#if NETWORK_SIMULATE
            if (Random.Range(0f, 1f) > 0.15f) // 15% de packet loss
            {
                m_udpClient.Send(inputMessage.ToBytes());
            }
            else
                Debug.Log("SIMULATING PACKET LOSS");
#else
            m_udpClient.Send(inputMessage.ToBytes());
#endif

            UpdateClientState(bufferIndex);
            
            ClientCorrection(bufferIndex);

            m_previousLocalTick = m_localTick++;
        }

        private void UpdateClientState(uint bufferIndex)
        {
            // set current client state to last one before updating it
            if (m_localTick == 0 && m_previousLocalTick == 0)
                m_clientStateBuffer[bufferIndex] = new ClientState();
            else
                m_clientStateBuffer[bufferIndex] = m_clientStateBuffer[m_previousLocalTick % CLIENT_STATE_BUFFER_SIZE];
            
            m_clientStateBuffer[bufferIndex].Tick = m_localTick;

            m_clientStateBuffer[bufferIndex].Step(
                m_inputBuffer[bufferIndex],
                Time.fixedDeltaTime,
                ref m_clientPhysics);
        }

        private void ClientCorrection(uint bufferIndex)
        {
            // client correction 
            // receive a state from server
            // check what tick it corresponds to
            // rewind client state to the tick
            // replay up to local tick by stepping every tick

            if (m_lastServerState != null)
            {

                // check if correction/rewind is needed (if local and remote state are too different)
                if (ClientState.NeedsCorrection(m_clientStateBuffer[bufferIndex], m_lastServerState))
                {
                    uint rewindTicks = m_remoteTick;
#if DEBUG
                    Debug.Log("Client: rewinding " + (m_localTick - rewindTicks) + " ticks");
#endif // DEBUG
                    
                    while (rewindTicks < m_localTick)
                    {
                        uint rewindIndex = rewindTicks++ % CLIENT_STATE_BUFFER_SIZE;

                        // reset world state to last server-sent state
                        ClientState.UpdateFromState(m_lastServerState);
                        // save world state to current state
                        m_clientStateBuffer[rewindIndex].SaveClientState();

                        m_clientStateBuffer[rewindIndex].Step(
                            m_inputBuffer[rewindIndex],
                            Time.fixedDeltaTime,
                            ref m_clientPhysics);
                    }
                }

                // hard reset to server position if error is too big 
                m_lastServerState = null;
            }
        }
        
        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            ClientState state = ClientState.FromBytes(packet.Data);
            if (state != null)
                m_lastServerState = state;
#if DEBUG_LOG
            Debug.Log("Received server state tick " + state.Tick);
#endif //DEBUG_LOG
            m_remoteTick = state.Tick;
        }
    }
}