using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace udp
    {
        namespace server
        {
            public interface IUDPServerReceiver
            {
                void UDPReceive(UDPToolkit.Packet packet, int playerID);
            }

        }
    }
}
