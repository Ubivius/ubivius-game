using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace udp
    {
        namespace server
        {
            public interface IServerReceiver
            {
                void Receive(UDPToolkit.Packet packet, System.Net.IPEndPoint clientIP);

                void OnConnect(System.Net.IPEndPoint clientIP);
                void OnDisconnect(System.Net.IPEndPoint clientIP);
            }

        }
    }
}
