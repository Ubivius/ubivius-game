using UnityEngine;
using UnityEditor;

namespace ubv.udp.client
{
    public interface IPacketReceiver
    {
        void ReceivePacket(UDPToolkit.Packet packet);
    }
}
