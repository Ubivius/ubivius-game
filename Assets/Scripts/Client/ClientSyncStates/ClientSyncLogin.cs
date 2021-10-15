using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using System.Net.Http;

namespace ubv.client.logic
{
    /// <summary>
    /// Reprensents the client state before he is logged in
    /// </summary>
    public class ClientSyncLogin : ClientSyncState
    {
        [SerializeField] private string m_clientMenuScene;

        private bool m_readyToGoToMenu;
        
        public override void StateLoad()
        {
            m_readyToGoToMenu = false;
            m_socialServices.OnAuthentication += OnLogin;
        }

        public override void StateUnload()
        {
            m_socialServices.OnAuthentication -= OnLogin;
        }

        public override void StateUpdate()
        {
            if (m_readyToGoToMenu)
            {
                m_readyToGoToMenu = false;
                GoToMenu();
            }
        }

        public void SendLoginRequest(string user, string pass)
        {
#if DEBUG_LOG
            Debug.Log("Trying to log in with " + user);
#endif // DEBUG_LOG

            
            m_socialServices.Authenticate(user, pass);
        }

        private void GoToMenu()
        {
#if DEBUG_LOG
            Debug.Log("Going to menu.");
#endif // DEBUG_LOG
            
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync(m_clientMenuScene);
            m_stateManager.PopState();
            // animation petit cercle de load to lobby
        }

        private void OnLogin(string playerIDString)
        {
            if (playerIDString != null)
            {
                PlayerID = playerIDString.GetHashCode();
                UserInfo = m_socialServices.CurrentUser;
                m_readyToGoToMenu = true;
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Login failed. Maybe wrong credentials ?");
#endif // DEBUG_LOG
            }
        }
    }   
}
