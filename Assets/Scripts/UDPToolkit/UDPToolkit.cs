using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;
using System.Net.Sockets;

namespace ubv
{
    namespace udp
    {
        /// <summary>
        /// Various UDP functions and utilities
        /// https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
        /// </summary>
        public class UDPToolkit
        {
            /// <summary>
            /// Manages sequence numbers and packet acknowledgement, creates packets to be sent and deals with reception
            /// </summary>
            public class ConnectionData
            {
                private uint m_localSequence;
                private uint m_remoteSequence;

                public ConnectionData()
                {
                    m_localSequence = 1;
                    m_remoteSequence = 0;
                }

                /// <summary>
                /// Creates a packet to be sent, with appropriate sequence and ACK
                /// </summary>
                /// <param name="data">Data to be sent within payload</param>
                /// <returns></returns>
                public Packet Send(byte[] Data, int playerID)
                {
                    Packet p = new Packet(Data, m_localSequence, m_remoteSequence, playerID);
                    ++m_localSequence;
                    return p;
                }

                /// <summary>
                /// Acknowledges packet and updates sequence numbers
                /// </summary>
                /// <param name="packet"></param>
                public bool Receive(Packet packet)
                {
                    if (packet.HasValidProtocolID())
                    {
                        if (packet.Sequence > m_remoteSequence)
                            m_remoteSequence = packet.Sequence;

                        return true;
                    }
                    return false;
                }

            }

            public class Packet : network.Packet
            {
                public const ushort UDP_MAX_PAYLOAD_SIZE = 2048 * 4; // TODO: WARN WHEN EXCEEDING THIS AND FIND RIGHT SIZE
                public const ushort UDP_HEADER_SIZE = DEFAULT_HEADER_SIZE + (2 * sizeof(int));
                public const ushort UDP_PACKET_SIZE = UDP_HEADER_SIZE + UDP_MAX_PAYLOAD_SIZE; // size in bytes

                public uint Sequence { get { return System.BitConverter.ToUInt32(RawBytes, 12); } }
                public uint ACK { get { return System.BitConverter.ToUInt32(RawBytes, 16); } }
                public byte[] Data { get { return RawBytes.SubArray(UDP_HEADER_SIZE, DataSize); } }
                
                private Packet(byte[] bytes) : base(bytes)
                {

                }

                internal Packet(byte[] data, uint seq, uint ack, int playerID) : base(new byte[UDP_PACKET_SIZE])
                {
                    ushort index = 0;
                    
                    for (ushort i = 0; i < 4; i++, index++)
                        RawBytes[index] = NET_PROTOCOL_ID[i];

                    for (ushort i = 0; i < 4; i++, index++)
                        RawBytes[index] = System.BitConverter.GetBytes(data.Length)[i];

                    for (ushort i = 0; i < 4; i++, index++)
                        RawBytes[index] = System.BitConverter.GetBytes(playerID)[i];

                    for (ushort i = 0; i < 4; i++, index++)
                        RawBytes[index] = System.BitConverter.GetBytes(seq)[i];

                    for (ushort i = 0; i < 4; i++, index++)
                        RawBytes[index] = System.BitConverter.GetBytes(ack)[i];
                    

                    for (ushort i = 0; i < UDP_MAX_PAYLOAD_SIZE; i++, index++)
                        RawBytes[index] = i < data.Length ? data[i] : (byte)0;
                }


                public override string ToString()
                {
                    return "Packet bytes: " + System.BitConverter.ToString(RawBytes);
                }
                
                public static Packet FirstPacketFromBytes(byte[] bytes)
                {
                    if (bytes.Length < UDP_HEADER_SIZE)
                    {
                        return null;
                    }

                    Packet packet = new Packet(bytes.SubArray(0, UDP_HEADER_SIZE));

                    if (!packet.HasValidProtocolID())
                    {
                        return null;
                    }

                    int byteCount = packet.DataSize + UDP_HEADER_SIZE;

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
