using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ubv.common.world
{
    class WallManager
    {
        private LogicGrid m_masterLogicGrid;
        private Tilemap m_wall;
        private TileBase m_tileWall;

        public WallManager(dataStruct.WolrdGeneratorToWallManager data)
        {
            m_masterLogicGrid = data.masterLogicGrid;
            m_wall = data.wall;
            m_tileWall = data.tilewall;
        }

        public LogicGrid GenerateWallGrid()
        {
            for (int x = 0; x < m_masterLogicGrid.Width; x++)
            {
                for (int y = 0; y < m_masterLogicGrid.Height; y++)
                {
                    AddWall(new Vector2Int(x, y));
                }
            }
            return m_masterLogicGrid;
        }

        public void AddWall(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x, pos.y] == null ||
                (m_masterLogicGrid.Grid[pos.x, pos.y])?.GetCellType() == cellType.CellInfo.CellType.CELL_NONE)
            {
                m_masterLogicGrid.Grid[pos.x, pos.y] = new world.cellType.WallCell();
                m_wall.SetTile(new Vector3Int(pos.x, pos.y, 0), m_tileWall);
            }
        }
    }
}
