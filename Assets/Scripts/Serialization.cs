using UnityEngine;
using UnityEditor;

namespace ubv
{
    public static class Serialization
    {
        public enum BYTE_TYPE : byte
        {
            INPUT_FRAME = 0x00,
            CLIENT_STATE = 0x01
        }
    }
}