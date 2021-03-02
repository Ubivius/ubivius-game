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

            HTTPMockedServer server = null;
            Thread t = new Thread(() =>
            {
                server = new HTTPMockedServer();
            });
            t.Start();

            Assert.IsTrue(client.Get("products/1", out string content).Equals(System.Net.HttpStatusCode.OK));
            Assert.IsTrue(client.Get("products/99", out string content2).Equals(System.Net.HttpStatusCode.NotFound));
        }

        [Test]
        public void PostTest()
        {

        }

        [Test]
        public void PutTest()
        {

        }

        [Test]
        public void DeleteTest()
        {

        }

    }
}
