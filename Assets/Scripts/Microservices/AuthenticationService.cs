using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class AuthenticationService : Microservice<GetMicroserviceRequest, 
        PostAuthenticationRequest, PutMicroserviceRequest, DeleteMicroserviceRequest>
    {
        private struct JSONAuthenticationResponse
        {
            public string accessToken;
            public string id;
        }
        
        protected override void MockPost(PostAuthenticationRequest request)
        {
#if DEBUG_LOG
            Debug.Log("Mocking auth. Auto logging in with random ID (or forced ID provided if any)");
#endif // DEBUG_LOG
            string id = m_mockData.UserID.Length > 0 ? m_mockData.UserID : System.Guid.NewGuid().ToString();
            request.Success.Invoke(id);
        }

        protected override void OnPostResponse(string JSON, PostAuthenticationRequest originalRequest)
        {
            JSONAuthenticationResponse authResponse = JsonUtility.FromJson<JSONAuthenticationResponse>(JSON);
            string token = authResponse.accessToken;
            m_HTTPClient.SetAuthenticationToken(token);

            originalRequest.Success.Invoke(authResponse.id);
        }
    }
}
