using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace ubv.http.client
{
    /// <summary>
    /// HTTP client custom wrapper
    /// See https://zetcode.com/csharp/httpclient/ 
    /// and https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0
    /// </summary>
    public class HTTPClient : MonoBehaviour
    {
        /*static private readonly HttpClient m_client = CreateHTTPClient();

        static private HttpClient CreateHTTPClient()
        {
            HttpClient client = new HttpClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.SystemDefault;
            return client;
        }*/

        private string m_endPoint = "http://localhost:9090";
        
        public delegate void HttpResponseMessageDelegate(UnityWebRequest response);

        private System.Net.Http.Headers.AuthenticationHeaderValue m_authHeader = null;

        public void SetAuthenticationToken(string token)
        {
            m_authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void SetEndpoint(string address)
        {
            m_endPoint = address;
        }

        public void PostJSON(string requestUrl, string jsonString, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            m_actionQueue.Enqueue(() => StartCoroutine(PostRequestCoroutine(requestUrl, jsonString, callbackOnResponse)));
        }

        public void Post(string requestUrl, object objToSerialize, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            PostJSON(requestUrl, JsonUtility.ToJson(objToSerialize), callbackOnResponse);
        }

        private IEnumerator PostRequestCoroutine(string requestUrl, string data, HttpResponseMessageDelegate callback)
        {
            UnityWebRequest test = UnityWebRequest.Post(m_endPoint + "/" + requestUrl, data);
            var cert = new ForceAcceptAll();
            test.certificateHandler = cert;

            test.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data));

            test.SetRequestHeader("Content-Type", "application/json");
            if (m_authHeader != null)
                test.SetRequestHeader("Authorization", m_authHeader.Scheme + " " + m_authHeader.Parameter);
            yield return test.SendWebRequest();
            callback?.Invoke(test);
        }

        public void PutJSON(string requestUrl, string jsonString, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            m_actionQueue.Enqueue(() => StartCoroutine(PutRequestCoroutine(requestUrl, jsonString, callbackOnResponse)));
        }

        private IEnumerator PutRequestCoroutine(string requestUrl, string data, HttpResponseMessageDelegate callback)
        {
            UnityWebRequest test = UnityWebRequest.Put(m_endPoint + "/" + requestUrl, data);
            var cert = new ForceAcceptAll();
            test.certificateHandler = cert;

            test.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data));

            test.SetRequestHeader("Content-Type", "application/json");
            if (m_authHeader != null)
                test.SetRequestHeader("Authorization", m_authHeader.Scheme + " " + m_authHeader.Parameter);
            yield return test.SendWebRequest();
            callback?.Invoke(test);
        }

        public void Put(string requestUrl, object objToSerialize, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            m_actionQueue.Enqueue(() => PutJSON(requestUrl, JsonUtility.ToJson(objToSerialize), callbackOnResponse));
        }

        public void Get(string requestUrl, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            m_actionQueue.Enqueue(() => StartCoroutine(GetRequestCoroutine(requestUrl, callbackOnResponse)));
        }

        private IEnumerator GetRequestCoroutine(string requestUrl, HttpResponseMessageDelegate callback)
        {
            UnityWebRequest test = UnityWebRequest.Get(m_endPoint + "/" + requestUrl);
            var cert = new ForceAcceptAll();
            test.certificateHandler = cert;
            if (m_authHeader != null)
                test.SetRequestHeader("Authorization", m_authHeader.Scheme + " " + m_authHeader.Parameter);
            yield return test.SendWebRequest();
            callback?.Invoke(test);
        }

        public void Delete(string requestUrl,  HttpResponseMessageDelegate callbackOnResponse = null)
        {
            m_actionQueue.Enqueue(() => StartCoroutine(DeleteRequestCoroutine(requestUrl, callbackOnResponse)));
        }

        private IEnumerator DeleteRequestCoroutine(string requestUrl, HttpResponseMessageDelegate callback)
        {
            UnityWebRequest test = UnityWebRequest.Delete(m_endPoint + "/" + requestUrl);
            var cert = new ForceAcceptAll();
            test.certificateHandler = cert;
            if (m_authHeader != null)
                test.SetRequestHeader("Authorization", m_authHeader.Scheme + " " + m_authHeader.Parameter);
            yield return test.SendWebRequest();
            callback?.Invoke(test);
        }

        private Queue<UnityAction> m_actionQueue = new Queue<UnityAction>();

        private void Update()
        {
            while(m_actionQueue.Count> 0)
            {
                m_actionQueue.Dequeue().Invoke();
            }
        }

        private class ForceAcceptAll : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
    }
}
