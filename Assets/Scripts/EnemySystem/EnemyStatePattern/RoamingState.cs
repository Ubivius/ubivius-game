using Assets.Scripts.EnemySystem.EnemyStatePattern;
using System.Collections;
using ubv.server.logic.ai;
using UnityEngine;

namespace Assets.EnemyAISystem.EnemyStatePattern
{
    public class RoamingState : EnemyState
    {
        private Vector3 m_roamPosition;
        private Vector3 m_startingPosition;

        private float m_targetRange = 50f;
        private float m_reachedPositionDistance = 10f;

        private EnemyMain m_enemyMain;
        public EnemyPathFindingMovement EnemyPathFindingMovement { get; private set; }

        public RoamingState(EnemyMain enemyMain)
        {
            m_enemyMain = enemyMain;

            m_startingPosition = m_enemyMain.GetPosition();
            m_roamPosition = GetRoamingPosition();
        }

        public EnemyState Update()
        {
            m_enemyMain.EnemyPathFindingMovement.MoveToTimer(m_roamPosition);

            if (Vector3.Distance(m_enemyMain.GetPosition(), m_roamPosition) < m_reachedPositionDistance)
            {
                // Reached Roam Position
                m_roamPosition = GetRoamingPosition();
            }

            return FindTarget();
        }

        private Vector3 GetRoamingPosition()
        {
            return m_startingPosition + GetRandomDir() * Random.Range(10f, 70f);
        }

        private EnemyState FindTarget()
        {
            //if (Vector3.Distance(m_enemyMain.GetPosition(), player.position /*Player.Instance.GetPosition()*/) < m_targetRange)
            {
                // Player within target range
                //return new ChasingTargetState();
            }

            return new RoamingState(m_enemyMain);
        }

        // Generate random normalized direction
        public static Vector3 GetRandomDir()
        {
            return new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
        }
    }

}