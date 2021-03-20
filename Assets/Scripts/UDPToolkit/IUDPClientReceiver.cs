using UnityEngine;
using UnityEditor;

namespace ubv
{
    namespace udp
    {
        namespace client
        {
            public interface IUDPClientReceiver
            {
                void ReceivePacket(UDPToolkit.Packet packet);
                void OnDisconnect();
            }
        }
    }
}
