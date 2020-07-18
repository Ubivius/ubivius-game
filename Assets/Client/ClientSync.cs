using UnityEngine;
using UnityEditor;

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

        private void Awake()
        {
            m_localTick = 0;
            m_clientStateBuffer = new ClientState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer = new InputFrame[CLIENT_STATE_BUFFER_SIZE];
        }

        public void AddToInputBuffer(InputFrame inputs)
        {

        }

        private void FixedUpdate()
        {
            ClientState.Step(ref m_clientStateBuffer[m_localTick], m_inputBuffer[m_localTick], Time.fixedDeltaTime);

            // send to server at a particular rate ?
            m_udpClient.Send(m_inputBuffer[m_localTick].ToBytes());

            ++m_localTick;
        }
    }
}