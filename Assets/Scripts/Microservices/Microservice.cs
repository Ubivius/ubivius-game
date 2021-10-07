using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

namespace ubv.microservices
{
    public abstract class Microservice<GetReq, PostReq> : MonoBehaviour
        where GetReq : GetMicroserviceRequest where PostReq : PostMicroserviceRequest
    {
        public const int REQUEST_CHECK_TIME = 3;
        protected readonly object m_requestLock = new object();

        [SerializeField] protected bool m_mock;
        [SerializeField] protected utils.Mocker m_mockData;

        [SerializeField] protected HTTPClient m_HTTPClient;
        [SerializeField] protected string m_serviceEndpoint;

        private bool m_readyForNextRequest;
        private Queue<GetReq> m_getRequests;
        private Queue<PostReq> m_postRequests;

        private void Awake()
        {
            m_readyForNextRequest = true;
            m_getRequests = new Queue<GetReq>();
            m_postRequests = new Queue<PostReq>();
        }

        private void Update()
        {
            if (Time.frameCount % REQUEST_CHECK_TIME == 0)
            {
                lock (m_requestLock)
                {
                    if (m_readyForNextRequest)
                    {
                        if (m_getRequests.Count > 0)
                        {
                            MicroserviceRequest request = m_getRequests.Peek();
                            Request(request);
                        }

                        if (m_postRequests.Count > 0)
                        {
                            MicroserviceRequest request = m_postRequests.Peek();
                            Request(request);
                        }
                    }
                }
            }
        }


        private void GetRequest(GetReq request)
        {
            m_readyForNextRequest = false;
            m_HTTPClient.SetEndpoint(m_serviceEndpoint);
            m_HTTPClient.Get(request.URL(), OnGetResponse);
        }

        private void PostRequest(PostReq request)
        {
            m_readyForNextRequest = false;
            m_HTTPClient.SetEndpoint(m_serviceEndpoint);
            m_HTTPClient.PostJSON(request.URL(), request.JSONString(), OnPostResponse);
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
                    m_getRequests.Enqueue(request as GetReq);
                }
                else if (request is PostReq)
                {
                    m_postRequests.Enqueue(request as PostReq);
                }

                if (!m_readyForNextRequest)
                {
                    return;
                }

                if (request is GetReq)
                {
                    GetRequest(request as GetReq);
                }
                else if (request is PostReq)
                {
                    PostRequest(request as PostReq);
                }
            }
        }

        protected virtual void MockPost(PostReq request) { }
        protected virtual void MockGet(GetReq request) { }

        protected virtual void OnGetResponse(string JSON, GetReq originalRequest) { }
        protected virtual void OnPostResponse(string JSON, PostReq originalRequest) { }


        private void OnGetResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                OnGetResponse(JSON, m_getRequests.Dequeue());
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("GET Request was not successful");
#endif // DEBUG_LOG
            }
            m_readyForNextRequest = true;
        }

        private void OnPostResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                OnPostResponse(JSON, m_postRequests.Dequeue());
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("POST Request was not successful");
#endif // DEBUG_LOG
            }
            m_readyForNextRequest = true;
        }
    }
}
