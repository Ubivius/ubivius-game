using System.Collections;
using UnityEngine;

namespace ubv.server.logic.ai
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

            if (Vector3.SqrMagnitude(transform.position - roamPosition) < Vector3.SqrMagnitude(reachedPositionDistance))
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
            if (Vector3.SqrMagnitude(transform.position, player.position) < Vector3.SqrMagnitude(targetRange))
            {
                // Player within target range
                return new ChasingTargetState();
            }

            return this;
        }
    }

}