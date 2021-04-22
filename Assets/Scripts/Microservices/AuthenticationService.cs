using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class AuthenticationService : MonoBehaviour
    {
        public delegate void OnLogin(string playerIDString);

        private class LoginRequest
        {
            public string User;
            public string Pass;
            public OnLogin Callback;
        }

        [SerializeField] private string m_forceUserID;
        [SerializeField] private bool m_mock;
        [SerializeField] private HTTPClient m_HTTPClient;
        [SerializeField] string m_authEndpoint;
        private bool m_readyForNextCall;

        private Queue<LoginRequest> m_onLoginCallbacks;

        private void Awake()
        {
            m_onLoginCallbacks = new Queue<LoginRequest>();
            m_readyForNextCall = true;
        }

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

        private void Update()
        {
            if (m_readyForNextCall)
            {
                if(m_onLoginCallbacks.Count > 0)
                {
                    LoginRequest request = m_onLoginCallbacks.Peek();
                    SendLoginRequest(request.User, request.Pass);
                }
            }
        }

        public void SendLoginRequest(string user, string pass, OnLogin onLogin)
        {
            if (m_mock)
            {
#if DEBUG_LOG
                Debug.Log("Mocking auth. Auto logging in with random ID (or forced ID provided if any)");
#endif // DEBUG_LOG
                string id = m_forceUserID.Length > 0 ? m_forceUserID : System.Guid.NewGuid().ToString();
                onLogin(id);
                return;
            }

            m_onLoginCallbacks.Enqueue(new LoginRequest() { User = user, Pass = pass, Callback = onLogin });
            if (!m_readyForNextCall)
            {
                return;
            }

            m_readyForNextCall = false;
            
            SendLoginRequest(user, pass);
        }

        private void SendLoginRequest(string user, string pass)
        {
            string jsonString = JsonUtility.ToJson(new JSONAuthentificationCredentials
            {
                username = user,
                password = pass,
            }).ToString();

            m_HTTPClient.SetEndpoint(m_authEndpoint);
            m_HTTPClient.PostJSON("signin", jsonString, OnAuthenticationResponse);
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
                m_HTTPClient.SetAuthenticationToken(token);

                m_onLoginCallbacks.Dequeue().Callback.Invoke(authResponse.id);
                m_readyForNextCall = true;
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Authentication login request was not successful : " + message.ReasonPhrase );
#endif // DEBUG_LOG
            }
        }
    }
}
