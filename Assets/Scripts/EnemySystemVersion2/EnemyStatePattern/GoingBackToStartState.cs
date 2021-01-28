using System.Collections;
using UnityEngine;

namespace Assets.EnemyAISystem.EnemyStatePattern
{
    public class GoingBackToStartState : EnemyState
    {

        // Use this for initialization
        public void Start()
        {

        }

        // Update is called once per frame
        public EnemyState Update()
        {
            //pathfindingMovement.MoveToTimer(startingPosition);

            reachedPositionDistance = 10f;
            if (Vector3.Distance(transform.position, startingPosition) < reachedPositionDistance)
            {
                // Reached Start Position
                return new RoamingState();
            }

            return new GoingBackToStartState();
        }
    }
}