using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using static ubv.microservices.UserInfo;
using System;

namespace ubv.microservices
{
    public class UserService : Microservice<GetUserInfoRequest,
        PostMicroserviceRequest, PutUserInfoRequest, DeleteMicroserviceRequest>
    {
        private struct JSONUserInfoResponse
        {
            public string id;
            public string username;
            public string firstname;
            public string lastname;
            public string email;
            public string dateofbirth;
            public string status;
        }
        
        protected override void OnGetResponse(string JSON, GetUserInfoRequest originalRequest)
        {
            JSONUserInfoResponse userInfoResponse = JsonUtility.FromJson<JSONUserInfoResponse>(JSON);
            StatusType statusType = (StatusType)Enum.Parse(typeof(StatusType), userInfoResponse.status, true);

            UserInfo userInfo = new UserInfo(userInfoResponse.id, userInfoResponse.username, userInfoResponse.firstname, userInfoResponse.lastname, userInfoResponse.email, userInfoResponse.dateofbirth, statusType);
            originalRequest.Callback.Invoke(userInfo);
        }

        protected override void OnPutResponse(string JSON, PutUserInfoRequest originalRequest)
        {
            originalRequest.Callback?.Invoke();
        }

        protected override void MockGet(GetUserInfoRequest request)
        {
#if DEBUG_LOG
            Debug.Log("Mocking user. Auto logging in with random ID (or forced ID provided if any)");
#endif // DEBUG_LOG
            string _id = m_mockData.UserID.Length > 0 ? m_mockData.UserID : System.Guid.NewGuid().ToString();
            string _user = m_mockData.UserName.Length > 0 ? m_mockData.UserName : "murphy-auto-username";
            request.Callback.Invoke(new UserInfo(_id, _user, "charles", "murphy", "murphy@gmail.com", "00-00-0001", StatusType.Online));
        }
    }
}
