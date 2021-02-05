using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;
using System.Net.Sockets;

namespace ubv
{
    namespace tcp
    {
        /// <summary>
        /// Various TCP functions and utilities
        /// </summary>
        public class TCPToolkit
        {
            public class Packet : network.Packet
            {
                public byte[] Data { get { return RawBytes.SubArray(NET_PROTOCOL_ID.Length, RawBytes.Length - NET_PROTOCOL_ID.Length); } }
                
                private Packet(byte[] bytes) : base(bytes)
                {
                }
                
                public static Packet PacketFromData(byte[] data)
                {
                    Packet packet = new Packet(new byte[NET_PROTOCOL_ID.Length + data.Length]);
                    int index = 0;
                    for (ushort i = 0; i < 4; i++, index++)
                        packet.RawBytes[index] = NET_PROTOCOL_ID[i];

                    for (ushort i = 0; i < data.Length; i++, index++)
                        packet.RawBytes[index] = data[i];

                    return packet;
                }

                public static Packet PacketFromBytes(byte[] bytes)
                {
                    return new Packet(bytes);
                }
            }
        }

    }
}
