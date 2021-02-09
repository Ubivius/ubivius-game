using UnityEngine;
using UnityEditor;

namespace ubv.udp.client
{
    public interface IPacketReceiver
    {
<<<<<<< HEAD:Assets/Scripts/UDPToolkit/IPacketReceiver.cs
        void ReceivePacket(UDPToolkit.Packet packet);
=======
        namespace client
        {
            public interface IUDPClientReceiver
            {
                void ReceivePacket(UDPToolkit.Packet packet);
            }
        }
>>>>>>> origin/master:Assets/Scripts/UDPToolkit/IUDPClientReceiver.cs
    }
}
