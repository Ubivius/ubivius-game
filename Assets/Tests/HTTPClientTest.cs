using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HTTPClientTest
    {   
        [Test]
        public void GetTest()
        {
            ubv.http.client.HTTPClient client = new GameObject().AddComponent<ubv.http.client.HTTPClient>();

            CreateMockHTTPServer();

            client.Get("products/1", (message) => { Assert.IsTrue(message.StatusCode.Equals(System.Net.HttpStatusCode.OK)); });
            client.Get("products/1", (message) => { Assert.IsTrue(message.StatusCode.Equals(System.Net.HttpStatusCode.NotFound)); });
        }

        [Test]
        public void PostTest()
        {
            ubv.http.client.HTTPClient client = new GameObject().AddComponent<ubv.http.client.HTTPClient>();

            CreateMockHTTPServer();
            
            client.Post("products", new object(), (message) => { Assert.IsTrue(message.StatusCode.Equals(System.Net.HttpStatusCode.Created)); });
        }

        [Test]
        public void PutTest()
        {
            ubv.http.client.HTTPClient client = new GameObject().AddComponent<ubv.http.client.HTTPClient>();

            CreateMockHTTPServer();

            client.Put("products", new object(), (message) => { Assert.IsTrue(message.StatusCode.Equals(System.Net.HttpStatusCode.Created)); });
        }

        [Test]
        public void DeleteTest()
        {
            ubv.http.client.HTTPClient client = new GameObject().AddComponent<ubv.http.client.HTTPClient>();

            CreateMockHTTPServer();

            client.Delete("products/1", (message) => { Assert.IsTrue(message.StatusCode.Equals(System.Net.HttpStatusCode.NoContent)); });
            client.Delete("products/99", (message) => { Assert.IsTrue(message.StatusCode.Equals(System.Net.HttpStatusCode.NotFound)); });
        }
        
        private void CreateMockHTTPServer()
        {
            HTTPMockedServer server = null;
            Thread t = new Thread(() =>
            {
                server = new HTTPMockedServer();
            });
            t.Start();
        }
    }
}
