using System.Collections;
using UnityEngine;

namespace Assets.EnemyAISystem.EnemyStatePattern
{
    public class AttackingTargetState : EnemyState
    {

        // Use this for initialization
        public void Start()
        {

        }

        // Update is called once per frame
        public EnemyState Update()
        {
            if (Time.time > nextShootTime)
            {
                //pathfindingMovement.StopMoving();

                float fireRate = .15f;
                nextShootTime = Time.time + fireRate;

                //aimShootAnims.ShootTarget(Player.Instance.GetPosition(), () => {
                //});
            }

            if (Vector3.Distance(transform.position, player.position /*Player.Instance.GetPosition()*/) < attackRange)
            {
                // Target within attack range
                return new AttackingTargetState();
            }
            
            return new ChasingTargetState();
            
        }
    }
}