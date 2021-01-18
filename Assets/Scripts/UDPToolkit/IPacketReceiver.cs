using UnityEngine;
using UnityEditor;

namespace ubv
{
    namespace udp
    {
        namespace client
        {
            public interface IPacketReceiver
            {
                void ReceivePacket(UDPToolkit.Packet packet);
            }
        }
    }
}
