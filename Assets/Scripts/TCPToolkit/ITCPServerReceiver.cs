using UnityEngine;
using System.Collections;

namespace ubv.tcp.server
{
    public interface ITCPServerReceiver
    {
        void ReceivePacket(TCPToolkit.Packet packet, int playerID);
        void OnConnect(int playerID);
        void OnDisconnect(int playerID);
    }
}
