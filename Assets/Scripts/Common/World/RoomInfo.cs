using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ubv.common.world
{
    public class RoomInfo : MonoBehaviour
    {
        [SerializeField] private Tilemap m_floor;
        [SerializeField] private Tilemap m_wall;
        [SerializeField] private Tilemap m_door;
        [SerializeField] private Tilemap m_interactable;

        [SerializeField] private int m_height;
        [SerializeField] private int m_width;
        private Vector3Int m_roomOrigin;

        private LogicGrid m_logicGrid;

        private void Awake()
        {
#if DEBUG
            Debug.Assert(m_floor != null);
#endif // DEBUG

            m_logicGrid = new LogicGrid(m_width, m_height);

            DoorManagement();
            InteractableManagement();
            WallManagement();
            FloorManagement();
        }
        private void FloorManagement()
        {
            m_floor.CompressBounds();
            m_width = m_floor.cellBounds.size.x;
            m_height = m_floor.cellBounds.size.y;
            m_roomOrigin = m_floor.origin;


        }
        private void WallManagement()
        {
            if (m_wall)
            {

            }
        }
        private void DoorManagement()
        {
            if (m_door)
            {

            }
        }
        private void InteractableManagement()
        {
            if (m_interactable)
            {

            }
        }

        Vector2Int GetDimension()
        {
            return new Vector2Int(m_width, m_height);
        }

    }

}
