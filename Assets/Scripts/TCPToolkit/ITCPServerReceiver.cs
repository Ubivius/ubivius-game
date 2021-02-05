using UnityEngine;
using System.Collections;

namespace ubv.tcp.server
{
    public interface ITCPServerReceiver
    {
        void Receive(TCPToolkit.Packet packet, System.Net.IPEndPoint clientIP);
    }
}
