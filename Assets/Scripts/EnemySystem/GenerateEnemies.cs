using System.Collections;
using UnityEngine;

namespace Assets.Scripts.EnemySystem
{
    public class GenerateEnemies : MonoBehaviour
    {
        [SerializeField] private GameObject m_enemy;
        [SerializeField] private int m_xPos;
        [SerializeField] private int m_yPos;
        [SerializeField]  private int m_enemyCount;

        // Use this for initialization
        void Start()
        {
            StartCoroutine(EnemySpawn());
        }

        IEnumerator EnemySpawn()
        {
            for(int i=0; i<m_enemyCount; i++)
            {
                m_xPos = Random.Range(1, 50);
                m_yPos = Random.Range(1, 31);
                Instantiate(m_enemy, new Vector3(m_xPos, m_yPos, 0), Quaternion.identity);

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}