using ubv.common.world;
using UnityEngine;

/// <summary>
/// In charge of updating the state of the enemy
/// </summary>

namespace ubv.server.logic.ai
{
    public class EnemyStateMachine : MonoBehaviour
    {
        private EnemyBehaviorState m_currentEnemyState;
        //public Transform player;
        private EnemyMovementUpdater m_movement;

        static public bool IsActive = false;

        private void Awake()
        {
            m_movement = GetComponent<EnemyMovementUpdater>();
        }
        
        public void Init(PlayerMovementUpdater playerMovement, WorldGenerator world, PathfindingGridManager pathfinding)
        {
            m_currentEnemyState = new RoamingState(world.GetEnemySpawnPosition(), m_movement, playerMovement, pathfinding);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (IsActive)
            {
                m_currentEnemyState = m_currentEnemyState?.Update();
            }
        }
    }
}
