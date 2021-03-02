using UnityEngine;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

namespace Tests
{
    public class HTTPMockedServer
    {
        private HttpListener m_httpListener;
        private readonly string m_prefix;
        private readonly string[] m_urls;
        private List<string> m_productsJSONs;

        public HTTPMockedServer()
        {
            m_prefix = "http://localhost:9090";
            m_urls = new string[] {
                "products"
            };

            m_productsJSONs = new List<string>()
            {
                "{\"id\":1, \"name\":\"newName\", \"price\":1.00, \"sku\":\"abc-abc-abcd\"}"
            };

            m_httpListener = new HttpListener();
            foreach (string url in m_urls)
            {
                m_httpListener.Prefixes.Add(m_prefix + "/" + url);
            }

            m_httpListener.Start();
        }

        public async Task HandleConnections()
        {
            bool run = true;

            while (run)
            {
                HttpListenerContext context = await m_httpListener.GetContextAsync();

                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                
                StringBuilder JSONBuilder = new StringBuilder();
                foreach (string str in m_productsJSONs)
                {
                    JSONBuilder.Append(str);
                }

                byte[] data = Encoding.UTF8.GetBytes(JSONBuilder.ToString());

                switch (request.HttpMethod)
                {
                    case "POST":
                        break;
                    case "PUT":
                        break;
                    case "GET":
                        break;
                    case "DELETE":
                        break;
                    default:
                        break;
                }

                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = data.LongLength;

                await response.OutputStream.WriteAsync(data, 0, data.Length);
                response.Close();
            }
        }
    }
}
