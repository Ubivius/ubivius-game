using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.common.world
{
    public struct BoxInfo
    {
        Vector2Int m_coord;
        Vector3Int m_info;

        public BoxInfo(int x, int y, int width, int height) : this()
        {
            m_coord.x = x;
            m_coord.y = y;
            m_info.x = width;
            m_info.y = height;
            m_info.z = width * height;
        }
        public Vector2Int GetOrigin()
        {
            return m_coord;
        }
        public int GetWidth() 
        { 
            return m_info.x; 
        }
        public int GetHeight()
        {
            return m_info.y;
        }
        public int GetArea()
        {
            return m_info.z;
        }
    }

    [RequireComponent(typeof(Grid))]

    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2Int m_boundariesMap;

        [SerializeField] private int m_numberRandomRoom;

        [SerializeField] private List<GameObject> m_roomPoolRandom;
        [SerializeField] private List<GameObject> m_roomPoolSection0;
        [SerializeField] private List<GameObject> m_roomPoolSection1;

        private Grid m_grid;

        private ubv.common.world.LogicGrid m_MasterLogicGrid;

        private void Awake()
        {
            m_grid = GetComponent<Grid>();

            m_MasterLogicGrid = new ubv.common.world.LogicGrid(m_boundariesMap.x, m_boundariesMap.y);

            foreach (GameObject room in m_roomPoolSection0)
            {
                Vector2Int roomOrigin = GetCoordInSection0();
                Instantiate(room, new Vector3(roomOrigin.x, roomOrigin.y, 0), Quaternion.identity, m_grid.transform);
            }
            
        }

        private Vector2Int GetCoordInSection0()
        {
            BoxInfo box = new BoxInfo(m_boundariesMap.x / 3, m_boundariesMap.y / 3, m_boundariesMap.x / 3, m_boundariesMap.y / 3);
            return GetRandomBoxCoord(box);
        }

        private Vector2Int GetCoordInTopLeft()
        {
            BoxInfo bigBox = new BoxInfo(0, m_boundariesMap.y / 2, m_boundariesMap.x / 3, m_boundariesMap.y / 2);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 3, m_boundariesMap.y / 3, m_boundariesMap.x / 6, m_boundariesMap.y / 3);
            return GetCoordInSection(bigBox, smallBox);
        }
        private Vector2Int GetCoordInTopRight()
        {
            BoxInfo bigBox = new BoxInfo(m_boundariesMap.x * 2 / 3, m_boundariesMap.y / 2, m_boundariesMap.x / 3, m_boundariesMap.y / 2);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 2, m_boundariesMap.y * 2 / 3, m_boundariesMap.x / 6, m_boundariesMap.y / 3);
            return GetCoordInSection(bigBox, smallBox);
        }
        private Vector2Int GetCoordInBottomLeft()
        {
            BoxInfo bigBox = new BoxInfo(0, 0, m_boundariesMap.x / 3, m_boundariesMap.y / 2);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 3, 0, m_boundariesMap.x / 6, m_boundariesMap.y / 3);
            return GetCoordInSection(bigBox, smallBox);
        }
        private Vector2Int GetCoordInBottomRight()
        {
            BoxInfo bigBox = new BoxInfo(m_boundariesMap.x * 2 / 3, 0, m_boundariesMap.x / 3, m_boundariesMap.y / 2);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 2, 0, m_boundariesMap.x / 6, m_boundariesMap.y / 3);
            return GetCoordInSection(bigBox, smallBox);
        }

        private Vector2Int GetCoordInSection(BoxInfo bigBox, BoxInfo smallBox)
        {

            if (Random.Range(0,(bigBox.GetArea() + smallBox.GetArea())) < bigBox.GetArea())
            {
                return GetRandomBoxCoord(bigBox);
            }
            else
            {
                return GetRandomBoxCoord(smallBox);
            }

        }

        private Vector2Int GetRandomBoxCoord(BoxInfo box)
        {
            return new Vector2Int(
                    Random.Range(box.GetOrigin().x, box.GetOrigin().x + box.GetWidth()),
                    Random.Range(box.GetOrigin().y, box.GetOrigin().y + box.GetHeight())
                    );
        }


    }
}

