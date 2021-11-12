﻿using UnityEngine;
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
            ///
            
            public class EnemyStateData : serialization.Serializable
            {
                // send this over network
                public serialization.types.Vector2 Position;
                public serialization.types.Vector2 GoalPosition;
                public serialization.types.Float Rotation;

                public serialization.types.List<serialization.types.Vector2> GoalPositions;

                public EnemyStateInfo EnemyStateInfo;
                public EnemyState EnemyState;

                // TODO : not send this via network because not likely to be changed
                public serialization.types.Int32 GUID;

                public EnemyStateData() : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Rotation = new serialization.types.Float(0);
                    GUID = new serialization.types.Int32(0);
                    GoalPosition = new serialization.types.Vector2(Vector2.zero);

                    InitSerializableMembers(Position, Rotation, GUID, GoalPosition);
                }

                public EnemyStateData(int enemyID) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Rotation = new serialization.types.Float(0);
                    GUID = new serialization.types.Int32(enemyID);
                    GoalPosition = new serialization.types.Vector2(Vector2.zero);

                    InitSerializableMembers(Position, Rotation, GUID, GoalPosition);
                }

                public EnemyStateData(EnemyStateData enemy) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Rotation = new serialization.types.Float(enemy.Rotation.Value);
                    GUID = new serialization.types.Int32(enemy.GUID.Value);
                    GoalPosition = new serialization.types.Vector2(Vector2.zero);

                    InitSerializableMembers(Position, Rotation, GUID, GoalPosition);
                }

                protected override ID.BYTE_TYPE SerializationID()
                {
                    return ID.BYTE_TYPE.ENEMY_STATE_DATA;
                }
            }
        }
    }
}
