using System.Collections;
using ubv.server.logic.ai;
using UnityEngine;

namespace Assets.Scripts.EnemySystem.EnemyStatePattern
{
    public class ChasingTargetState : EnemyState
    {
        private float m_attackRange = 30f;
        private float m_stopChaseDistance = 80f;

        // Update is called once per frame
        public EnemyState Update()
        {
            pathfindingMovement.MoveTo(player.position/*Player.Instance.GetPosition*/);
            //aimShootAnims.SetAimTarget(Player.Instance.GetPosition());

            if (Vector3.SqrMagnitude(transform.position - player.position /*Player.Instance.GetPosition()*/) < m_attackRange)
            {
                // Target within attack range
                //return new FightingState();
            }

            if (Vector3.SqrMagnitude(transform.position - player.position/*Player.Instance.GetPosition()*/) > m_stopChaseDistance * m_stopChaseDistance)
            {
                // Too far, stop chasing
                //return new SearchingState();
            }

            return this;
        }
    }
}