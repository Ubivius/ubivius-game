using UnityEngine;
using System.Collections;

namespace ubv.server.logic
{
    public class ServerNetworkingManager : MonoBehaviour
    {
        static public ServerNetworkingManager Instance { get; private set; } = null;
        
        public tcp.server.TCPServer TCPServer;
        public udp.server.UDPServer UDPServer;
        public http.server.HTTPServer HTTPServer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
