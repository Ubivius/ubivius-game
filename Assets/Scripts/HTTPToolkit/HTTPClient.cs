using System.Collections;
using System.Collections.Generic;
using System.Net;
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

        [SerializeField] private string m_endPoint = "http://localhost:9090";
        
        public HttpStatusCode PostJSON(string requestUrl, string jsonString)
        {
            StringContent data = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = m_client.PostAsync(m_endPoint + "/" + requestUrl, data).Result;
            
            return response.StatusCode;
        }

        public HttpStatusCode Post(string requestUrl, object objToSerialize)
        {
            return PostJSON(requestUrl, JsonUtility.ToJson(objToSerialize));
        }

        public HttpStatusCode PutJSON(string requestUrl, string jsonString)
        {
            StringContent data = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = m_client.PutAsync(m_endPoint + "/" + requestUrl, data).Result;
            return response.StatusCode;
        }

        public HttpStatusCode Put(string requestUrl, object objToSerialize)
        {
            return PutJSON(requestUrl, JsonUtility.ToJson(objToSerialize));
        }

        public HttpStatusCode Get(string requestUrl, out string responseContent)
        {
            HttpResponseMessage response = m_client.GetAsync(m_endPoint + "/" + requestUrl).Result;
            responseContent = response.Content.ReadAsStringAsync().Result;
            return response.StatusCode;
        }

        public byte[] GetBytes(string requestUrl)
        {
            byte[] responseContent = m_client.GetByteArrayAsync(m_endPoint + "/" + requestUrl).Result;

            return responseContent;
        }

        public HttpStatusCode Delete(string requestUrl)
        {
            HttpResponseMessage result = m_client.DeleteAsync(m_endPoint + "/" + requestUrl).Result;

            return result.StatusCode;
        }
    }
}
