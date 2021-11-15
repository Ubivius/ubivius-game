using System.Collections;
using ubv.common.data;
using ubv.common.serialization;
using UnityEngine;

namespace ubv.server.logic.ai
{
    abstract public class EnemyBehaviorState
    {
        // Use this for initialization
        public virtual EnemyBehaviorState Init()
        {
            return this;
        }
        
        // Update is called once per frame
        public virtual EnemyBehaviorState Update()
        {
            return this;
        }
    }
}
