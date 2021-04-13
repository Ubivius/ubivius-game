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
        [SerializeField] private IPEndPoint m_authEndPoint;

        private bool m_readyToGoToLobby;
        
        protected override void StateAwake()
        {
            ClientSyncState.m_loginState = this;
            ClientSyncState.m_currentState = this;
            m_readyToGoToLobby = false;
        }

        protected override void StateUpdate()
        {
            if (m_readyToGoToLobby)
            {
                m_readyToGoToLobby = false;
                GoToLobby();
            }
        }

        public void SendLoginRequest(string user, string pass)
        {
#if DEBUG_LOG
            Debug.Log("Trying to log in with " + user);
#endif // DEBUG_LOG


            m_authenticationService.SendLoginRequest(user, pass, OnLogin);
        }

        private void GoToLobby()
        {
#if DEBUG_LOG
            Debug.Log("Going to lobby.");
#endif // DEBUG_LOG
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync("ClientLobby");
            // animation petit cercle de load to lobby
        }

        private void OnLogin(int? playerID)
        {
            if (playerID != null)
            {
                PlayerID = playerID;
                m_readyToGoToLobby = true;
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
