using ubv.microservices;
using UnityEngine;

namespace ubv.client.logic
{
    abstract public class ClientSyncState : MonoBehaviour
    {
        static public UserInfo CurrentUser { get; protected set; }

        static protected ClientConnectionManager m_server;
        static public DispatcherMicroservice DispatcherService;
        static public SocialServicesController SocialServices;
        static public CharacterDataService CharacterService;

        private bool m_isPaused;

        static public void InitDependencies()
        {
            m_server = ClientNetworkingManager.Instance.Server;
            DispatcherService = ClientNetworkingManager.Instance.Dispatcher;
            SocialServices = ClientNetworkingManager.Instance.SocialServices;
            CharacterService = ClientNetworkingManager.Instance.CharacterData;
        }

        protected readonly object m_lock = new object();

        private void Awake()
        {
            m_isPaused = false;
            ClientStateManager.Instance.AddStateToManager(gameObject.scene.name, this);
            StateLoad();
        }

        protected abstract void StateLoad();
        protected abstract void StateUnload();

        protected abstract void StatePause();
        protected abstract void StateResume();
        
        public virtual void StateUpdate() { }

        public virtual void StateFixedUpdate() { }
        
        public virtual void OnStart() { }

        private void OnDestroy()
        {
            StateUnload();
        }

        private void OnEnable()
        {
            ClientStateManager.Instance.SetCurrentState(this);
            if (m_isPaused)
            {
                StateResume();
            }
        }

        public UserInfo GetActiveUser()
        {
            return CurrentUser;
        }

        private void OnDisable()
        {
            if (!m_isPaused)
            {
                m_isPaused = true;
                StatePause();
            }
        }
    }
}
