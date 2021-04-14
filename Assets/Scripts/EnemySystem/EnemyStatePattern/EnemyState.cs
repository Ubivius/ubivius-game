using System.Collections;
using ubv.common.data;
using ubv.common.serialization;
using UnityEngine;

namespace ubv.server.logic.ai
{
    abstract public class EnemyState: common.serialization.Serializable
    {
        private EnemyStateData m_eneyStateData;

        // Use this for initialization
        public virtual EnemyState Init()
        {
            return this;
        }

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
