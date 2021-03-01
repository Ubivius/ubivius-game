using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HTTPClientTest
    {
        [UnityTest]
        public IEnumerator HTTPClientTest_Works()
        {
            yield return null;
        }

        public class HTTPClientMonoBehaviourTest : MonoBehaviour, IMonoBehaviourTest
        {
            bool IMonoBehaviourTest.IsTestFinished
            {
                get { return false; }
            }
        }

        ubv.http.HTTPClient m_client;

        private void Init()
        {
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
    }
}
