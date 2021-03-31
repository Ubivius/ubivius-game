using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ubv.common.world.generationManager
{
    class DeadEndManager
    {
        private LogicGrid m_masterLogicGrid;
        private Tilemap m_floor;
        private Tilemap m_door;
        private List<Vector2Int> m_ends;

        public DeadEndManager(dataStruct.WorldGeneratorToDeadEndManager data)
        {
            m_masterLogicGrid = data.masterLogicGrid;
            m_floor = data.floor;
            m_door = data.door;
            m_ends = data.ends;
        }

        public LogicGrid GenerateDeadEndGrid()
        {
            foreach (Vector2Int pos in m_ends)
            {
                RemoveDeadEnd(pos);
            }

            return m_masterLogicGrid;
        }

        private void RemoveDeadEnd(Vector2Int pos)
        {
            //Debug.Log("X: " + pos.x + "   Y: " + pos.y);
            Direction deadEnd = IsDeadEnd(pos);
            if (deadEnd != Direction.Stop)
            {
                RemoveTile(pos, deadEnd);
                pos = MoveCursor(pos, deadEnd);
                RemoveDeadEnd(pos);
            }
        }

        private Direction IsDeadEnd(Vector2Int pos)
        {
            if (IsFinish(pos))
            {
                return Direction.Stop;
            }
            if (IsDeadEndNorth(pos))
            {
                return Direction.North;
            }
            if (IsDeadEndEast(pos))
            {
                return Direction.East;
            }
            if (IsDeadEndSouth(pos))
            {
                return Direction.South;
            }
            if (IsDeadEndWest(pos))
            {
                return Direction.West;
            }
            return Direction.Stop;
        }

        private bool IsFinish(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x,     pos.y    ] == null && // center                 
                m_masterLogicGrid.Grid[pos.x - 1, pos.y - 1] == null && // bottom-left
                m_masterLogicGrid.Grid[pos.x - 1, pos.y    ] == null && // left
                m_masterLogicGrid.Grid[pos.x - 1, pos.y + 1] == null && // top-left
                m_masterLogicGrid.Grid[pos.x,     pos.y + 1] == null && // top
                m_masterLogicGrid.Grid[pos.x + 1, pos.y + 1] == null && // top-right
                m_masterLogicGrid.Grid[pos.x + 1, pos.y    ] == null && // right
                m_masterLogicGrid.Grid[pos.x + 1, pos.y - 1] == null && // bottom-right
                m_masterLogicGrid.Grid[pos.x,     pos.y - 1] == null    // bottom
                )
            {
                return true;
            }
            return false;
        }

        private bool IsDeadEndNorth(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x - 2, pos.y + 1] == null &&
                m_masterLogicGrid.Grid[pos.x - 1, pos.y + 2] == null &&
                m_masterLogicGrid.Grid[pos.x    , pos.y + 2] == null &&
                m_masterLogicGrid.Grid[pos.x + 1, pos.y + 2] == null &&
                m_masterLogicGrid.Grid[pos.x + 2, pos.y + 1] == null)
            {
                return true;
            }
            return false;
        }

        private bool IsDeadEndEast(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x + 1, pos.y - 2] == null &&
                m_masterLogicGrid.Grid[pos.x + 2, pos.y - 1] == null &&
                m_masterLogicGrid.Grid[pos.x + 2, pos.y    ] == null &&
                m_masterLogicGrid.Grid[pos.x + 2, pos.y + 1] == null &&
                m_masterLogicGrid.Grid[pos.x + 1, pos.y + 2] == null)
            {
                return true;
            }
            return false;
        }

        private bool IsDeadEndSouth(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x - 2, pos.y - 1] == null &&
                m_masterLogicGrid.Grid[pos.x - 1, pos.y - 2] == null &&
                m_masterLogicGrid.Grid[pos.x    , pos.y - 2] == null &&
                m_masterLogicGrid.Grid[pos.x + 1, pos.y - 2] == null &&
                m_masterLogicGrid.Grid[pos.x + 2, pos.y - 1] == null)
            {
                return true;
            }
            return false;
        }

        private bool IsDeadEndWest(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x - 1, pos.y - 2] == null &&
                m_masterLogicGrid.Grid[pos.x - 2, pos.y - 1] == null &&
                m_masterLogicGrid.Grid[pos.x - 2, pos.y    ] == null &&
                m_masterLogicGrid.Grid[pos.x - 2, pos.y + 1] == null &&
                m_masterLogicGrid.Grid[pos.x - 1, pos.y + 2] == null)
            {
                return true;
            }
            return false;
        }

        private Vector2Int MoveCursor(Vector2Int pos, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    pos.y--;
                    return pos;
                case Direction.East:
                    pos.x--;
                    return pos;
                case Direction.West:
                    pos.x++;
                    return pos;
                case Direction.South:
                    pos.y++;
                    return pos;
                default:
                    return pos;
            }
        }

        private void RemoveTile(Vector2Int pos, Direction dir)
        {
            switch (dir) // TODO Retirer tout les add to tilemap
            {
                case Direction.North:
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y + 1] = null;
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y + 1] = null;
                    m_masterLogicGrid.Grid[pos.x,     pos.y + 1] = null;
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), null);
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), null);
                    m_floor.SetTile(new Vector3Int(pos.x,     pos.y + 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x,     pos.y + 1, 0), null);
                    break;
                case Direction.East:
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y - 1] = null;
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y    ] = null;
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y + 1] = null;
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0), null);
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y,     0), null);
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x + 1, pos.y,     0), null);
                    m_door.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), null);
                    break;
                case Direction.West:
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y - 1] = null;
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y    ] = null;
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y + 1] = null;
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0), null);
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y,     0), null);
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x - 1, pos.y,     0), null);
                    m_door.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), null);
                    break;
                case Direction.South:
                    m_masterLogicGrid.Grid[pos.x + 1, pos.y - 1] = null;
                    m_masterLogicGrid.Grid[pos.x - 1, pos.y - 1] = null;
                    m_masterLogicGrid.Grid[pos.x,     pos.y - 1] = null;
                    m_floor.SetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0), null);
                    m_floor.SetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0), null);
                    m_floor.SetTile(new Vector3Int(pos.x,     pos.y - 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0), null);
                    m_door.SetTile(new Vector3Int(pos.x,     pos.y - 1, 0), null);
                    break;
                default:
                    break;
            }
        }
    }
}
