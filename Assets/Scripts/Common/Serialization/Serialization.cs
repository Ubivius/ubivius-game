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
            LOGIC_CELL_INTERACTABLE,
            LOGIC_GRID,
            INT32,
            FLOAT,
            UINT32,
            BOOL,
            VECTOR2,
            VECTOR2INT,
            QUATERNION,
            STRING,
            LIST_PLAYERSTATE,
            HASHMAP_INT_PLAYERSTATE,
            LIST_INPUTFRAME,
            LOGIC_CELL_INFO,
            ARRAY2D_CELLINFO,
            BYTEARRAY,
            BYTE,
            NULL
        }
    }
}
