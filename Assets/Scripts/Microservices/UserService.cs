using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class UserService : Microservice
    {
        public delegate void OnGetInfo(UserInfo info);
        
        public class UserInfoRequest : Microservice.MicroserviceRequest
        {
            public string ID;

            public UserInfoRequest(string ID, OnGetInfo callback)
            {
                this.ID = ID;
                Callback = new UserInfoCallback() { OnGetInfo = callback };
            }

            public override string GetURL()
            {
                return "user/" + ID;
            }
        }

        public class UserInfoCallback : MicroserviceCallback
        {
            public OnGetInfo OnGetInfo;
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
        
        protected override void OnResponse(string JSON, MicroserviceRequest originalRequest)
        {
            JSONUserInfoResponse userInfoResponse = JsonUtility.FromJson<JSONUserInfoResponse>(JSON);
            UserInfo userInfo = new UserInfo(userInfoResponse.id, userInfoResponse.username, userInfoResponse.email, userInfoResponse.dateofbirth);
            ((originalRequest as UserInfoRequest)?.Callback as UserInfoCallback)?.OnGetInfo.Invoke(userInfo);
        }

        protected override void Mock(MicroserviceCallback callback)
        {
#if DEBUG_LOG
            Debug.Log("Mocking user. Auto logging in with random ID (or forced ID provided if any)");
#endif // DEBUG_LOG
            string _id = m_mockData.UserID.Length > 0 ? m_mockData.UserID : System.Guid.NewGuid().ToString();
            string _user = m_mockData.UserName.Length > 0 ? m_mockData.UserName : "murphy-auto-username";
            (callback as UserInfoCallback)?.OnGetInfo.Invoke(new UserInfo(_id, _user, "murphy@gmail.com", "00-00-0001"));
        }
    }
}
