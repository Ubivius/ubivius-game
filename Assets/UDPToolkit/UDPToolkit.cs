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
        public const short UDP_PACKET_SIZE = 256; // size in bytes
        public const short UDP_MAX_PAYLOAD_SIZE = 128;
        
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
                Data = data;
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
                for (i = 0; i < UDP_PROTOCOL_ID.Length; i++)
                {
                    arr[i] = UDP_PROTOCOL_ID[i];
                }

                arr[i++] = (byte)(Sequence);
                arr[i++] = (byte)(Sequence >> 8);

                arr[i++] = (byte)(ACK);
                arr[i++] = (byte)(ACK >> 8);
                
                arr[i++] = (byte)(ACK_Bitfield);
                arr[i++] = (byte)(ACK_Bitfield >> 8);
                arr[i++] = (byte)(ACK_Bitfield >> 16);
                arr[i++] = (byte)(ACK_Bitfield >> 24);
                
                return arr;
            }

            public static Packet PacketFromBytes(byte[] bytes)
            {
                byte[] data = new byte[UDP_MAX_PAYLOAD_SIZE];
                for(ushort i = 0; i < UDP_MAX_PAYLOAD_SIZE; i++)
                {
                    data[i] = bytes[i + 12];
                }
                return new Packet(data, bytes[4], bytes[6], bytes[8]);
            }
        }

        
    }

}