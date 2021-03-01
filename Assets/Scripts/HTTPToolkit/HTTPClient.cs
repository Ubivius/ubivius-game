using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

namespace ubv.http
{
    /// <summary>
    /// HTTP client custom wrapper
    /// See https://zetcode.com/csharp/httpclient/ 
    /// and https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0
    /// </summary>
    public class HTTPClient : MonoBehaviour
    {
        static private readonly HttpClient m_client = new HttpClient();

        [SerializeField] string m_endPoint = "http://localhost:9090";

        private void Awake()
        {
            
        }
        
        public HttpResponseMessage PostJSON(string requestUrl, string jsonString)
        {
            StringContent data = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = m_client.PostAsync(m_endPoint + "/" + requestUrl, data).Result;
            
            return response;
        }

        public HttpResponseMessage Post(string requestUrl, object objToSerialize)
        {
            return PostJSON(requestUrl, JsonUtility.ToJson(objToSerialize));
        }

        public HttpResponseMessage Put(string requestUrl, string jsonString)
        {
            StringContent data = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = m_client.PutAsync(m_endPoint + "/" + requestUrl, data).Result;
            return response;
        }

        public HttpResponseMessage Put(string requestUrl, object objToSerialize)
        {
            return Put(requestUrl, JsonUtility.ToJson(objToSerialize));
        }

        public string Get(string requestUrl)
        {
            string responseContent = m_client.GetStringAsync(m_endPoint + "/" + requestUrl).Result;
            
            return responseContent;
        }

        public byte[] GetBytes(string requestUrl)
        {
            byte[] responseContent = m_client.GetByteArrayAsync(m_endPoint + "/" + requestUrl).Result;

            return responseContent;
        }

        public HttpResponseMessage Delete(string requestUrl)
        {
            HttpResponseMessage result = m_client.DeleteAsync(m_endPoint + "/" + requestUrl).Result;

            return result;
        }
    }
}