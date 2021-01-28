using Assets.EnemySystemVersion2;
using System.Collections;
using UnityEngine;

namespace Assets.EnemyAISystem.EnemyStatePattern
{
    public class EnemyStateMachine : EnemyAI2
    {
        private EnemyState m_enemyState;

        // Use this for initialization
        void Start()
        {
            startingPosition = transform.position;
            //SHould change to allow enemy to not necessarly start in this state 
            pathfindingMovement = GetComponent<EnemyPathFindingMovement>();
            //aimShootAnims = GetComponent<IAimShootAnims>();

            m_enemyState = new RoamingState();
        }

        // Update is called once per frame
        void Update()
        {
            m_enemyState = m_enemyState.Update();
        }
        public void SetEnemyState(EnemyState enestateState)
        {
            m_enemyState = enestateState;
        }

        public EnemyState GetState()
        {
            return m_enemyState;
        }
    }
}