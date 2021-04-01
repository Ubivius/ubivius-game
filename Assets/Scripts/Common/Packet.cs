using UnityEngine;
using System.Collections;

namespace ubv.network
{
    public class Packet
    {
        // The header of a packet is ALWAYS (TCP or UDP, both are the same)
        /*
            NET_PROTOCOL_ID // 4 bytes
            PAYLOAD_LENTGH // 4 bytes, describes the size of the rest of the packet
            PLAYER_ID // 4 bytes
        */
        protected static readonly byte[] NET_PROTOCOL_ID = { 0xAA, 0x0C, 0xC0, 0xFF };
        public int DataSize { get { return System.BitConverter.ToInt32(RawBytes, 4); } }
        public int PlayerID { get { return System.BitConverter.ToInt32(RawBytes, 8); } }
        public const int DEFAULT_HEADER_SIZE = 3 * sizeof(int);

        public readonly byte[] RawBytes;

        protected Packet(byte[] bytes)
        {
            RawBytes = bytes;
        }
        
        public bool HasValidProtocolID()
        {
            if (RawBytes.Length < NET_PROTOCOL_ID.Length)
                return false;

            bool valid = true;

            for (ushort i = 0; i < NET_PROTOCOL_ID.Length && valid; i++)
            {
                if (RawBytes[i] != NET_PROTOCOL_ID[i])
                    valid = false;
            }

            return valid;
        }
    }
}
