using UnityEngine;
using UnityEditor;

namespace ubv.tcp.client
{
    public interface ITCPClientReceiver
    {
        void OnSuccessfulConnect();
        void ReceivePacket(TCPToolkit.Packet packet);
        void OnDisconnect();
    }
}
