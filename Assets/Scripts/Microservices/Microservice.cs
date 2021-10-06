using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

namespace ubv.microservices
{
    public abstract class Microservice : MonoBehaviour
    {
        public abstract class MicroserviceRequest
        {
            public abstract string GetURL();
            public MicroserviceCallback Callback;
        }
        public abstract class MicroserviceCallback { }

        public const int REQUEST_CHECK_TIME = 13;
        protected readonly object m_requestLock = new object();

        [SerializeField] protected bool m_mock;
        [SerializeField] protected utils.Mocker m_mockData;

        [SerializeField] protected HTTPClient m_HTTPClient;
        [SerializeField] protected string m_serviceEndpoint;

        protected bool m_readyForNextRequest;
        protected Queue<MicroserviceRequest> m_requests;

        private void Awake()
        {
            m_readyForNextRequest = true;
            m_requests = new Queue<MicroserviceRequest>();
        }

        private void Update()
        {
            if (Time.frameCount % REQUEST_CHECK_TIME == 0)
            {
                lock (m_requestLock)
                {
                    if (m_readyForNextRequest)
                    {
                        if (m_requests.Count > 0)
                        {
                            MicroserviceRequest request = m_requests.Peek();
                            SendRequest(request);
                        }
                    }
                }
            }
        }
        

        protected void SendRequest(MicroserviceRequest request)
        {
            m_readyForNextRequest = false;
            m_HTTPClient.SetEndpoint(m_serviceEndpoint);
            m_HTTPClient.Get(request.GetURL(), OnResponse);
        }

        public void Request(MicroserviceRequest request)
        {
            if (m_mock)
            {
                Mock(request.Callback);
                return;
            }

            lock (m_requestLock)
            {
                m_requests.Enqueue(request);

                if (!m_readyForNextRequest)
                {
                    return;
                }

                SendRequest(request);
            }
        }

        protected abstract void Mock(MicroserviceCallback callback);

        protected abstract void OnResponse(string JSON, MicroserviceRequest originalRequest);

        private void OnResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                OnResponse(JSON, m_requests.Dequeue());
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Request was not successful");
#endif // DEBUG_LOG
            }
            m_readyForNextRequest = true;
        }
    }
}
