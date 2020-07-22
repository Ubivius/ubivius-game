using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace UBV
{
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

        private Queue<ClientState> m_serverStates;

        private uint m_localTick;

        private const ushort CLIENT_STATE_BUFFER_SIZE = 4096;

        [SerializeField] private string m_physicsScene;
        private PhysicsScene2D m_clientPhysics;

        private void Awake()
        {
            m_localTick = 0;
            m_clientStateBuffer = new ClientState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer = new InputFrame[CLIENT_STATE_BUFFER_SIZE];
            
            m_clientPhysics = SceneManager.GetSceneByName(m_physicsScene).GetPhysicsScene2D();
            m_serverStates = new Queue<ClientState>();
            ClientState.RegisterReceiver(this);
        }

        public void SetCurrentInputBuffer(InputFrame inputs)
        {
            // queue up plusieurs inputs pour un tick?
            // pour pouvoir send plusieurs inputs dans un seul packet? si la vitesse de la connexion baisse
            m_lastInput = inputs;
        }

        private void FixedUpdate()
        {
            uint bufferIndex = m_localTick % CLIENT_STATE_BUFFER_SIZE;
            m_clientStateBuffer[bufferIndex] = m_localTick == 0 ?
                new ClientState() :  m_clientStateBuffer[(m_localTick - 1) % CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer[bufferIndex] = m_lastInput == null ? new InputFrame() : m_lastInput;

            m_clientStateBuffer[bufferIndex].Tick = bufferIndex;
            m_inputBuffer[bufferIndex].Tick = bufferIndex;

            ClientState.Step(ref m_clientStateBuffer[bufferIndex], 
                m_inputBuffer[bufferIndex], 
                Time.fixedDeltaTime,
                ref m_clientPhysics);

            // send to server at a particular rate ?
            m_udpClient.Send(m_inputBuffer[bufferIndex].ToBytes());
            
            // client correction ?
            // receive a state from server
            // check what tick it corresponds to
            // rewind client state to the tick
            // replay up to local tick by stepping every tick

            ClientState playerServerState = null;
            while (m_serverStates.Count > 0)
                playerServerState = m_serverStates.Dequeue();

            if (playerServerState != null)
            {
                uint rewindTicks = playerServerState.Tick;
                uint rewindIndex = rewindTicks++ % CLIENT_STATE_BUFFER_SIZE;
                m_clientStateBuffer[bufferIndex] = playerServerState;

                while (rewindTicks < m_localTick)
                {
                    rewindIndex = rewindTicks++ % CLIENT_STATE_BUFFER_SIZE;
                    ClientState.Step(ref m_clientStateBuffer[rewindIndex],
                        m_inputBuffer[rewindIndex],
                        Time.fixedDeltaTime,
                        ref m_clientPhysics);
                }
            }
            
            ++m_localTick;;
        }
        
        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            ClientState state = ClientState.FromBytes(packet.Data);
            if(state != null)
                m_serverStates.Enqueue(state);
        }
    }
}