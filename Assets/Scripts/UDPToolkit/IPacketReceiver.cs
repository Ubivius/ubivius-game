using UnityEngine;
using UnityEditor;

namespace UBV
{
    public interface IPacketReceiver
    {
        void ReceivePacket(UDPToolkit.Packet packet);
    }
}