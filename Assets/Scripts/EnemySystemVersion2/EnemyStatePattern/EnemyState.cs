using System.Collections;
using UnityEngine;

namespace Assets.EnemyAISystem.EnemyStatePattern
{
    public class EnemyState : EnemyStateMachine
    {
        public EnemyPathFindingMovement pathfindingMovement;
        public Vector3 startingPosition;
        public Vector3 roamPosition;
        public float nextShootTime;

        // Use this for initialization
        public void Start()
        {
            
        }

        // Update is called once per frame
        public EnemyState Update()
        {
            return new EnemyState();
        }
    }
}