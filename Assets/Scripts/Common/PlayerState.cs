using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            /// <summary>
            /// Class reprensenting individual player state 
            /// Add here the data of a single player
            /// </summary>
            public class PlayerState : udp.Serializable
            {
                public udp.SerializableTypes.Vector2 Position;
                public udp.SerializableTypes.Quaternion Rotation;
                public udp.SerializableTypes.Int32 GUID;

                public PlayerState() : base() { }

                public PlayerState(PlayerState player) : base()
                {
                    Position.Set(player.Position);
                    Rotation.Set(player.Rotation);
                    GUID.Set(player.GUID);
                }

                protected override void InitSerializableMembers()
                {
                    Position = new udp.SerializableTypes.Vector2(this, Vector2.zero);
                    Rotation = new udp.SerializableTypes.Quaternion(this, Quaternion.identity);
                    GUID = new udp.SerializableTypes.Int32(this, -1);
                }

                protected override byte SerializationID()
                {
                    return (byte)udp.Serialization.BYTE_TYPE.PLAYER_STATE;
                }

            }
        }
    }
}