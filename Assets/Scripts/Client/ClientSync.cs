using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace UBV
{
    public class ClientSync : MonoBehaviour
    {
        // TO CHECK:: https://www.codeproject.com/Articles/311944/BinaryFormatter-or-Manual-serializing
        // https://github.com/spectre1989/unity_physics_csp/blob/master/Assets/Logic.cs

        [SerializeField]
        private UDPClient m_udpClient;

        // has an input buffer to recreate inputs after server correction
        private ClientState[] m_clientStateBuffer;
        private InputFrame[] m_inputBuffer;

        private uint m_localTick;

        private const ushort CLIENT_STATE_BUFFER_SIZE = 256;
        
        private PhysicsScene2D m_clientPhysics;

        private void Awake()
        {
            m_localTick = 0;
            m_clientStateBuffer = new ClientState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer = new InputFrame[CLIENT_STATE_BUFFER_SIZE];

            m_clientPhysics = SceneManager.GetActiveScene().GetPhysicsScene2D();
        }

        public void SetCurrentInputBuffer(InputFrame inputs)
        {
            // queue up plusieurs inputs pour un tick?
            // pour pouvoir send plusieurs inputs dans un seul packet? si la vitesse de la connexion baisse
            m_inputBuffer[m_localTick % CLIENT_STATE_BUFFER_SIZE] = inputs;
        }

        private void FixedUpdate()
        {
            uint bufferIndex = m_localTick % CLIENT_STATE_BUFFER_SIZE;
            m_clientStateBuffer[bufferIndex] = m_localTick == 0 ?
                new ClientState() : 
                m_clientStateBuffer[(m_localTick - 1) % CLIENT_STATE_BUFFER_SIZE];

            m_clientStateBuffer[bufferIndex].Tick = bufferIndex;

            ClientState.Step(ref m_clientStateBuffer[bufferIndex], 
                m_inputBuffer[bufferIndex], 
                Time.fixedDeltaTime);

            m_clientPhysics.Simulate(Time.fixedDeltaTime);

            // client correction ?
            // receive a state from server
            // check what tick it corresponds to
            // rewind client state to the tick
            // replay up to local tick by stepping every tick

            // send to server at a particular rate ?
            m_udpClient.Send(m_inputBuffer[bufferIndex].ToBytes());

            ++m_localTick;;
        }
    }
}