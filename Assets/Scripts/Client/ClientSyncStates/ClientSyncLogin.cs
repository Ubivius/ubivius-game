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
        [SerializeField] IPEndPoint m_authEndPoint;
        
        private struct JSONAuthentificationCredentials
        {
            public string username;
            public string password;
        }

        private struct JSONAuthenticationResponse
        {
            public string   token;
            public int      guid;
        }

        protected override void StateAwake()
        {
            ClientSyncState.m_loginState = this;
            ClientSyncState.m_currentState = this;
        }
        
        public void SendLoginRequest(string user, string password)
        {
#if DEBUG_LOG
            Debug.Log("Trying to log in with " + user);
#endif // DEBUG_LOG

            /*
            string request = "authenticator";
            m_HTTPClient.Get(request, OnDispatcherResponse);
            */

            // mock auth response for now
            HttpResponseMessage msg = new HttpResponseMessage();
            string jsonString = JsonUtility.ToJson(new JSONAuthenticationResponse
            {
                token = "",
                guid = System.Guid.NewGuid().GetHashCode(),
            }).ToString();
            msg.Content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            msg.StatusCode = HttpStatusCode.OK;
            OnAuthenticationResponse(msg);
        }

        private void GoToLobby()
        {
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
                string token = authResponse.token;
                int guid = authResponse.guid;
                m_playerID = guid;
                m_HTTPClient.SetAuthenticationToken(token);
                GoToLobby();
            }
            else
            {
                Debug.Log("Dispatcher GET request was not successful");
            }
        }
    }   
}
