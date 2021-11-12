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
                public serialization.types.Vector2 GoalPosition;
                
                public serialization.types.Int32 GUID;

                public EnemyState() : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    GUID = new serialization.types.Int32(0);
                    GoalPosition = new serialization.types.Vector2(Vector2.zero);

                    InitSerializableMembers(Position, GUID, GoalPosition);
                }

                public EnemyState(int enemyID) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    GUID = new serialization.types.Int32(enemyID);
                    GoalPosition = new serialization.types.Vector2(Vector2.zero);

                    InitSerializableMembers(Position, GUID, GoalPosition);
                }

                public EnemyState(EnemyState enemy) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    GUID = new serialization.types.Int32(enemy.GUID.Value);
                    GoalPosition = new serialization.types.Vector2(Vector2.zero);

                    InitSerializableMembers(Position, GUID, GoalPosition);
                }

                protected override ID.BYTE_TYPE SerializationID()
                {
                    return ID.BYTE_TYPE.ENEMY_STATE_DATA;
                }

                public bool IsDifferent(EnemyState other, float tolerance = 0.1f)
                {
                    if (!other.GUID.Value.Equals(GUID.Value)) return true;

                    if ((Position.Value - other.Position.Value).sqrMagnitude > tolerance * tolerance)
                    {
                        return true;
                    }

                    if ((GoalPosition.Value - other.GoalPosition.Value).sqrMagnitude > tolerance * tolerance)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }
    }
}
