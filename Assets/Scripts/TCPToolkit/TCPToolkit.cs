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
                
                public static Packet PacketFromBytes(byte[] bytes)
                {
                    return new Packet(bytes);
                }
            }
        }

    }
}
