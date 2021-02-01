using System.Collections;
using UnityEngine;

namespace Assets.EnemySystemVersion2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    

    public class EnemyAI : MonoBehaviour
    {
        //Only for the moment, cause we'll have a class for the player
        public Transform player;
        public EnemyAISystem.EnemyStatePattern.EnemyStateMachine enemyStateMachine;

        //private IAimShootAnims aimShootAnims; To set to enemi to attack
        // NOTE: would e nice to put some of these variable in editor
        //public EnemyPathFindingMovement pathfindingMovement;
        public Vector3 startingPosition;
        public Vector3 roamPosition;
        public float attackRange = 30f;
        public float reachedPositionDistance = 10f;
        public float nextShootTime;

        private void Awake()
        {
            //pathfindingMovement = GetComponent<EnemyPathFindingMovement>();
            //aimShootAnims = GetComponent<IAimShootAnims>();
            enemyStateMachine = new EnemyAISystem.EnemyStatePattern.EnemyStateMachine();
        }

        private void Start()
        {
        }

        private void Update()
        {
        }
    }
}