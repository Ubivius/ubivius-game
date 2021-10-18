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
        private string m_startingState;

        private Stack<string> m_stateStack;

        [HideInInspector]
        public ClientSyncState CurrentState = null;

        public io.ClientFileSaveManager FileSaveManager;

        private void Awake()
        {
            if(Instance == null)
                Instance = this;

            m_stateStack = new Stack<string>();
            
        }

        private void Start()
        {
            m_loadingScreen.gameObject.SetActive(false);
            ClientSyncState.InitDependencies();
            PushState(m_startingState);
        }

        private void Update()
        {
            if (CurrentState == null)
                return;

            CurrentState.StateUpdate();
        }

        private void FixedUpdate()
        {
            if (CurrentState == null)
                return;

            CurrentState.StateFixedUpdate();
        }
        
        public void PushState(string state)
        {
            if (CurrentState != null)
            {
                CurrentState.gameObject.SetActive(false);
            }

            m_stateStack.Push(state);
            StartCoroutine(LoadSceneCoroutine(state));
        }

        public void PopState()
        {
            StartCoroutine(UnloadSceneCoroutine(m_stateStack.Pop()));
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
            m_loadingScreen.gameObject.SetActive(false);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
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
            m_loadingScreen.gameObject.SetActive(false);
        }
    }
}
