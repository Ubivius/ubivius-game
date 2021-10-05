using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace ubv.http.client
{
    /// <summary>
    /// HTTP client custom wrapper
    /// See https://zetcode.com/csharp/httpclient/ 
    /// and https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0
    /// </summary>
    public class HTTPClient : MonoBehaviour
    {
        static private readonly HttpClient m_client = new HttpClient();

        private string m_endPoint = "http://localhost:9090";
        
        public delegate void HttpResponseMessageDelegate(HttpResponseMessage response);

        public void SetAuthenticationToken(string token)
        {
            m_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void SetEndpoint(string address)
        {
            m_endPoint = address;
        }

        public void PostJSON(string requestUrl, string jsonString, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            new Task(() =>
            {
                StringContent data = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = m_client.PostAsync(m_endPoint + "/" + requestUrl, data).Result;
                callbackOnResponse?.Invoke(response);
            }).Start();
        }

        public void Post(string requestUrl, object objToSerialize, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            PostJSON(requestUrl, JsonUtility.ToJson(objToSerialize), callbackOnResponse);
        }

        public void PutJSON(string requestUrl, string jsonString, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            new Task(() =>
            {
                StringContent data = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = m_client.PutAsync(m_endPoint + "/" + requestUrl, data).Result;
                callbackOnResponse?.Invoke(response);
            }).Start();
        }

        public void Put(string requestUrl, object objToSerialize, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            PutJSON(requestUrl, JsonUtility.ToJson(objToSerialize), callbackOnResponse);
        }

        public void Get(string requestUrl, HttpResponseMessageDelegate callbackOnResponse = null)
        {
            new Task(() =>
            {
                HttpResponseMessage response = m_client.GetAsync(m_endPoint + "/" + requestUrl).Result;
                string responseContent = response.Content.ReadAsStringAsync().Result;
                callbackOnResponse?.Invoke(response);
            }).Start();
        }

        public void Delete(string requestUrl,  HttpResponseMessageDelegate callbackOnResponse = null)
        {
            new Task(() =>
            {
                HttpResponseMessage result = m_client.DeleteAsync(m_endPoint + "/" + requestUrl).Result;
                callbackOnResponse?.Invoke(result);
            }).Start();
            
        }
    }
}
