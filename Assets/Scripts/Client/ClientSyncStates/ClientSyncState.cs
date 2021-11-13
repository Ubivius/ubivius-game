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

        private PlayerControls m_controls;

        private bool m_isPaused;
        protected bool m_canBack = false;

        static public void InitDependencies()
        {
            m_server = ClientNetworkingManager.Instance.Server;
            DispatcherService = ClientNetworkingManager.Instance.Dispatcher;
            SocialServices = ClientNetworkingManager.Instance.SocialServices;
            CharacterService = ClientNetworkingManager.Instance.CharacterData;
        }

        protected readonly object m_lock = new object();

        protected virtual void Awake()
        {
            m_isPaused = false;
            m_controls = new PlayerControls();
            m_controls.Menu.Back.canceled += context => Back();
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
            m_controls.Menu.Enable();
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
            m_controls.Menu.Disable();
            if (!m_isPaused)
            {
                m_isPaused = true;
                StatePause();
            }
        }

        public void Back()
        {
            if (m_canBack) 
            { 
                ClientStateManager.Instance.PopState();
            }
        }

        public void SetCanBack(bool canBack)
        {
            m_canBack = canBack;
        }
    }
}
