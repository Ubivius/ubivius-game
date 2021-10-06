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
        
        private class UserInfoRequest : Microservice.MicroserviceRequest
        {
            public string ID;

            public override string GetURL()
            {
                return "user/" + ID;
            }
        }

        private class UserInfoCallback : MicroserviceCallback
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
            throw new System.NotImplementedException();
        }
    }
}
