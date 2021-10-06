using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using ubv.server.logic.ai;

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
            
            // add enemystateinfo comme cellinfo
            public class EnemyStateData : serialization.Serializable
            {
                // send this over network
                public serialization.types.Vector2 Position;
                public serialization.types.Float Rotation;
                public serialization.types.Int32 NbOfEnemy;
                //public ubv.server.logic.ai.EnemyState EnemyState;

                public EnemyStateInfo EnemyStateInfo;
                public EnemyState EnemyState;

                /*private serialization.types.Byte m_enemyState;
                private serialization.types.Int32 m_enemyStateID;
                private serialization.types.ByteArray m_enemyStateBytes;*/
                /*public enum EnemyStateType
                {
                    ROAMING,
                    SEARCHING,
                    CHASING,
                    FIGHTING
                }*/

                // TODO : not send this via network because not likely to be changed
                public serialization.types.Int32 GUID;

                public EnemyStateData() : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Rotation = new serialization.types.Float(0);
                    GUID = new serialization.types.Int32(0);
                    EnemyStateInfo = new EnemyStateInfo();

                    InitSerializableMembers(Position, Rotation, GUID);
                }

                public EnemyStateData(int enemyID) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Rotation = new serialization.types.Float(0);
                    GUID = new serialization.types.Int32(enemyID);

                    InitSerializableMembers(Position, Rotation, GUID);
                }

                public EnemyStateData(EnemyStateData enemy) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Rotation = new serialization.types.Float(enemy.Rotation.Value);
                    GUID = new serialization.types.Int32(enemy.GUID.Value);

                    InitSerializableMembers(Position, Rotation, GUID);
                }

                /*public EnemyState EnemyStateFromBytes()
                {
                    EnemyState enemyState = null;
                    switch ((EnemyStateType)m_enemyState.Value)
                    {
                        case EnemyStateType.ROAMING:
                            enemyState = CreateFromBytes<RoamingState>(m_enemyStateBytes.Value.ArraySegment());
                            break;
                        case EnemyStateType.SEARCHING:
                            enemyState = CreateFromBytes<SearchingState>(m_enemyStateBytes.Value.ArraySegment());
                            break;
                        case EnemyStateType.CHASING:
                            enemyState = CreateFromBytes<ChasingState>(m_enemyStateBytes.Value.ArraySegment());
                            break;
                        case EnemyStateType.FIGHTING:
                            enemyState = CreateFromBytes<FightingState>(m_enemyStateBytes.Value.ArraySegment());
                            break;
                        default:
                            break;
                    }

                    return enemyState;
                }*/

                protected override ID.BYTE_TYPE SerializationID()
                {
                    return ID.BYTE_TYPE.ENEMY_STATE_DATA;
                }

            }
        }
    }
}
