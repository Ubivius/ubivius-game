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
        static private HttpListener m_httpListener;
        private readonly string m_prefix;
        static private readonly List<string> m_productIDs = new List<string>()
        {
            "1", "2", "3", "4", "5", "7", "9"
        };
        static private bool m_running ;

        public HTTPMockedServer()
        {
            m_prefix = "http://localhost:9090";
            
            if (m_httpListener == null)
            {
                m_httpListener = new HttpListener();
                m_httpListener.Prefixes.Add(m_prefix + "/products/");

                m_running = true;
                m_httpListener.Start();
                Task listenTask = HandleConnections();
                listenTask.GetAwaiter().GetResult();

                m_httpListener.Close();
                m_httpListener = null;
            }
        }

        ~HTTPMockedServer()
        {
            Stop();
        }

        public void Stop()
        {
            m_running = false;
        }

        public static async Task HandleConnections()
        {
            while (m_running)
            {
                HttpListenerContext context = await m_httpListener.GetContextAsync();

                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                
                byte[] data = Encoding.UTF8.GetBytes("Successful data");

                // response codes: https://docs.microsoft.com/en-us/azure/architecture/best-practices/api-design#get-methods
                
                string[] parts = request.Url.LocalPath.Split('/');
                string id = parts[parts.Length - 1];
                switch (request.HttpMethod)
                {
                    case "POST":
                        response.StatusCode = 201;
                        break;
                    case "PUT":
                        response.StatusCode = 200;
                        break;
                    case "GET":
                        if (m_productIDs.Contains(id))
                        {
                            response.StatusCode = 200;
                        }
                        else
                        {
                            response.StatusCode = 404;
                        }
                        break;
                    case "DELETE":
                        if (m_productIDs.Contains(id))
                        {
                            response.StatusCode = 204;
                        }
                        else
                        {
                            response.StatusCode = 404;
                        }
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
