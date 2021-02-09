using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading;

namespace ubv.client
{
    /// <summary>
    /// Client-side synchronisation with server information
    /// </summary>
    public class ClientSync : MonoBehaviour
    {
        // TO CHECK:: https://www.codeproject.com/Articles/311944/BinaryFormatter-or-Manual-serializing
        // https://github.com/spectre1989/unity_physics_csp/blob/master/Assets/Logic.cs

        client.logic.ClientSyncState m_currentState;
            
<<<<<<< HEAD
        [SerializeField] private udp.client.UDPClient m_udpClient;

        [SerializeField] private InputController m_inputController;
        [SerializeField] private string m_physicsScene;
=======
            [SerializeField] private udp.client.UDPClient m_UDPClient;
            [SerializeField] private tcp.client.TCPClient m_TCPClient;
            
            [SerializeField] private PlayerSettings m_playerSettings;
            [SerializeField] private string m_physicsScene;
>>>>>>> origin/master

#if NETWORK_SIMULATE
        [HideInInspector] public UnityEngine.Events.UnityEvent ConnectButtonEvent;
        [HideInInspector] public UnityEngine.Events.UnityEvent PlayWithoutServerButtonEvent;
#endif // NETWORK_SIMULATE

<<<<<<< HEAD
        private void Awake()
        {

        }

        private void Start()
        {
            m_currentState = new logic.ClientSyncInit(m_udpClient, 
                m_physicsScene, 
                m_inputController
=======
            private void Awake()
            {
                
            }

            private void Start()
            {
                m_currentState = new logic.ClientSyncInit(
                    m_TCPClient,
                    m_UDPClient,
                    m_physicsScene,
                    m_playerSettings
>>>>>>> origin/master
#if NETWORK_SIMULATE
                , this
#endif // NETWORK_SIMULATE
<<<<<<< HEAD
                );
        }

        private void Update()
        {
            m_currentState = m_currentState.Update();
        }

        private void FixedUpdate()
        {
            m_currentState = m_currentState.FixedUpdate();
=======
                    );
            }

            private void Update()
            {
                m_currentState = m_currentState.Update();
            }

            private void FixedUpdate()
            {
                m_currentState = m_currentState.FixedUpdate();
            }

#if NETWORK_SIMULATE
            public void InvokeConnect()
            {
                ConnectButtonEvent.Invoke();
            }

            public void InvokeNoServerConnect()
            {
                PlayWithoutServerButtonEvent.Invoke();
            }
#endif // NETWORK_SIMULATE
>>>>>>> origin/master
        }
    }
}
