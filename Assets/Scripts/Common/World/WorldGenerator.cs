using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ubv.common.world
{
    [RequireComponent(typeof(Grid))]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2Int m_boundariesMap;

        // Section0
        [SerializeField] private List<RoomInfo> m_randomRoomPoolSection0;
        [SerializeField] private int m_numberRandomRoomSection0;
        [SerializeField] private int m_numberofTrySection0;
        [SerializeField] private List<RoomInfo> m_mandatoryRoomPoolSection0;
        // TopLeft
        [SerializeField] private List<RoomInfo> m_randomRoomPoolTopLeft;
        [SerializeField] private int m_numberRandomRoomTopLeft;
        [SerializeField] private int m_numberofTryTopLeft;
        [SerializeField] private List<RoomInfo> m_mandatoryRoomPoolTopLeft;
        // TopRight
        [SerializeField] private List<RoomInfo> m_randomRoomPoolTopRight;
        [SerializeField] private int m_numberRandomRoomTopRight;
        [SerializeField] private int m_numberofTryTopRight;
        [SerializeField] private List<RoomInfo> m_mandatoryRoomPoolTopRight;
        // BottomLeft
        [SerializeField] private List<RoomInfo> m_randomRoomPoolBottomLeft;
        [SerializeField] private int m_numberRandomRoomBottomLeft;
        [SerializeField] private int m_numberofTryBottomLeft;
        [SerializeField] private List<RoomInfo> m_mandatoryRoomPoolBottomLeft;
        // BottomRight
        [SerializeField] private List<RoomInfo> m_randomRoomPoolBottomRight;
        [SerializeField] private int m_numberRandomRoomBottomRight;
        [SerializeField] private int m_numberofTryBottomRight;
        [SerializeField] private List<RoomInfo> m_mandatoryRoomPoolBottomRight;

        [SerializeField] private Tilemap m_floor;
        [SerializeField] private TileBase m_tileFloor;
        [SerializeField] private int m_wallThickness;

        private Grid m_grid;

        private ubv.common.world.LogicGrid m_masterLogicGrid;

        private ubv.common.world.RoomManager m_roomManager;
        private ubv.common.world.CorridorsManager m_corridorsManager;

        private dataStruct.WorldGeneratorToRoomManager m_worldGeneratorToRoomManager;
        private dataStruct.WorldGeneratorToCorridorsManager m_worldGeneratorToCorridorsManager;

        private void Awake()
        {
            m_grid = GetComponent<Grid>();

            m_worldGeneratorToRoomManager = new dataStruct.WorldGeneratorToRoomManager(
                m_boundariesMap,
                m_randomRoomPoolSection0,
                m_numberRandomRoomSection0,
                m_numberofTrySection0,
                m_mandatoryRoomPoolSection0,
                m_randomRoomPoolTopLeft,
                m_numberRandomRoomTopLeft,
                m_numberofTryTopLeft,
                m_mandatoryRoomPoolTopLeft,
                m_randomRoomPoolTopRight,
                m_numberRandomRoomTopRight,
                m_numberofTryTopRight,
                m_mandatoryRoomPoolTopRight,
                m_randomRoomPoolBottomLeft,
                m_numberRandomRoomBottomLeft,
                m_numberofTryBottomLeft,
                m_mandatoryRoomPoolBottomLeft,
                m_randomRoomPoolBottomRight,
                m_numberRandomRoomBottomRight,
                m_numberofTryBottomRight,
                m_mandatoryRoomPoolBottomRight,
                m_grid,
                m_wallThickness);

        public void GenerateWorld()
        {
            m_roomManager = new RoomManager(m_worldGeneratorToRoomManager);
            m_masterLogicGrid = m_roomManager.GenerateRoomGrid();

            m_worldGeneratorToCorridorsManager = new dataStruct.WorldGeneratorToCorridorsManager(m_masterLogicGrid, m_floor, m_tileFloor, m_wallThickness);

            m_corridorsManager = new CorridorsManager(m_worldGeneratorToCorridorsManager);
            m_masterLogicGrid = m_corridorsManager.GenerateCorridorsGrid();
        }
        
        public cellType.CellInfo[,] GetCellInfoArray()
        {
            return m_masterLogicGrid.GetCellInfo();
        }

    }
}

