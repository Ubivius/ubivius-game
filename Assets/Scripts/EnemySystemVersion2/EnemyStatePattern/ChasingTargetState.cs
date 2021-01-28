using System.Collections;
using UnityEngine;

namespace Assets.EnemyAISystem.EnemyStatePattern
{
    public class ChasingTargetState : EnemyState
    {

        // Use this for initialization
        public void Start()
        {

        }

        // Update is called once per frame
        public EnemyState Update()
        {
            pathfindingMovement.MoveToTimer(player.position/*Player.Instance.GetPosition()*/);

            //aimShootAnims.SetAimTarget(Player.Instance.GetPosition());

            if (Vector3.Distance(transform.position, player.position /*Player.Instance.GetPosition()*/) < attackRange)
            {
                // Target within attack range
                return new AttackingTargetState();
            }

            float stopChaseDistance = 80f;
            if (Vector3.Distance(transform.position, player.position/*Player.Instance.GetPosition()*/) > stopChaseDistance)
            {
                // Too far, stop chasing
                return new GoingBackToStartState();
            }

            return new ChasingTargetState();
        }
    }
}