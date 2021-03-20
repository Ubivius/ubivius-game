using UnityEngine;
using UnityEditor;

namespace ubv.tcp.client
{
    public interface ITCPClientReceiver
    {
        void ReceivePacket(TCPToolkit.Packet packet);
        void OnDisconnect();
    }
}
