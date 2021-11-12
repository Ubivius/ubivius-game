using System.Collections;
using UnityEngine;
using ubv.common.serialization;

namespace ubv.server.logic.ai
{
    public class ChasingState : EnemyState
    {
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CHASING_STATE;
        }

        public override EnemyStateInfo.EnemyStateType GetEnemyStateType()
        {
            return EnemyStateInfo.EnemyStateType.CHASING;
        }
    }
}
