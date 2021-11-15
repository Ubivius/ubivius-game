using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using ubv.server.logic.ai;
using System.Collections.Generic;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            /// <summary>
            /// Class reprensenting individual enemy state 
            /// Add here the data of a single enemy
            /// </summary>
            public class EnemyState : Serializable
            {
                // send this over network
                public serialization.types.Vector2 Position;
                
                public serialization.types.Int32 GUID;

                public EnemyState() : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    GUID = new serialization.types.Int32(0);

                    InitSerializableMembers(Position, GUID);
                }

                public EnemyState(int enemyID) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    GUID = new serialization.types.Int32(enemyID);

                    InitSerializableMembers(Position, GUID);
                }

                public EnemyState(EnemyState enemy) : base()
                {
                    Position = new serialization.types.Vector2(enemy.Position.Value);
                    GUID = new serialization.types.Int32(enemy.GUID.Value);

                    InitSerializableMembers(Position, GUID);
                }

                protected override ID.BYTE_TYPE SerializationID()
                {
                    return ID.BYTE_TYPE.ENEMY_STATE_DATA;
                }

                public bool IsDifferent(EnemyState other, float tolerance = 0.1f)
                {
                    if (other == this) return false;

                    if ((Position.Value - other.Position.Value).sqrMagnitude > tolerance * tolerance)
                    {
                        Debug.Log("Self pos " + Position.Value + " vs other pos " + other.Position.Value);
                        return true;
                    }

                    return false;
                }
            }
        }
    }
}
