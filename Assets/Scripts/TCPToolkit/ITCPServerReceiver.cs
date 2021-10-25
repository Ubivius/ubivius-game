using UnityEngine;
using System.Collections;

namespace ubv.tcp.server
{
    public interface ITCPServerReceiver
    {
        void TCPReceive(TCPToolkit.Packet packet, int playerID);
        void OnTCPConnect(int playerID);
        void OnTCPDisconnect(int playerID);
    }
}
