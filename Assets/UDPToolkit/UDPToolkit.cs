using UnityEngine;
using System.Collections;

/// <summary>
/// Various UDP functions 
/// </summary>
public class UDPToolkit
{
    private static readonly byte[] UDP_PROTOCOL_ID = { 0xAA, 0x0C, 0xC0, 0xFF };
    public const short UDP_PACKET_SIZE = 64; // size in bytes

    public class ConnectionData
    {
        public uint LocalSequence;
        public uint RemoteSequence;
    }

    public struct Packet
    {
        public Packet(uint data, uint seq, uint ack, uint ackBitfield)
        {
            ProtocolID = UDP_PROTOCOL_ID;
            Data = data;
            Sequence = seq;
            ACK = ack;
            ACK_Bitfield = ackBitfield;
        }

        public byte[] ProtocolID { get; }
        public uint Sequence { get; }
        public uint ACK { get; }
        public uint ACK_Bitfield { get; }
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
