using System.Collections;
using ubv.common.data;
using ubv.common.serialization;
using UnityEngine;

namespace ubv.server.logic.ai
{
    abstract public class EnemyBehaviorState
    {
        protected PathfindingGridManager m_pathfinding;
        protected EnemyMovementUpdater m_enemyMovement;
        protected PlayerMovementUpdater m_playerMovement;

        protected EnemyBehaviorState(EnemyMovementUpdater enemyMovement, 
            PlayerMovementUpdater playerMovement, PathfindingGridManager pathfinding)
        {
            m_pathfinding = pathfinding;
            m_enemyMovement = enemyMovement;
            m_playerMovement = playerMovement;
            m_enemyMovement.SetPathfinding(m_pathfinding);
        }

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
