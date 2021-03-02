using UnityEngine;
using System.Collections;
using System.Net;
using System.Threading.Tasks;

namespace Tests
{
    public class HTTPMockedServer
    {
        private HttpListener m_httpListener;
        private readonly string m_prefix;
        private readonly string[] m_urls;

        public HTTPMockedServer()
        {
            m_prefix = "http://localhost:9090";
            m_urls = new string[] {
                "products"
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


            }
        }
    }
}
