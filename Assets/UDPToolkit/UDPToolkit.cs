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
        public const ushort UDP_PACKET_SIZE = 256; // size in bytes
        public const ushort UDP_MAX_PAYLOAD_SIZE = 128;
        
        public class ConnectionData
        {
            public class PacketCallback : UnityEvent<Packet> { }
            
            private ushort m_localSequence;
            private ushort m_remoteSequence;
            private List<ushort> m_received = new List<ushort>();

            private int GetACKBitfield()
            {
                int bitfield = 0;
                for (ushort i = 0; i < 32; i++)
                {
                    if (m_remoteSequence > i && m_received.Contains((ushort)(m_remoteSequence - i)))
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
            public readonly byte[] ProtocolID;
            public readonly ushort Sequence;
            public readonly ushort ACK;
            public readonly int ACK_Bitfield;
            public readonly byte[] Data; 

            public Packet(byte[] data, ushort seq, ushort ack, int ackBitfield)
            {
                ProtocolID = UDP_PROTOCOL_ID;
                Data = new byte[UDP_MAX_PAYLOAD_SIZE];
                for (ushort i = 0; i < UDP_MAX_PAYLOAD_SIZE; i++)
                {
                    Data[i] = i < data.Length ? data[i] : (byte)0;
                }
                Sequence = seq;
                ACK = ack;
                ACK_Bitfield = ackBitfield;
            }

            public  bool HasValidProtocolID()
            {
                bool valid = true;
                for (ushort i = 0; i < UDP_PROTOCOL_ID.Length && valid; i++)
                {
                    if (ProtocolID[i] != UDP_PROTOCOL_ID[i])
                        valid = false;
                }

                return valid;
            }

            public override string ToString()
            {
                return "Packet: " + Sequence.ToString() +
                    " " + ACK.ToString() +
                    " " + System.Convert.ToString(ACK_Bitfield, 2) +
                    " " + Encoding.Default.GetString(Data);
            }

            public byte[] ToBytes()
            {
                byte[] arr = new byte[UDP_PACKET_SIZE];
                ushort i;
                int currentLimit = UDP_PROTOCOL_ID.Length;
                for (i = 0; i < currentLimit; i++)
                    arr[i] = UDP_PROTOCOL_ID[i];

                byte[] sequenceBytes = System.BitConverter.GetBytes(Sequence);
                for (; i < currentLimit + sequenceBytes.Length; i++)
                    arr[i] = sequenceBytes[i - currentLimit];
                currentLimit += sequenceBytes.Length;

                byte[] ACKBytes = System.BitConverter.GetBytes(ACK);
                for (; i < currentLimit + ACKBytes.Length; i++)
                    arr[i] = ACKBytes[i - currentLimit];
                currentLimit += ACKBytes.Length;
                
                byte[] bitFieldBytes = System.BitConverter.GetBytes(ACK_Bitfield);
                for (; i < currentLimit + bitFieldBytes.Length; i++)
                    arr[i] = bitFieldBytes[i - currentLimit];
                currentLimit += bitFieldBytes.Length;

                for (; i < UDP_MAX_PAYLOAD_SIZE + currentLimit; i++)
                    arr[i] = Data[i - 12];
                
                return arr;
            }

            public static Packet PacketFromBytes(byte[] bytes)
            {
                byte[] data = new byte[UDP_MAX_PAYLOAD_SIZE];
                for(ushort i = UDP_MAX_PAYLOAD_SIZE; i > 0; i--)
                {
                    data[UDP_MAX_PAYLOAD_SIZE - i] = bytes[i + 12];
                }

                return new Packet(data, 
                    System.BitConverter.ToUInt16(bytes, 4),
                    System.BitConverter.ToUInt16(bytes, 6),
                    System.BitConverter.ToInt32(bytes, 8));
            }
        }

        
    }

}