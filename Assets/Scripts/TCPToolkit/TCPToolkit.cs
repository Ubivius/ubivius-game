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
                public const int TCP_HEADER_SIZE = DEFAULT_HEADER_SIZE; // NET PROTOCOL ID + PlayerID + Data length
                public byte[] Data { get { return RawBytes.ArrayFrom(TCP_HEADER_SIZE); } }
                
                private Packet(byte[] bytes) : base(bytes)
                { }
                
                public static Packet DataToPacket(byte[] data, int playerID)
                {
                    Packet packet = new Packet(new byte[TCP_HEADER_SIZE + data.Length]);
                    int index = 0;
                    for (int i = 0; i < 4; i++, index++)
                        packet.RawBytes[index] = NET_PROTOCOL_ID[i];

                    byte[] payloadSizeBytes = System.BitConverter.GetBytes(data.Length);

                    for (int i = 0; i < payloadSizeBytes.Length; i++, index++)
                        packet.RawBytes[index] = payloadSizeBytes[i];

                    for (int i = 0; i < 4; i++, index++)
                        packet.RawBytes[index] = System.BitConverter.GetBytes(playerID)[i];

                    for (int i = 0; i < data.Length; i++, index++)
                        packet.RawBytes[index] = data[i];

                    return packet;
                }

                public static Packet PacketFromBytes(byte[] bytes)
                {
                    return new Packet(bytes);
                }

                public static Packet FirstPacketFromBytes(byte[] bytes)
                {
                    if(bytes.Length < TCP_HEADER_SIZE)
                    {
                        return null;
                    }
                    
                    Packet packet = new Packet(bytes.SubArray(0, TCP_HEADER_SIZE));

                    if (!packet.HasValidProtocolID())
                    {
                        return null;
                    }

                    int byteCount = packet.DataSize + TCP_HEADER_SIZE;
                    
                    if (bytes.Length < byteCount)
                    {
                        return null;
                    }

                    packet = new Packet(bytes.SubArray(0, byteCount));
                    
                    return packet;
                }
            }
        }

    }
}
