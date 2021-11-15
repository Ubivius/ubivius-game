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
        private EnemyMovementUpdater m_movement;

        private void Awake()
        {
            m_movement = GetComponent<EnemyMovementUpdater>();
        }

        // Use this for initialization
        void Start()
        {
            CurrentEnemyState = new RoamingState(m_movement);
        }

        // Update is called once per frame
        void Update()
        {
            CurrentEnemyState = CurrentEnemyState.Update();
        }
    }
}
