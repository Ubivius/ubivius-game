using System.Collections;
using UnityEngine;

namespace Assets.EnemyAISystem.EnemyStatePattern
{
    public class EnemyContext : MonoBehaviour
    {
        private EnemyState m_enemyState;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetEnemyState(EnemyState enestateState)
        {
            m_enemyState = enestateState;
        }

        public EnemyState GetState()
        {
            return m_enemyState;
        }
    }
}