﻿using System.Collections;
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
        private HttpClient m_client;

        [SerializeField] string m_target;

        private void Awake()
        {
            m_client = new HttpClient();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}