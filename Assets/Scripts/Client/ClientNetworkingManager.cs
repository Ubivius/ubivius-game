using System.Collections;
using System.Collections.Generic;
using ubv.microservices;
using UnityEngine;

namespace ubv.client.logic
{
    public class ClientNetworkingManager : MonoBehaviour
    {
        static public ClientNetworkingManager Instance { get; private set; } = null;
        
        public tcp.client.TCPClient TCPClient;
        public udp.client.UDPClient UDPClient;
        public http.HTTPClient HTTPClient;
        public microservices.DispatcherMicroservice Dispatcher;

        public AuthenticationService Authentication;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
