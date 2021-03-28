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
                void Receive(UDPToolkit.Packet packet, int playerID);
            }

        }
    }
}
