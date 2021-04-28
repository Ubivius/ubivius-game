using UnityEngine;
using System.Collections;
using ubv.http;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class UserService : MonoBehaviour
    {
        [SerializeField] private bool m_mock;
        [SerializeField] private string m_forceUserID;
        [SerializeField] private string m_forceUserName;

        [SerializeField] private HTTPClient m_HTTPClient;
        [SerializeField] string m_userEndpoint;
        public delegate void OnGetInfo(UserInfo info);

        private bool m_readyForNextRequest;
        private Queue<UserInfoRequest> m_userRequests;

        private class UserInfoRequest
        {
            public string ID;
            public OnGetInfo Callback;
        }
        
        private struct JSONUserInfoResponse
        {
            public string id;
            public string username;
            public string email;
            public string dateofbirth;
        }

        public class UserInfo
        {
            public readonly string ID;
            public readonly string UserName;
            public readonly string Email;
            public readonly string DateOfBirth;

            public UserInfo(string id, string userName, string email, string dateOfBirth)
            {
                ID = id;
                UserName = userName;
                Email = email;
                DateOfBirth = dateOfBirth;
            }
        }

        private void Awake()
        {
            m_readyForNextRequest = true;
            m_userRequests = new Queue<UserInfoRequest>();
        }

        private void Update()
        {
            if (m_readyForNextRequest)
            {
                if (m_userRequests.Count > 0)
                {
                    UserInfoRequest request = m_userRequests.Peek();
                    Debug.Log("Requesting user info from " + request.ID + " from Update Loop");
                    SendUserInfoRequest(request.ID);
                }
            }
        }

        public void SendUserInfoRequest(string id, OnGetInfo onGetInfo)
        {
            if (m_mock)
            {
#if DEBUG_LOG
                Debug.Log("Mocking user. Auto logging in with random ID (or forced ID provided if any)");
#endif // DEBUG_LOG
                string _id = m_forceUserID.Length > 0 ? m_forceUserID : System.Guid.NewGuid().ToString();
                string _user = m_forceUserName.Length > 0 ? m_forceUserName : "murphy-auto-username";
                onGetInfo(new UserInfo(_id, _user, "murphy@gmail.com", "00-00-0001"));
                return;
            }
#if DEBUG_LOG
            Debug.Log("Requesting user info from " + id);
#endif // DEBUG_LOG

            m_userRequests.Enqueue(new UserInfoRequest() { ID = id, Callback = onGetInfo });

            Debug.Log("User requests count : " + m_userRequests.Count);
            
            if (!m_readyForNextRequest)
            {
                return;
            }

            SendUserInfoRequest(id);
        }

        private void SendUserInfoRequest(string id)
        {
            m_readyForNextRequest = false;
            m_HTTPClient.SetEndpoint(m_userEndpoint);
            m_HTTPClient.Get("users/" + id, OnUserResponse);
        }

        private void OnUserResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                // on s'attend à recevoir un token
                // on add le token au HTTPClient

                string JSON = message.Content.ReadAsStringAsync().Result;
                JSONUserInfoResponse userInfoResponse = JsonUtility.FromJson<JSONUserInfoResponse>(JSON);
                UserInfo userInfo = new UserInfo(userInfoResponse.id, userInfoResponse.username, userInfoResponse.email, userInfoResponse.dateofbirth);
                Debug.Log("Received user info "  + userInfo­.ID + " (on OnUserResponse in service)");
                Debug.Log("Calling callback on " + m_userRequests.Peek().ID);
                m_userRequests.Dequeue().Callback.Invoke(userInfo);
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("User request was not successful");
#endif // DEBUG_LOG
            }
            m_readyForNextRequest = true;
        }
    }
}
