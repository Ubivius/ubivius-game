using System.Collections;
using ubv.server.logic.ai;
using UnityEngine;

namespace Assets.Scripts.EnemySystem.EnemyStatePattern
{
    public class FightingState : EnemyState
    {
        private float m_attackRange = 80f; //available in CHASING STATE
        private float m_fireRate = .15f;
        private float m_nextShootTime;

        // Update is called once per frame
        public EnemyState Update()
        {
            if (Time.time > m_nextShootTime)
            {
                pathfindingMovement.StopMoving();
                m_nextShootTime = Time.time + m_fireRate;

                //aimShootAnims.ShootTarget(Player.Instance.GetPosition(), () => {
                //});
            }

            if (Vector3.SqrMagnitude(transform.position - player.position /*Player.Instance.GetPosition()*/) < attackRange)
            {
                // Target within attack range
                return this;
            }

            return new ChasingTargetState();
        }
    }
}