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
        
        private int? m_playerID;

        protected override void StateAwake()
        {
            ClientSyncState.m_loginState = this;
            ClientSyncState.m_currentState = this;
        }
        
        public void SendLoginRequest(string user, string password)
        {
            Debug.Log("Trying to log in with " + user);
            // mock dispatcher response for now
            /*HttpResponseMessage msg = new HttpResponseMessage();
            string jsonString = JsonUtility.ToJson(new JSONServerInfo
            {
                TCPAddress = m_serverTCPAddress,
                TCPPort = m_serverTCPPort,
                UDPAddress = m_serverUDPAddress,
                UDPPort = m_serverUDPPort
            }).ToString();
            msg.Content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            msg.StatusCode = HttpStatusCode.OK;
            OnDispatcherResponse(msg)*/

            // uncomment when dispatcher ready
            // m_HTTPClient.Get("dispatcher/" + playerID.ToString(), OnDispatcherResponse);

            string request = "authenticator";
            m_HTTPClient.Get(request, OnDispatcherResponse);
        }

        private void GoToLobby()
        {
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync("ClientGame");
            // animation petit cercle de load to lobby
        }

        private void OnDispatcherResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                // on s'attend à recevoir un token
                // on add le token au HTTPClient

                string token = message.Content.ReadAsStringAsync().Result;
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
