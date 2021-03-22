using UnityEngine;
using System.Collections;
using ubv.common.serialization;

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
            public class PlayerState : serialization.Serializable
            {   
                // send this over network
                public serialization.types.Vector2 Position;
                public serialization.types.Float Rotation;

                // TODO : not send this via network because not likely to be changed
                public serialization.types.Int32 GUID;

                public PlayerState() : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Rotation = new serialization.types.Float(0);
                    GUID = new serialization.types.Int32(0);

                    InitSerializableMembers(Position, Rotation, GUID);
                }

                public PlayerState(int playerID) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Rotation = new serialization.types.Float(0);
                    GUID = new serialization.types.Int32(playerID);

                    InitSerializableMembers(Position, Rotation, GUID);
                }

                public PlayerState(PlayerState player) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Rotation = new serialization.types.Float(player.Rotation.Value);
                    GUID = new serialization.types.Int32(player.GUID.Value);

                    InitSerializableMembers(Position, Rotation, GUID);
                }
                
                
                protected override ID.BYTE_TYPE SerializationID()
                {
                    return ID.BYTE_TYPE.PLAYER_STATE;
                }

            }
        }
    }
}
