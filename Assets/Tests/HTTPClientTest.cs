using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HTTPClientTest
    {
        private ubv.http.HTTPClient m_client;
        private HTTPMockedServer m_server;

        private void Init()
        {
            m_server = new HTTPMockedServer();
        }

        [Test]
        public void RunAllTests()
        {
            Init();
            GetTest();
            PostTest();
            PutTest();
            DeleteTest();
        }

        [Test]
        public void GetTest()
        {

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

        [UnityTest]
        public IEnumerator HTTPClientTestRoutine()
        {
            yield return null;
        }

    }
}
