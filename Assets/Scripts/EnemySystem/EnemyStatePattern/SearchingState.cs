using System.Collections;
using ubv.server.logic.ai;
using UnityEngine;

namespace Assets.Scripts.EnemySystem.EnemyStatePattern
{
    public class SearchingState : EnemyState
    {
        public EnemyState Update()
        {
            pathfindingMovement.MoveTo(Suspiciousposition);

            float reachedPositionDistance = 10f;
            if (Vector3.Distance(transform.position, roamPosition) < reachedPositionDistance)
            {
                // Reached Roam Position
                roamPosition = GetRoamingPosition();
            }

            return FindTarget();
        }

        private Vector3 GetRoamingPosition()
        {
            return startingPosition + Utils.GetRandomDir() * Random.Range(10f, 70f);
        }

        private EnemyState FindTarget()
        {
            float targetRange = 50f;
            if (Vector3.Distance(transform.position, player.position /*Player.Instance.GetPosition()*/) < targetRange)
            {
                // Player within target range
                return new ChasingTargetState();
            }

            return this();
        }
    }
}