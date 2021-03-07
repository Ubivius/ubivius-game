using Assets.EnemyAISystem.EnemyStatePattern;
using Assets.Scripts.EnemySystem.EnemyStatePattern;
using UnityEngine;

/// <summary>
/// In charge of updating the state of the enemy
/// </summary>

namespace ubv.server.logic.ai
{
    public class EnemyStateMachine : MonoBehaviour
    {
        public EnemyMain EnemyMain { get; private set; }

        private EnemyState m_currentEnemyState;
        

        // Use this for initialization
        void Start()
        {
            EnemyMain = GetComponent<EnemyMain>();
            m_currentEnemyState = new RoamingState(EnemyMain enemyMain);
        }

        // Update is called once per frame
        void Update()
        {
            m_currentEnemyState = m_currentEnemyState.Update();
        }
    }
}