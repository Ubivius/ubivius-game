using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ubv.http.client;
using UnityEngine;

namespace ubv.microservices
{
    public abstract class Microservice<GetReq, PostReq, PutReq, DelReq> : MonoBehaviour
        where GetReq : GetMicroserviceRequest where PostReq : PostMicroserviceRequest
        where PutReq : PutMicroserviceRequest where DelReq : DeleteMicroserviceRequest
    {
        protected readonly object m_requestLock = new object();

        [SerializeField] protected bool m_mock;
        [SerializeField] protected utils.Mocker m_mockData;

        [SerializeField] protected HTTPClient m_HTTPClient;
        [SerializeField] protected string m_serviceEndpoint;
        
        private void GetRequest(GetReq request)
        {
            m_HTTPClient.SetEndpoint(m_serviceEndpoint);
            m_HTTPClient.Get(request.URL(), (HttpResponseMessage message) => {
                OnGetResponse(message, request);
            });
        }

        private void PutRequest(PutReq request)
        {
            m_HTTPClient.SetEndpoint(m_serviceEndpoint);
            m_HTTPClient.PutJSON(request.URL(), request.JSONString(), (HttpResponseMessage message) => {
                OnPutResponse(message, request);
            } );
        }

        private void DeleteRequest(DelReq request)
        {
            m_HTTPClient.SetEndpoint(m_serviceEndpoint);
            m_HTTPClient.Delete(request.URL(), (HttpResponseMessage message) => {
                OnDeleteResponse(message, request);
            });
        }

        private void PostRequest(PostReq request)
        {
            m_HTTPClient.SetEndpoint(m_serviceEndpoint);
            m_HTTPClient.PostJSON(request.URL(), request.JSONString(), (HttpResponseMessage message) => {
                OnPostResponse(message, request);
            });
        }

        public void Request<Req>(Req request) where Req : MicroserviceRequest
        {
            if (m_mock)
            {
                if (request is GetReq)
                {
                    MockGet(request as GetReq);
                }
                else if (request is PostReq)
                {
                    MockPost(request as PostReq);
                }
                return;
            }

            lock (m_requestLock)
            {
                if (request is GetReq)
                {
                    GetRequest(request as GetReq);
                }
                else if (request is PostReq)
                {
                    PostRequest(request as PostReq);
                }
                else if (request is PutReq)
                {
                    PutRequest(request as PutReq);
                }
                else if (request is DelReq)
                {
                    DeleteRequest(request as DelReq);
                }
            }
        }

        protected virtual void MockPost(PostReq request) { }
        protected virtual void MockGet(GetReq request) { }
        protected virtual void MockPut(PutReq request) { }
        protected virtual void MockDelete(DelReq request) { }

        protected virtual void OnGetResponse(string JSON, GetReq originalRequest) { }
        protected virtual void OnPostResponse(string JSON, PostReq originalRequest) { }
        protected virtual void OnDeleteResponse(string JSON, DelReq originalRequest) { }
        protected virtual void OnPutResponse(string JSON, PutReq originalRequest) { }

        private void OnGetResponse(HttpResponseMessage message, GetReq request)
        {
            if (message.IsSuccessStatusCode)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                OnGetResponse(JSON, request);
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("GET Request was not successful");
#endif // DEBUG_LOG
            }
        }

        private void OnPostResponse(HttpResponseMessage message, PostReq request)
        {
            if (message.IsSuccessStatusCode)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                OnPostResponse(JSON, request);
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("POST Request " + request.URL() + " was not successful: " + message.ReasonPhrase);
#endif // DEBUG_LOG
            }
        }

        private void OnPutResponse(HttpResponseMessage message, PutReq request)
        {
            if (message.IsSuccessStatusCode)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                OnPutResponse(JSON, request);
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("PUT Request was not successful");
#endif // DEBUG_LOG
            }
        }

        private void OnDeleteResponse(HttpResponseMessage message, DelReq request)
        {
            if (message.IsSuccessStatusCode)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                OnDeleteResponse(JSON, request);
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("DELETE Request was not successful");
#endif // DEBUG_LOG
            }
        }
    }
}
