using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

namespace ubv.common.http
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

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log(Get("products"));
            PostJSON("products", "{\"name\":\"New Object added by Murphy\", \"price\":69.69, \"sku\":\"abc-abc-abcd\"}");
            Debug.Log(Get("products"));
            Delete("products/4");
            Debug.Log(Get("products"));
            Put("products", "{ \"id\":1 \"name\":\"Object put(modified) by Murphy\", \"price\":00.01, \"sku\":\"abc-abc-abcd\"}");
            Debug.Log(Get("products"));
        }

        // Update is called once per frame
        void Update()
        {

        }
        
        public void PostJSON(string requestUrl, string jsonString)
        {
            StringContent data = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = m_client.PostAsync(m_endPoint + "/" + requestUrl, data).Result;

#if DEBUG_LOG
            Debug.Log("POST response : " + response.Headers.ToString());
#endif //DEBUG_LOG
        }

        public void Post(string requestUrl, object objToSerialize)
        {
            PostJSON(requestUrl, JsonUtility.ToJson(objToSerialize));
        }

        public void Put(string requestUrl, string jsonString)
        {
            StringContent data = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = m_client.PutAsync(m_endPoint + "/" + requestUrl, data).Result;

#if DEBUG_LOG
            Debug.Log("PUT response : " + response.Headers.ToString());
#endif //DEBUG_LOG
        }

        public void Put(string requestUrl, object objToSerialize)
        {
            Put(requestUrl, JsonUtility.ToJson(objToSerialize));
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

        public void Delete(string requestUrl)
        {
            HttpResponseMessage result = m_client.DeleteAsync(m_endPoint + "/" + requestUrl).Result;

            if (!result.IsSuccessStatusCode)
            {
                Debug.Log("DELETE to " + requestUrl + " failed");
            }
        }
    }
}