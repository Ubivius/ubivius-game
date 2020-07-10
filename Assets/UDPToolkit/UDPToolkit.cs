using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Various UDP functions 
/// </summary>
public class UDPToolkit
{
    private static readonly byte[] UDP_PROTOCOL_ID = { 0xAA, 0x0C, 0xC0, 0xFF };
    public const short UDP_PACKET_SIZE = 64; // size in bytes

    public class ConnectionData : ISenderReceiver
    {
        public ushort LocalSequence;
        public ushort RemoteSequence;
        private List<ushort> m_received = new List<ushort>();

        public int GetACKBitfield()
        {
            int bitfield = 0;
            for(ushort i = 0; i < 32; i++)
            {
                if(RemoteSequence > i && m_received.Contains((ushort)(RemoteSequence - i)))
                    bitfield |= 1 << i;
            }
            return bitfield;
        }

        public void Send(uint Data, ISenderReceiver receiver)
        {
            int bitfield = 0;

            receiver.Receive(new Packet(Data, LocalSequence++, RemoteSequence, bitfield));
        }

        public void Receive(Packet packet)
        {
            if (packet.Sequence > RemoteSequence)
                RemoteSequence = packet.Sequence;

            m_received.Add(packet.ACK);
        }
    }

    public struct Packet
    {
        public Packet(uint data, ushort seq, ushort ack, int ackBitfield)
        {
            ProtocolID = UDP_PROTOCOL_ID;
            Data = data;
            Sequence = seq;
            ACK = ack;
            ACK_Bitfield = ackBitfield;
        }

        public byte[] ProtocolID { get; }
        public ushort Sequence { get; }
        public ushort ACK { get; }
        public int ACK_Bitfield { get; }
        public uint Data { get; } 
    }

    public static bool HasValidProtocolID(Packet packet)
    {
        bool valid = true;
        for (ushort i = 0; i < UDP_PROTOCOL_ID.Length && valid; i++)
        {
            if (packet.ProtocolID[i] != UDP_PROTOCOL_ID[i])
                valid = false;
        }

        return valid;
    }
}
