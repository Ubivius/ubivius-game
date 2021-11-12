using UnityEngine;

/// <summary>
/// In charge of updating the state of the enemy
/// </summary>

namespace ubv.server.logic.ai
{
    public class EnemyStateMachine : MonoBehaviour
    {
        public EnemyBehaviorState CurrentEnemyState;
        //public Transform player;
        private EnemyMovementUpdater m_pathfindingMovement;

        private void Awake()
        {
            m_pathfindingMovement = GetComponent<EnemyMovementUpdater>();
            CurrentEnemyState = new RoamingState(m_pathfindingMovement);
        }

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            CurrentEnemyState = CurrentEnemyState.Update();
        }
    }
}
