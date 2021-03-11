using UnityEngine;

/// <summary>
/// In charge of updating the state of the enemy
/// </summary>

namespace ubv.server.logic.ai
{
    public class EnemyStateMachine : MonoBehaviour
    {
        private EnemyState m_currentEnemyState;

        // Use this for initialization
        void Start()
        {
            //m_currentEnemyState = new RoamingState()
        }

        // Update is called once per frame
        void Update()
        {
            //m_currentEnemyState = m_currentEnemyState.Update();
        }
    }
}