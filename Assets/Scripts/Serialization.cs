using UnityEngine;
using UnityEditor;

namespace ubv
{
    public static class Serialization
    {
        public enum BYTE_TYPE : byte
        {
            INPUT_FRAME =   0x00,
            INPUT_MESSAGE = 0x01,
            CLIENT_STATE =  0x02,
        }
    }
}