using System.Collections;
using UnityEngine;

namespace Assets.Scripts.EnemySystem.EnemyStatePattern
{
    public class EnemyMain : MonoBehaviour
    {

        public Vector3 m_startingPosition;
        //public Vector3 m_roamPosition;

        public EnemyPathFindingMovement EnemyPathFindingMovement { get; private set; }
        public Rigidbody2D EnemyRigidbody2D { get; private set; }
        //public HealthSystem HealthSystem { get; private set; }

        private void Awake()
        {
            EnemyPathFindingMovement = GetComponent<EnemyPathFindingMovement>();
            EnemyRigidbody2D = GetComponent<Rigidbody2D>();
            //HealthSystem = new HealthSystem(100);
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}