using System.Collections;
using ubv.common.data;
using ubv.common.serialization;
using UnityEngine;

namespace ubv.server.logic.ai
{

    public class EnemyStateInfo : common.serialization.Serializable
    {
        public enum EnemyStateType
        {
            ROAMING,
            SEARCHING,
            CHASING,
            FIGHTING,
            NONE_STATE
        }

        private common.serialization.types.Byte m_enemyState;
        private common.serialization.types.ByteArray m_enemyStateBytes;


        public EnemyStateInfo() : base()
        {
            m_enemyState = new common.serialization.types.Byte((byte)EnemyStateType.NONE_STATE);
            m_enemyStateBytes = new common.serialization.types.ByteArray(new byte[0]);

            InitSerializableMembers(m_enemyState, m_enemyStateBytes);
        }

        public EnemyState EnemyStateFromBytes()
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
                case EnemyStateType.NONE_STATE:
                default:
                    break;
            }

            return enemyState;
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.ENEMY_STATE_INFO;
        }
    }

    abstract public class EnemyState: common.serialization.Serializable
    {
        private EnemyStateData m_eneyStateData;

        // Use this for initialization
        public virtual EnemyState Init()
        {
            return this;
        }

        public abstract EnemyStateInfo.EnemyStateType GetEnemyStateType();

        // Update is called once per frame
        public virtual EnemyState Update()
        {
            return this;
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.ENEMY_STATE;
        }
    }
}
