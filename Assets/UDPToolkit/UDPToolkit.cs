using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;
using System.Net.Sockets;

namespace UBV
{

    /// <summary>
    /// Various UDP functions and utilities
    /// https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
    /// </summary>
    public class UDPToolkit
    {
        private static readonly byte[] UDP_PROTOCOL_ID = { 0xAA, 0x0C, 0xC0, 0xFF };
        public const ushort UDP_PACKET_SIZE = 64; // size in bytes
        public const ushort UDP_MAX_PAYLOAD_SIZE = 16;
        public const ushort UDP_HEADER_SIZE = 4 + 4 + 4 + 4;
        
        /// <summary>
        /// Manages sequence numbers and packet acknowledgement, creates packets to be sent and deals with reception
        /// </summary>
        public class ConnectionData
        {
            public class PacketCallback : UnityEvent<Packet> { }
            
            private uint m_localSequence;
            private uint m_remoteSequence;
            private List<uint> m_received = new List<uint>();

            private int GetACKBitfield()
            {
                int bitfield = 0;
                for (ushort i = 0; i < 32; i++)
                {
                    if (m_remoteSequence > i && m_received.Contains(m_remoteSequence - i))
                        bitfield |= 1 << i;
                }
                return bitfield;
            }

            /// <summary>
            /// Creates a packet to be sent, with appropriate sequence and ACK
            /// </summary>
            /// <param name="data">Data to be sent within payload</param>
            /// <returns></returns>
            public Packet Send(byte[] Data)
            {
                return new Packet(Data, m_localSequence++, m_remoteSequence, GetACKBitfield());
            }
            
            /// <summary>
            /// Acknowledges packet and updates sequence numbers
            /// </summary>
            /// <param name="packet"></param>
            public void Receive(Packet packet)
            {
                if (packet.HasValidProtocolID())
                {
                    if (packet.Sequence > m_remoteSequence)
                        m_remoteSequence = packet.Sequence;

                    m_received.Add(packet.ACK);
                }
            }
            
        }

        public class Packet
        {
            public readonly byte[] RawBytes;
            public uint Sequence { get  { return System.BitConverter.ToUInt32(RawBytes, 4); }}
            public uint ACK { get { return System.BitConverter.ToUInt32(RawBytes, 8); } }
            public int ACK_Bitfield { get { return System.BitConverter.ToInt32(RawBytes, 12); } }
            public byte[] Data { get { return RawBytes.SubArray(16, UDP_PACKET_SIZE - UDP_HEADER_SIZE); } }

            private Packet(byte[] bytes)
            {
                RawBytes = bytes;
            }

            internal Packet(byte[] data, uint seq, uint ack, int ackBitfield)
            {
                RawBytes = new byte[UDP_PACKET_SIZE];

                ushort index = 0;
                for (; index < UDP_PROTOCOL_ID.Length; index++)
                    RawBytes[index] = UDP_PROTOCOL_ID[index];

                for (ushort i = 0; i < 4; i++, index++)
                    RawBytes[index] = System.BitConverter.GetBytes(seq)[i];

                for (ushort i = 0; i < 4; i++, index++)
                    RawBytes[index] = System.BitConverter.GetBytes(ack)[i];

                for (ushort i = 0; i < 4; i++, index++)
                    RawBytes[index] = System.BitConverter.GetBytes(ackBitfield)[i];

                for (ushort i = 0; i < UDP_MAX_PAYLOAD_SIZE; i++, index++)
                    RawBytes[index] = i < data.Length ? data[i] : (byte)0;
            }

            public bool HasValidProtocolID()
            {
                bool valid = true;
                for (ushort i = 0; i < UDP_PROTOCOL_ID.Length && valid; i++)
                {
                    if (RawBytes[i] != UDP_PROTOCOL_ID[i])
                        valid = false;
                }

                return valid;
            }

            public override string ToString()
            {
                return "Packet bytes: " + System.BitConverter.ToString(RawBytes);
            }

            public byte[] ToBytes()
            {
                return RawBytes;
            }

            public static Packet PacketFromBytes(byte[] bytes)
            {
                return new Packet(bytes);
            }
        }
    }

}