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
            IDENTIFICATION_MESSAGE,
            SERVER_INIT_MESSAGE,
            LOGIC_CELL,
            LOGIC_CELL_DOOR,
            LOGIC_CELL_FLOOR,
            LOGIC_CELL_WALL,
            LOGIC_CELL_INTERACTABLE,
            LOGIC_CELL_PLAYERSPAWN,
            LOGIC_GRID,
            INT32,
            FLOAT,
            UINT32,
            BOOL,
            VECTOR2,
            VECTOR2INT,
            QUATERNION,
            STRING,
            LIST_STRING,
            HASHMAP_INT_PLAYERSTATE,
            LIST_INPUTFRAME,
            LOGIC_CELL_INFO,
            ARRAY2D_CELLINFO,
            BYTEARRAY,
            BYTE,
            NULL,
            CLIENT_READY_MESSAGE,
            LOGIC_CELL_VOID,
            CHARACTER_LIST_MESSAGE,
            SERVER_STARTS_MESSAGE,
            CLIENT_WORLD_LOADED_MESSAGE,
            SERVER_SUCCESSFUL_TCP_CONNECT_MESSAGE,
            LOBBY_ENTER_MESSAGE,
            HASHMAP_INT_STRING,
            NET_INFO,
            CLIENT_STATE_MESSAGE,
            INT64,
            RTT_MSG,
            BITSET,
            SERVER_SUCCESSFUL_UDP_CONNECT_MESSAGE
        }
    }
}
