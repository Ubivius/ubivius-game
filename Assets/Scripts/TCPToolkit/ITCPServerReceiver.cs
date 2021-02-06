using UnityEngine;
using System.Collections;

namespace ubv.tcp.server
{
    public interface ITCPServerReceiver
    {
        void ReceivePacket(TCPToolkit.Packet packet, System.Net.IPEndPoint clientIP);
        void OnConnect(System.Net.IPEndPoint clientIP);
        void OnDisconnect(System.Net.IPEndPoint clientIP);
    }
}
