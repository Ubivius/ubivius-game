using UnityEngine;
using System.Collections;

namespace ubv.network
{
    public class Packet
    {
        protected static readonly byte[] NET_PROTOCOL_ID = { 0xAA, 0x0C, 0xC0, 0xFF };

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
