using UnityEngine;
using UnityEditor;

namespace ubv.tcp.client
{
    public interface ITCPClientReceiver
    {
        void OnSuccessfulTCPConnect();
        void ReceivePacket(TCPToolkit.Packet packet);
        void OnDisconnect();
    }
}
