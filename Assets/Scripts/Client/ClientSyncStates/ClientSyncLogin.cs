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
    public class ClientSyncLogin : ClientSyncState
    {
        [SerializeField] private string m_clientMenuScene;
        [SerializeField] private IPEndPoint m_authEndPoint;

        private bool m_readyToGoToMenu;
        
        protected override void StateAwake()
        {
            ClientSyncState.m_loginState = this;
            ClientSyncState.m_currentState = this;
            m_readyToGoToMenu = false;
        }

        protected override void StateUpdate()
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


            m_authenticationService.SendLoginRequest(user, pass, OnLogin);
        }

        private void GoToMenu()
        {
#if DEBUG_LOG
            Debug.Log("Going to menu.");
#endif // DEBUG_LOG
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync(m_clientMenuScene);
            // animation petit cercle de load to lobby
        }

        private void OnLogin(string playerIDString)
        {
            if (playerIDString != null)
            {
                PlayerID = playerIDString.GetHashCode();
                m_userService.Request(new microservices.UserService.UserInfoRequest(playerIDString, OnGetUserInfo));
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Login failed. Maybe wrong credentials ?");
#endif // DEBUG_LOG
            }
        }

        private void OnGetUserInfo(microservices.UserService.UserInfo info)
        {
            UserInfo = info;
            m_readyToGoToMenu = true;
        }
    }   
}
