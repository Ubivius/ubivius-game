using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ubv.http.server
{
    public class HTTPServer : MonoBehaviour
    {
        protected bool m_exitSignal;
        //Health probe listener
        private HttpListener m_healthListener;
        private void Awake()
        {
            m_exitSignal = false;
            m_healthListener = new HttpListener();
            string[] healthPaths = new string[2] { "http://+:9100/liveness/", "http://+:9100/readiness/" };
            foreach (string s in healthPaths)
            {
                m_healthListener.Prefixes.Add(s);
            }
        }

        private void Start()
        {
            m_healthListener.Start();
            Thread t1 = new Thread(new ThreadStart(CommThread));
            t1.Start();
        }

        private void CommThread()
        {
            while (!m_exitSignal)
            {
                HttpListenerContext context = m_healthListener.GetContext();
                Debug.Log("Get Request recieved");
                Thread send = new Thread(SendingThread);
                send.Start(context);
                send.Join();
            }
            
        }

        private void SendingThread(object ctxt)
        {
            HttpListenerContext context = (HttpListenerContext)ctxt;
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = "<HTML><BODY>OK</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            try
            {
                Task.Delay(50, new CancellationToken(m_exitSignal)).Wait();
            }
            catch (AggregateException ex)
            {
#if DEBUG_LOG
                Debug.Log(ex.Message);
#endif // DEBUG_LOG
            }
        }

        private void OnDestroy()
        {
            m_exitSignal = true;
        }
    }
}
