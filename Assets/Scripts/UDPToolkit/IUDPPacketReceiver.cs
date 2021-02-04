using UnityEngine;
using UnityEditor;

namespace ubv
{
    namespace udp
    {
        namespace client
        {
            public interface IUDPPacketReceiver
            {
                void ReceivePacket(UDPToolkit.Packet packet);
            }
        }
    }
}
