using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ubv.client.logic
{
    /// <summary>
    ///  Manages the flow between the client game states
    /// </summary>
    public class ClientStateManager : MonoBehaviour
    {
        public static ClientStateManager Instance { get; private set; } = null;

        [SerializeField]
        private LoadingScreen m_loadingScreen;
        
        [SerializeField]
        private string m_startingScene;

        private Stack<string> m_sceneStack;

        private Dictionary<string, ClientSyncState> m_sceneStates;
        
        private ClientSyncState m_currentState;

        private void Awake()
        {
            if(Instance == null)
                Instance = this;

            m_currentState = null;
            m_sceneStack = new Stack<string>();
            m_sceneStates = new Dictionary<string, ClientSyncState>();
        }

        private void Start()
        {
            m_loadingScreen.gameObject.SetActive(false);
            ClientSyncState.InitDependencies();
            PushScene(m_startingScene);
        }

        private void Update()
        {
            if (m_currentState == null)
                return;

            m_currentState.StateUpdate();
        }

        private void FixedUpdate()
        {
            if (m_currentState == null)
                return;

            m_currentState.StateFixedUpdate();
        }

        public void SetCurrentState(ClientSyncState state)
        {
            if (m_sceneStates.ContainsValue(state))
            {
                m_currentState = state;
            }
#if DEBUG_LOG
            else
            {
                Debug.LogWarning("State not registered. Ignoring.");
            }
#endif // DEBUG_LOG
        }
        

        public void AddStateToManager(string scene, ClientSyncState state)
        {
            if (!m_sceneStates.ContainsValue(state))
            {
                m_sceneStates.Add(scene, state);
            }
#if DEBUG_LOG
            else
            {
                Debug.LogWarning("State already registered. Ignoring.");
            }
#endif // DEBUG_LOG
        }
        
        public void PushScene(string scene)
        {
            if (m_currentState != null)
            {
                m_currentState.gameObject.SetActive(false);
            }
            
            m_sceneStack.Push(scene);
            StartCoroutine(LoadSceneCoroutine(scene));
        }

        public void PopState()
        {
            StartCoroutine(UnloadSceneCoroutine(m_sceneStack.Pop()));
        }
        
        private IEnumerator LoadSceneCoroutine(string sceneToLoad)
        {
            m_loadingScreen.gameObject.SetActive(true);
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            load.allowSceneActivation = true;
            while (!load.isDone)
            {
                m_loadingScreen.SetPercentage(load.progress);
                yield return null;
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
            m_loadingScreen.gameObject.SetActive(false);
            m_sceneStates[sceneToLoad].gameObject.SetActive(true);
        }

        private IEnumerator UnloadSceneCoroutine(string sceneToUnload)
        {
            m_loadingScreen.gameObject.SetActive(true);
            AsyncOperation unload = SceneManager.UnloadSceneAsync(sceneToUnload);
            unload.allowSceneActivation = true;
            while (!unload.isDone)
            {
                m_loadingScreen.SetPercentage(unload.progress);
                yield return null;
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(m_sceneStack.Peek()));
            m_loadingScreen.gameObject.SetActive(false);
            m_sceneStates[m_sceneStack.Peek()].gameObject.SetActive(true);
        }
    }
}
