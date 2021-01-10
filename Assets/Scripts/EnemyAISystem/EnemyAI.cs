using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    //Only for the moment, cause we'll have a class for the player
    public Transform player;

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
        state = State.Roaming;
    }

    private void Start()
    {
        startingPosition = transform.position;
        roamPosition = GetRoamingPosition();
    }

    private void Update()
    {
        switch (state)
        {
            case State.Roaming:
                //pathfindingMovement.MoveToTimer(roamPosition);

                float reachedPositionDistance = 10f;
                if (Vector3.Distance(transform.position, roamPosition) < reachedPositionDistance)
                {
                    // Reached Roam Position
                    roamPosition = GetRoamingPosition();
                }

                FindTarget();
                break;
            case State.ChaseTarget:
                pathfindingMovement.MoveToTimer(player.position/*Player.Instance.GetPosition()*/);

                //aimShootAnims.SetAimTarget(Player.Instance.GetPosition());

                float attackRange = 30f;
                if (Vector3.Distance(transform.position, player.position /*Player.Instance.GetPosition()*/) < attackRange)
                {
                    // Target within attack range
                    if (Time.time > nextShootTime)
                    {
                        pathfindingMovement.StopMoving();
                        state = State.ShootingTarget;
                        //aimShootAnims.ShootTarget(Player.Instance.GetPosition(), () => {
                        state = State.ChaseTarget;
                        //});
                        float fireRate = .15f;
                        nextShootTime = Time.time + fireRate;
                    }
                }

                float stopChaseDistance = 80f;
                if (Vector3.Distance(transform.position, player.position/*Player.Instance.GetPosition()*/) > stopChaseDistance)
                {
                    // Too far, stop chasing
                    state = State.GoingBackToStart;
                }
                break;
            case State.ShootingTarget:
                break;
            case State.GoingBackToStart:
                pathfindingMovement.MoveToTimer(startingPosition);

                reachedPositionDistance = 10f;
                if (Vector3.Distance(transform.position, startingPosition) < reachedPositionDistance)
                {
                    // Reached Start Position
                    state = State.Roaming;
                }
                break;
        }
    }

    private Vector3 GetRoamingPosition()
    {
        return startingPosition + Utils.GetRandomDir() * Random.Range(10f, 70f);
    }

    private void FindTarget()
    {
        float targetRange = 50f;
        if (Vector3.Distance(transform.position, player.position /*Player.Instance.GetPosition()*/) < targetRange)
        {
            // Player within target range
            state = State.ChaseTarget;
        }
    }

}