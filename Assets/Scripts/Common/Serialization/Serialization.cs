using UnityEngine;
using UnityEditor;

namespace ubv.common.serialization
{
    public static class ID
    {
        public enum BYTE_TYPE : byte
        {
            INPUT_FRAME = 0x00,
            INPUT_MESSAGE = 0x01,
            CLIENT_STATE = 0x02,
            PLAYER_STATE, // 0x03...
            AUTH_MESSAGE,
            START_MESSAGE,
            LOGIC_CELL,
            LOGIC_CELL_DOOR,
            LOGIC_CELL_FLOOR,
            LOGIC_CELL_WALL,
            LOGIC_CELL_INTERACTABLE // 0xA0, ou 10
        }
    }
}
