using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.common.world
{
    [RequireComponent(typeof(Grid))]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2Int m_boundariesMap;

        [SerializeField] private int m_numberRandomRoom;

        [SerializeField] private List<GameObject> m_roomPoolRandom;
        [SerializeField] private List<GameObject> m_roomPoolSection0;
        [SerializeField] private List<GameObject> m_roomPoolSection1;

        private Grid m_grid;
        private Vector2Int m_section0Boundaries;
        private Vector2 m_section0Origin;

        private ubv.common.world.LogicGrid m_MasterLogicGrid;

        private void Awake()
        {
            m_section0Boundaries = new Vector2Int(m_boundariesMap.x / 3, m_boundariesMap.y / 3);
            m_section0Origin = new Vector2Int(m_section0Boundaries.x, m_section0Boundaries.y);

            m_grid = GetComponent<Grid>();

            m_MasterLogicGrid = new ubv.common.world.LogicGrid();

            foreach (GameObject room in m_roomPoolSection0)
            {

                Instantiate(room, new Vector3(Random.Range(m_section0Origin.x, m_section0Boundaries.x), Random.Range(m_section0Origin.y, m_section0Boundaries.y), 0), Quaternion.identity, m_grid.transform);

            }
            
        }
        
        private Vector2Int GetTopLeftSectionCoord()
        {

        }


    }
}

