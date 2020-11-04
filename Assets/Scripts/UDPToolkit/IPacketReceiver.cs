using UnityEngine;
using UnityEditor;

namespace ubv
{
    public interface IPacketReceiver
    {
        void ReceivePacket(UDPToolkit.Packet packet);
    }
}