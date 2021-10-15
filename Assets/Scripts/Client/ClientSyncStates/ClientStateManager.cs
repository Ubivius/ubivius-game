using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ubv.client.logic
{
    /// <summary>
    ///  Manages the flow between the client game states
    /// </summary>
    public class ClientStateManager : MonoBehaviour
    {
        public static ClientStateManager Instance { get; private set; } = null;

        [SerializeField]
        private ClientSyncState m_startingState;

        private Stack<ClientSyncState> m_stateStack = new Stack<ClientSyncState>();

        private void Awake()
        {
            if(Instance == null)
                Instance = this;

            ClientSyncState.InitDependencies();
            PushState(m_startingState);
        }
        
        private void Update()
        {
            if (m_stateStack.Count == 0)
                return;

            m_stateStack.Peek().StateUpdate();
        }

        private void FixedUpdate()
        {
            if (m_stateStack.Count == 0)
                return;

            m_stateStack.Peek().StateFixedUpdate();
        }
        public void PushState(ClientSyncState state)
        {
            m_stateStack.Push(state);
            m_stateStack.Peek().StateLoad();
        }

        public void PopState()
        {
            if(m_stateStack.Count > 0)
                m_stateStack.Pop().StateUnload();
        }
    }
}
