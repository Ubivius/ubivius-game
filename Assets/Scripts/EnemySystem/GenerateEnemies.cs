using System.Collections;
using ubv.common.world;
using UnityEngine;

namespace Assets.Scripts.EnemySystem
{
    // MOCK CHANGE WILL BE MADE TO FIT THE PROPER REQUIREMENTS
    public class GenerateEnemies : MonoBehaviour
    {
        [SerializeField] private GameObject m_enemy;
        [SerializeField] private int m_xPos;
        [SerializeField] private int m_yPos;
        [SerializeField]  private int m_enemyCount;
        [SerializeField] private PathfindingGridManager m_pathfindingGridManager;

        private PathNode[,] m_pathNodes;
        private LogicGrid m_logicGrid;

        // Use this for initialization
        void Start()
        {
            m_pathNodes = m_pathfindingGridManager.GetPathNodeArray();
            m_logicGrid = m_pathfindingGridManager.GetLogicGrid();
            StartCoroutine(EnemySpawn());
        }

        IEnumerator EnemySpawn()
        {
            // Wait until manager ready(Use event instead???)
            while(m_pathfindingGridManager.IsSetUpDone() == false)
            {

            }

            int i = 0;
            while (i < m_enemyCount)
            {
                m_xPos = Random.Range(0, m_pathNodes.GetLength(0)-1);
                m_yPos = Random.Range(0, m_pathNodes.GetLength(1)-1);

                if (m_pathNodes[m_xPos, m_yPos] != null)
                {
                    Instantiate(m_enemy, new Vector3(m_xPos, m_yPos, 0), Quaternion.identity);
                    yield return new WaitForSeconds(0.1f);
                    i++;
                }
            }
        }
    }
}