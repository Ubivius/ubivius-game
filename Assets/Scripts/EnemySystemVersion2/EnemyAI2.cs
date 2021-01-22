using System.Collections;
using UnityEngine;

namespace Assets.EnemySystemVersion2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    

    public class EnemyAI2 : MonoBehaviour
    {
        //Only for the moment, cause we'll have a class for the player
        public Transform player;
        public EnemyAISystem.EnemyStatePattern.EnemyStateMachine enemyStateMachine;

        private enum State
        {
            Roaming,
            ChaseTarget,
            ShootingTarget,
            GoingBackToStart,
        }

        //private IAimShootAnims aimShootAnims; To set to enemi to attack
        private EnemyPathFindingMovement pathfindingMovement;
        private Vector3 startingPosition;
        private Vector3 roamPosition;
        private float nextShootTime;
        private State state;

        private void Awake()
        {
            pathfindingMovement = GetComponent<EnemyPathFindingMovement>();
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