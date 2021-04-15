using UnityEditor;
using UnityEngine;
using ubv.common.serialization;

namespace ubv.server.logic.ai
{
    public class SearchingState : EnemyState
    {
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SEARCHING_STATE;
        }

        public override EnemyStateInfo.EnemyStateType GetEnemyStateType()
        {
            return EnemyStateInfo.EnemyStateType.SEARCHING;
        }
    }
}
