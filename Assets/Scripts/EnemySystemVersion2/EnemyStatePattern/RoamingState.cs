using System.Collections;
using UnityEngine;

namespace Assets.EnemyAISystem.EnemyStatePattern
{
    public class RoamingState : EnemyState
    {

        // Use this for initialization
        public void Start()
        {

        }

        // Update is called once per frame
        public EnemyState Update()
        {
            //pathfindingMovement.MoveToTimer(roamPosition);

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

            return new RoamingState();
        }
    }

}