using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using ubv.common.world;

namespace ubv.common.world
{
    enum Direction
    {
        North,
        East,
        West,
        South,
        Stop
    }

    class CorridorsManager
    {
        private ubv.common.world.LogicGrid m_masterLogicGrid;
        private Tilemap m_floor;
        private TileBase m_tileFloor;

        private int m_wallThickness = 1;

        public CorridorsManager(dataStruct.WorldGeneratorToCorridorsManager data)
        {
            m_masterLogicGrid = data.masterLogicGrid;
            m_floor = data.floor;
            m_tileFloor = data.tileFloor;
            m_wallThickness = data.wallThickness;
        }

        public LogicGrid GenerateCorridorsGrid()
        {
            Vector2Int coord = GetStartCoord();
            while (coord.x != -1)
            {
                CoverStartPoint(coord);
                CreatePath(coord, GetRandomDirection(coord));
                coord = GetStartCoord();          
            }

            return m_masterLogicGrid;
        }

        private Vector2Int GetStartCoord()
        {
            int width = m_masterLogicGrid.Width;
            int height = m_masterLogicGrid.Height;
            for (int x = 1 + m_wallThickness; x < width - 1 - m_wallThickness; x++) // On ne veut pas regarder en dehors du tableau
            {
                for (int y = 1 + m_wallThickness; y < height - 1 - m_wallThickness; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (IsSpaceFreeNorth(pos) &&
                        IsSpaceFreeEast(pos) &&
                        IsSpaceFreeSouth(pos) &&
                        IsSpaceFreeWest(pos))
                    {
                        if (m_masterLogicGrid.Grid[x,     y    ] == null && // center                 
                            m_masterLogicGrid.Grid[x - 1, y - 1] == null && // bottom-left
                            m_masterLogicGrid.Grid[x - 1, y    ] == null && // left
                            m_masterLogicGrid.Grid[x - 1, y + 1] == null && // top-left
                            m_masterLogicGrid.Grid[x,     y + 1] == null && // top
                            m_masterLogicGrid.Grid[x + 1, y + 1] == null && // top-right
                            m_masterLogicGrid.Grid[x + 1, y    ] == null && // right
                            m_masterLogicGrid.Grid[x + 1, y - 1] == null && // bottom-right
                            m_masterLogicGrid.Grid[x,     y - 1] == null    // bottom
                            )
                        {
                            return new Vector2Int(x, y);
                        }
                    }
                }
            }
            return new Vector2Int(-1,-1); // Aucun emplacement de vide;
        }

        private void CoverStartPoint(Vector2Int pos)
        {
            m_masterLogicGrid.Grid[pos.x,     pos.y - 1] = new world.cellType.FloorCell();
            m_masterLogicGrid.Grid[pos.x,     pos.y    ] = new world.cellType.FloorCell();
            m_masterLogicGrid.Grid[pos.x,     pos.y + 1] = new world.cellType.FloorCell();
            m_masterLogicGrid.Grid[pos.x - 1, pos.y - 1] = new world.cellType.FloorCell();
            m_masterLogicGrid.Grid[pos.x - 1, pos.y    ] = new world.cellType.FloorCell();
            m_masterLogicGrid.Grid[pos.x - 1, pos.y + 1] = new world.cellType.FloorCell();
            m_masterLogicGrid.Grid[pos.x + 1, pos.y - 1] = new world.cellType.FloorCell();
            m_masterLogicGrid.Grid[pos.x + 1, pos.y    ] = new world.cellType.FloorCell();
            m_masterLogicGrid.Grid[pos.x + 1, pos.y + 1] = new world.cellType.FloorCell();

            m_floor.SetTile(new Vector3Int(pos.x,     pos.y - 1, 0), m_tileFloor);
            m_floor.SetTile(new Vector3Int(pos.x,     pos.y,     0), m_tileFloor);
            m_floor.SetTile(new Vector3Int(pos.x,     pos.y + 1, 0), m_tileFloor);
            m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0), m_tileFloor);
            m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y    , 0), m_tileFloor);
            m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), m_tileFloor);
            m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0), m_tileFloor);
            m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y    , 0), m_tileFloor);
            m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), m_tileFloor);
            m_floor.RefreshAllTiles();
        }

        private void CreatePath(Vector2Int pos, Direction dir)
        {
            pos = MoveCursor(pos, dir);
            AddTile(pos, dir);
            dir = GetRandomDirection(pos, dir);
            while (dir != Direction.Stop)
            {                
                CreatePath(pos, dir);
                dir = GetRandomDirection(pos, dir);
            }
        }

        private Vector2Int MoveCursor(Vector2Int pos, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    pos.y++;
                    return pos;
                case Direction.East:
                    pos.x++;
                    return pos;
                case Direction.West:
                    pos.x--;
                    return pos;
                case Direction.South:
                    pos.y--;
                    return pos;
                default:
                    return pos;
            }
        }

        private void AddTile(Vector2Int pos, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y + 1] = new world.cellType.FloorCell();
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y + 1] = new world.cellType.FloorCell();
                    m_masterLogicGrid.Grid[pos.x,     pos.y + 1] = new world.cellType.FloorCell();
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), m_tileFloor);
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), m_tileFloor);
                    m_floor.SetTile(new Vector3Int(pos.x,     pos.y + 1, 0), m_tileFloor);
                    break;
                case Direction.East:
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y - 1] = new world.cellType.FloorCell();
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y    ] = new world.cellType.FloorCell();
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y + 1] = new world.cellType.FloorCell();
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0), m_tileFloor);
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y    , 0), m_tileFloor);
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), m_tileFloor);
                    break;
                case Direction.West:
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y - 1] = new world.cellType.FloorCell();
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y    ] = new world.cellType.FloorCell();
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y + 1] = new world.cellType.FloorCell();
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0), m_tileFloor);
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y    , 0), m_tileFloor);
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), m_tileFloor);
                    break;
                case Direction.South:
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y - 1] = new world.cellType.FloorCell();
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y - 1] = new world.cellType.FloorCell();
                    m_masterLogicGrid.Grid[pos.x,     pos.y - 1] = new world.cellType.FloorCell();
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0), m_tileFloor);
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0), m_tileFloor);
                    m_floor.SetTile(new Vector3Int(pos.x,     pos.y - 1, 0), m_tileFloor);
                    break;
                default:
                    break;
            }
            m_floor.RefreshAllTiles();
        }

        private Direction GetRandomDirection(Vector2Int pos, Direction foward)
        {
            List<int> dir = new List<int> { 0, 1, 2, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4};

            int select;

            while (dir.Count > 0)
            {
                select = Random.Range(0, dir.Count);

                switch (dir.ElementAt(select))
                {
                    case 0:
                        if (IsSpaceFreeNorth(pos))
                        {
                            return Direction.North;
                        }
                        break;
                    case 1:
                        if (IsSpaceFreeEast(pos))
                        {
                            return Direction.East;
                        }
                        break;
                    case 2:
                        if (IsSpaceFreeSouth(pos))
                        {
                            return Direction.South;
                        }
                        break;
                    case 3:
                        if (IsSpaceFreeWest(pos))
                        {
                            return Direction.West;
                        }
                        break;
                    default:
                        if (IsSpaceFree(pos, foward))
                        {
                            return foward;
                        }
                        break;
                }                
                if(select < 5)
                {
                    dir.RemoveAt(select);
                }
                else
                {
                    dir.RemoveRange(5, dir.Count - 5);
                }
            }

            return Direction.Stop;
        }

        private Direction GetRandomDirection(Vector2Int pos)
        {
            List<int> dir = new List<int> { 0, 1, 2, 3 };

            int select;

            while (dir.Count > 0)
            {
                select = Random.Range(0, dir.Count);

                switch (dir.ElementAt(select))
                {
                    case 0:
                        if(IsSpaceFreeNorth(pos))
                        {
                            return Direction.North;
                        }
                        break;
                    case 1:
                        if (IsSpaceFreeEast(pos))
                        {
                            return Direction.East;
                        }
                        break;
                    case 2:
                        if (IsSpaceFreeSouth(pos))
                        {
                            return Direction.South;
                        }
                        break;
                    default:
                        if (IsSpaceFreeWest(pos))
                        {
                            return Direction.West;
                        }
                        break;
                }
                dir.RemoveAt(select);
            }

            return Direction.Stop;
        }

        private bool IsSpaceFree(Vector2Int pos, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return IsSpaceFreeNorth(pos);
                case Direction.East:
                    return IsSpaceFreeEast(pos);
                case Direction.West:
                    return IsSpaceFreeWest(pos);
                case Direction.South:
                    return IsSpaceFreeSouth(pos);
            }
            return false;
        }

        private bool IsSpaceFreeNorth(Vector2Int pos)
        {
            if (pos.y < m_masterLogicGrid.Height - 2 - m_wallThickness)
            {
                if (m_masterLogicGrid.Grid[pos.x - 1 - m_wallThickness, pos.y + 2 + m_wallThickness] == null && // top-left-left
                    m_masterLogicGrid.Grid[pos.x - 1 ,                  pos.y + 2 + m_wallThickness] == null && // top-left
                    m_masterLogicGrid.Grid[pos.x,                       pos.y + 2 + m_wallThickness] == null && // top
                    m_masterLogicGrid.Grid[pos.x + 1,                   pos.y + 2 + m_wallThickness] == null && // top-right
                    m_masterLogicGrid.Grid[pos.x + 1 + m_wallThickness, pos.y + 2 + m_wallThickness] == null && // top-right-right
                    m_masterLogicGrid.Grid[pos.x - 1 - m_wallThickness, pos.y + 2] == null &&
                    m_masterLogicGrid.Grid[pos.x - 1,                   pos.y + 2] == null &&
                    m_masterLogicGrid.Grid[pos.x,                       pos.y + 2] == null && 
                    m_masterLogicGrid.Grid[pos.x + 1,                   pos.y + 2] == null &&   
                    m_masterLogicGrid.Grid[pos.x + 1 + m_wallThickness, pos.y + 2] == null 
                    )
                {
                    return true;
                }
            }            
            return false;
        }

        private bool IsSpaceFreeEast(Vector2Int pos)
        {
            if (pos.x < m_masterLogicGrid.Width - 2 - m_wallThickness)
            {
                if (m_masterLogicGrid.Grid[pos.x + 2 + m_wallThickness, pos.y + 1 + m_wallThickness] == null && // right-top-top
                    m_masterLogicGrid.Grid[pos.x + 2 + m_wallThickness, pos.y + 1                  ] == null && // right-top
                    m_masterLogicGrid.Grid[pos.x + 2 + m_wallThickness, pos.y                      ] == null && // right
                    m_masterLogicGrid.Grid[pos.x + 2 + m_wallThickness, pos.y - 1                  ] == null && // right-bottom
                    m_masterLogicGrid.Grid[pos.x + 2 + m_wallThickness, pos.y - 1 - m_wallThickness] == null && // right-bottom-bottom
                    m_masterLogicGrid.Grid[pos.x + 2, pos.y + 1 + m_wallThickness] == null && 
                    m_masterLogicGrid.Grid[pos.x + 2, pos.y + 1                  ] == null &&
                    m_masterLogicGrid.Grid[pos.x + 2, pos.y                      ] == null &&
                    m_masterLogicGrid.Grid[pos.x + 2, pos.y - 1                  ] == null &&
                    m_masterLogicGrid.Grid[pos.x + 2, pos.y - 1 - m_wallThickness] == null
                    )
                {
                    return true;
                }
            }            
            return false;
        }

        private bool IsSpaceFreeSouth(Vector2Int pos)
        {
            if (pos.y > 1 + m_wallThickness)
            {
                if (m_masterLogicGrid.Grid[pos.x - 1 - m_wallThickness, pos.y - 2 - m_wallThickness] == null && // bottom-left-left
                    m_masterLogicGrid.Grid[pos.x - 1,                   pos.y - 2 - m_wallThickness] == null && // bottom-left
                    m_masterLogicGrid.Grid[pos.x,                       pos.y - 2 - m_wallThickness] == null && // bottom
                    m_masterLogicGrid.Grid[pos.x + 1,                   pos.y - 2 - m_wallThickness] == null && // bottom-right
                    m_masterLogicGrid.Grid[pos.x + 1 + m_wallThickness, pos.y - 2 - m_wallThickness] == null && // bottom-right-right
                    m_masterLogicGrid.Grid[pos.x - 1 - m_wallThickness, pos.y - 2] == null &&
                    m_masterLogicGrid.Grid[pos.x - 1,                   pos.y - 2] == null &&
                    m_masterLogicGrid.Grid[pos.x,                       pos.y - 2] == null &&
                    m_masterLogicGrid.Grid[pos.x + 1,                   pos.y - 2] == null &&
                    m_masterLogicGrid.Grid[pos.x + 1 + m_wallThickness, pos.y - 2] == null 
                    )
                {
                    return true;
                }
            }            
            return false;
        }

        private bool IsSpaceFreeWest(Vector2Int pos)
        {
            if (pos.x > 1 + m_wallThickness)
            {
                if (m_masterLogicGrid.Grid[pos.x - 2 - m_wallThickness, pos.y - 1 - m_wallThickness] == null && // left-bottom-bottom
                    m_masterLogicGrid.Grid[pos.x - 2 - m_wallThickness, pos.y - 1                  ] == null && // left-bottom
                    m_masterLogicGrid.Grid[pos.x - 2 - m_wallThickness, pos.y                      ] == null && // left
                    m_masterLogicGrid.Grid[pos.x - 2 - m_wallThickness, pos.y + 1                  ] == null && // left-top
                    m_masterLogicGrid.Grid[pos.x - 2 - m_wallThickness, pos.y + 1 + m_wallThickness] == null && // left-top-top
                    m_masterLogicGrid.Grid[pos.x - 2, pos.y - 1 - m_wallThickness] == null &&
                    m_masterLogicGrid.Grid[pos.x - 2, pos.y - 1                  ] == null &&
                    m_masterLogicGrid.Grid[pos.x - 2, pos.y                      ] == null && 
                    m_masterLogicGrid.Grid[pos.x - 2, pos.y + 1                  ] == null &&
                    m_masterLogicGrid.Grid[pos.x - 2, pos.y + 1 + m_wallThickness] == null
                    )
                {
                    return true;
                }
            }            
            return false;
        }

    }
}
