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
            private static readonly byte[] TCP_PROTOCOL_ID = { 0xAA, 0x0C, 0xC0, 0xFF };
            public const ushort TCP_MAX_PAYLOAD_SIZE = 512 * 2 * 2;
            public const ushort TCP_HEADER_SIZE = 5 * sizeof(int);
            public const ushort TCP_PACKET_SIZE = TCP_HEADER_SIZE + TCP_MAX_PAYLOAD_SIZE; // size in bytes
        }

    }
}
