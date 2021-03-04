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
            ubv.http.HTTPClient client = new GameObject().AddComponent<ubv.http.HTTPClient>();

            CreateMockHTTPServer();

            Assert.IsTrue(client.Get("products/1", out string content).Equals(System.Net.HttpStatusCode.OK));
            Assert.IsTrue(client.Get("products/99", out string content2).Equals(System.Net.HttpStatusCode.NotFound));
        }

        [Test]
        public void PostTest()
        {
            ubv.http.HTTPClient client = new GameObject().AddComponent<ubv.http.HTTPClient>();

            CreateMockHTTPServer();

            Assert.IsTrue(client.Post("products", new object()).Equals(System.Net.HttpStatusCode.Created));
        }

        [Test]
        public void PutTest()
        {
            ubv.http.HTTPClient client = new GameObject().AddComponent<ubv.http.HTTPClient>();

            CreateMockHTTPServer();

            Assert.IsTrue(client.Put("products", new object()).Equals(System.Net.HttpStatusCode.OK));
        }

        [Test]
        public void DeleteTest()
        {
            ubv.http.HTTPClient client = new GameObject().AddComponent<ubv.http.HTTPClient>();

            CreateMockHTTPServer();

            Assert.IsTrue(client.Delete("products/1").Equals(System.Net.HttpStatusCode.NoContent));
            Assert.IsTrue(client.Delete("products/99").Equals(System.Net.HttpStatusCode.NotFound));
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
