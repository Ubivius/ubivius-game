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
            ENEMY_STATE_DATA,
            IDENTIFICATION_MESSAGE,
            SERVER_INIT_MESSAGE,
            LOGIC_CELL,
            LOGIC_CELL_DOOR,
            LOGIC_CELL_FLOOR,
            LOGIC_CELL_WALL,
            LOGIC_CELL_INTERACTABLE,
            LOGIC_GRID,
            ENEMY_STATE,
            ROAMING_STATE,
            SEARCHING_STATE,
            CHASING_STATE,
            FIGHTING_STATE,
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
            HASHMAP_INT_ENEMYSTATEDATA,
            LIST_INPUTFRAME,
            LOGIC_CELL_INFO,
            ARRAY2D_CELLINFO,
            ENEMY_STATE_INFO,
            BYTEARRAY,
            BYTE,
            NULL,
            CLIENT_READY_MESSAGE,
            LOGIC_CELL_VOID,
            CLIENT_LIST_MESSAGE,
            SERVER_STARTS_MESSAGE,
            CLIENT_WORLD_LOADED_MESSAGE,
            SERVER_SUCCESSFUL_CONNECT_MESSAGE,
        }
    }
}
