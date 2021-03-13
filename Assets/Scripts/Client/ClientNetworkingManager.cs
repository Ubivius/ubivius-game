using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.client.logic
{
    public class ClientNetworkingManager : MonoBehaviour
    {
        static public ClientNetworkingManager Instance { get; private set; } = null;

        public ClientSync ClientSync;
        public tcp.client.TCPClient TCPClient;
        public udp.client.UDPClient UDPClient;
        public http.HTTPClient HTTPClient;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
