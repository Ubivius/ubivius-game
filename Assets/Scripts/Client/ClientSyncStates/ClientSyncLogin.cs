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
        [SerializeField] private string m_menuScene;

        private bool m_readyToGoToMenu;
        
        protected override void StateLoad()
        {
            m_readyToGoToMenu = false;
            SocialServices.OnAuthentication += OnLogin;
        }

        protected override void StateUnload()
        {
            SocialServices.OnAuthentication -= OnLogin;
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

            
            SocialServices.Authenticate(user, pass);
        }

        private void GoToMenu()
        {
            ClientStateManager.Instance.PushScene(m_menuScene);
        }

        private void OnLogin(string playerIDString)
        {
            if (playerIDString != null)
            {
                PlayerID = playerIDString.GetHashCode();
                UserInfo = SocialServices.CurrentUser;
                m_readyToGoToMenu = true;
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Login failed. Maybe wrong credentials ?");
#endif // DEBUG_LOG
            }
        }

        protected override void StatePause() { }

        protected override void StateResume() { }
    }   
}
