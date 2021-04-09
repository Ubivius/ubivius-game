using UnityEngine;
using System.Collections;
using ubv.http;
using System.Net.Http;
using System.Net;

namespace ubv.microservices
{
    public class AuthenticationService : MonoBehaviour
    {
        [SerializeField] private bool m_mock;
        [SerializeField] private HTTPClient m_HTTPClient;
        [SerializeField] string m_authEndpoint;
        public delegate void OnLogin(int? playerID);

        private OnLogin m_onLoginCallback;

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

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
        public void SendLoginRequest(string user, string pass, OnLogin onLogin)
        {
            if (m_mock)
            {
#if DEBUG_LOG
                Debug.Log("Mocking auth. Auto logging in with random ID.");
#endif // DEBUG_LOG
                onLogin(System.Guid.NewGuid().GetHashCode());
                return;
            }

            m_HTTPClient.SetEndpoint(m_authEndpoint);
            string jsonString = JsonUtility.ToJson(new JSONAuthentificationCredentials
            {
                username = user,
                password = pass,
            }).ToString();
            m_onLoginCallback = onLogin;
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
                int guid = authResponse.id.GetHashCode();
                m_HTTPClient.SetAuthenticationToken(token);

                m_onLoginCallback.Invoke(guid);
                m_onLoginCallback = null;
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Authentication login request was not successful");
#endif // DEBUG_LOG
            }
        }
    }
}
