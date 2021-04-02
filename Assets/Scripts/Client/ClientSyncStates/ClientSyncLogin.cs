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
        
        private struct JSONAuthentificationCredentials
        {
            public string username;
            public string password;
        }

        private struct JSONAuthenticationResponse
        {
            public string accessToken;
            public string id;
        }

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

            
            string request = "authenticator/" + user;
            string jsonString = JsonUtility.ToJson(new JSONAuthentificationCredentials
            {
                username = user,
                password = pass,
            }).ToString();
            m_HTTPClient.PostJSON("signin", jsonString, OnAuthenticationResponse);
        }

        private void GoToLobby()
        {
            Debug.Log("Going to lobby.");
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync("ClientLobby");
            // animation petit cercle de load to lobby
        }

        private void OnAuthenticationResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                // on s'attend à recevoir un token
                // on add le token au HTTPClient

                string JSON = message.Content.ReadAsStringAsync().Result;
                JSONAuthenticationResponse authResponse = JsonUtility.FromJson<JSONAuthenticationResponse>(JSON);
                string token = authResponse.accessToken;
                int guid = authResponse.id.GetHashCode();
                m_playerID = guid;
                m_HTTPClient.SetAuthenticationToken(token);
                m_readyToGoToLobby = true;
            }
            else
            {
                Debug.Log("Dispatcher GET request was not successful");
            }
        }
    }   
}
