using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ubv
{
    /// <summary>
    /// Client-side synchronisation with server information
    /// </summary>
    public class ClientSync : MonoBehaviour, IPacketReceiver
    {
        // TO CHECK:: https://www.codeproject.com/Articles/311944/BinaryFormatter-or-Manual-serializing
        // https://github.com/spectre1989/unity_physics_csp/blob/master/Assets/Logic.cs

        [SerializeField]
        private UDPClient m_udpClient;

        [SerializeField]
        private int m_sendToServerRate = 5;

        // has an input buffer to recreate inputs after server correction
        private ClientState[] m_clientStateBuffer;
        private InputFrame[] m_inputBuffer;
        private InputFrame m_lastInput;

        private ClientState m_lastServerState;

        private uint m_localTick;
        private uint m_previousLocalTick;

        private const ushort CLIENT_STATE_BUFFER_SIZE = 256;

        [SerializeField] private string m_physicsScene;
        private PhysicsScene2D m_clientPhysics;

        private void Awake()
        {
            m_localTick = 0;
            m_previousLocalTick = 0;
            m_clientStateBuffer = new ClientState[CLIENT_STATE_BUFFER_SIZE];
            m_inputBuffer = new InputFrame[CLIENT_STATE_BUFFER_SIZE];
            
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
            m_inputBuffer[bufferIndex] = m_lastInput ?? new InputFrame();
            m_lastInput = null;
            // send to server at a particular rate ?
            //if ( m_localTick % m_sendToServerRate == 0 )
            {
                m_udpClient.Send(m_inputBuffer[bufferIndex].ToBytes());
            }

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
            m_inputBuffer[bufferIndex].Tick = m_localTick;

            ClientState.Step(ref m_clientStateBuffer[bufferIndex],
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
                uint rewindTicks = m_lastServerState.Tick;
                uint rewindIndex = 0;
                //m_clientStateBuffer[bufferIndex] = playerServerState;

                // check if correction/rewind is needed (if local and remote state are too different)
                if (ClientState.NeedsCorrection(ref m_clientStateBuffer[bufferIndex], m_lastServerState))
                {
                    while (rewindTicks < m_localTick)
                    {
                        rewindIndex = rewindTicks++ % CLIENT_STATE_BUFFER_SIZE;
                        ClientState.Step(ref m_clientStateBuffer[rewindIndex],
                            m_inputBuffer[rewindIndex],
                            Time.fixedDeltaTime,
                            ref m_clientPhysics);
                    }
                }

                // hard reset to server position if error is too big 
            }
        }
        
        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            ClientState state = ClientState.FromBytes(packet.Data);
            if (state != null)
                m_lastServerState = state;
        }
    }
}