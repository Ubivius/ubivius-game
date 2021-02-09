using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;
using System.Net.Sockets;

namespace ubv.udp
{
    /// <summary>
    /// Various UDP functions and utilities
    /// https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
    /// </summary>
    public class UDPToolkit
    {
        private static readonly byte[] UDP_PROTOCOL_ID = { 0xAA, 0x0C, 0xC0, 0xFF };
        public const ushort UDP_MAX_PAYLOAD_SIZE = 512 * 2 * 2; // TODO: WARN WHEN EXCEEDING THIS AND FIND RIGHT SIZE
        public const ushort UDP_HEADER_SIZE = 5 * sizeof(int);
        public const ushort UDP_PACKET_SIZE = UDP_HEADER_SIZE + UDP_MAX_PAYLOAD_SIZE; // size in bytes

        /// <summary>
        /// Manages sequence numbers and packet acknowledgement, creates packets to be sent and deals with reception
        /// </summary>
        public class ConnectionData
        {
<<<<<<< HEAD
            private uint m_localSequence;
            private uint m_remoteSequence;
            private Queue<uint> m_acknowledged; // packets we sent and know other side received
=======
            public const ushort UDP_MAX_PAYLOAD_SIZE = 512 * 2 * 2; // TODO: WARN WHEN EXCEEDING THIS AND FIND RIGHT SIZE
            public const ushort UDP_HEADER_SIZE = 5 * sizeof(int);
            public const ushort UDP_PACKET_SIZE = UDP_HEADER_SIZE + UDP_MAX_PAYLOAD_SIZE; // size in bytes
>>>>>>> origin/master

            public ConnectionData()
            {
                m_localSequence = 1;
                m_remoteSequence = 0;
                m_acknowledged = new Queue<uint>();
            }

            private int GenerateACKBitfield()
            {
                int bitfield = 0;
                for (ushort i = 0; i < 32; i++)
                {
                    if (m_remoteSequence > i && m_acknowledged.Contains(m_remoteSequence - i))
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
                return new Packet(Data, m_localSequence++, m_remoteSequence, GenerateACKBitfield());
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

                    uint ack = packet.ACK;
                    // ack packet 
                    if (ack > 0 && !m_acknowledged.Contains(ack))
                        m_acknowledged.Enqueue(ack);

                    // ack bitfield
                    int bitfield = packet.ACK_Bitfield;
                    for (ushort i = 0; i < 32; i++)
                    {
                        if ((bitfield & (1 << i)) == 1 && !m_acknowledged.Contains(ack - i))
                            m_acknowledged.Enqueue(ack - i);
                    }

                    while (m_acknowledged.Count > 32)
                        m_acknowledged.Dequeue();

                    return true;
                }
                return false;
            }

<<<<<<< HEAD
        }

        public class Packet
        {
            public readonly byte[] RawBytes;
            public uint Sequence { get { return System.BitConverter.ToUInt32(RawBytes, 4); } }
            public uint ACK { get { return System.BitConverter.ToUInt32(RawBytes, 8); } }
            public int ACK_Bitfield { get { return System.BitConverter.ToInt32(RawBytes, 12); } }
            public int DataSize { get { return System.BitConverter.ToInt32(RawBytes, 16); } }
            public byte[] Data { get { return RawBytes.SubArray(UDP_HEADER_SIZE, DataSize); } }

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
=======
            public class Packet : network.Packet
            {
                public uint Sequence { get { return System.BitConverter.ToUInt32(RawBytes, 4); } }
                public uint ACK { get { return System.BitConverter.ToUInt32(RawBytes, 8); } }
                public int ACK_Bitfield { get { return System.BitConverter.ToInt32(RawBytes, 12); } }
                public int DataSize { get { return System.BitConverter.ToInt32(RawBytes, 16); } }
                public byte[] Data { get { return RawBytes.SubArray(UDP_HEADER_SIZE, DataSize); } }
                
                private Packet(byte[] bytes) : base(bytes)
                {

                }

                internal Packet(byte[] data, uint seq, uint ack, int ackBitfield) : base(new byte[UDP_PACKET_SIZE])
                {
                    ushort index = 0;
                    
                    for (ushort i = 0; i < 4; i++, index++)
                        RawBytes[index] = NET_PROTOCOL_ID[i];
>>>>>>> origin/master

                for (ushort i = 0; i < 4; i++, index++)
                    RawBytes[index] = System.BitConverter.GetBytes(seq)[i];

                for (ushort i = 0; i < 4; i++, index++)
                    RawBytes[index] = System.BitConverter.GetBytes(ack)[i];

                for (ushort i = 0; i < 4; i++, index++)
                    RawBytes[index] = System.BitConverter.GetBytes(ackBitfield)[i];

                for (ushort i = 0; i < 4; i++, index++)
                    RawBytes[index] = System.BitConverter.GetBytes(data.Length)[i];

                for (ushort i = 0; i < UDP_MAX_PAYLOAD_SIZE; i++, index++)
                    RawBytes[index] = i < data.Length ? data[i] : (byte)0;
            }

<<<<<<< HEAD
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
=======

                public override string ToString()
                {
                    return "Packet bytes: " + System.BitConverter.ToString(RawBytes);
                }
                
                public static Packet PacketFromBytes(byte[] bytes)
                {
                    return new Packet(bytes);
                }
>>>>>>> origin/master
            }

            public static Packet PacketFromBytes(byte[] bytes)
            {
                return new Packet(bytes);
            }
        }
    }
}
