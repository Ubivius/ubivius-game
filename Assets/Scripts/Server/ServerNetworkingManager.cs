using UnityEngine;
using System.Collections;

namespace ubv.server.logic
{
    public class ServerNetworkingManager : MonoBehaviour
    {
        static public ServerNetworkingManager Instance { get; private set; } = null;

        public const int SERVER_TICK_BUFFER_SIZE = 4;

        public tcp.server.TCPServer TCPServer;
        public udp.server.UDPServer UDPServer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
