using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        class Move
        {
            public Vector2Int pos;
            public Direction dir;

            public Move(Vector2Int pos, Direction dir)
            {
                this.pos = pos;
                this.dir = dir;
            }
        }

        const int distanceForMove = 2;
        const int distanceForJump = 5;

        private ubv.common.world.LogicGrid m_masterLogicGrid;
        private Tilemap m_floor;
        private TileBase m_tileFloor;

        private int m_wallThickness;

        private int PassCount = 0;


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
                CreatePath(new Move(coord, GetRandomDirection(coord)));
                coord = GetStartCoord();          
            }

            Debug.LogError("Frontier Pass Count : " + PassCount);

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
                    if (!IsStartPointOnFrontier(pos))
                    {
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
            }
            return new Vector2Int(-1,-1); // Aucun emplacement de vide;
        }

        // Fonction pas utiliser, mais si jamais utile plus tard je veux pas la supprimer
        private bool IsStartCoordSurrondingClear(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x,     pos.y    ] != null || // center                 
                m_masterLogicGrid.Grid[pos.x - 1, pos.y - 1] != null || // bottom-left
                m_masterLogicGrid.Grid[pos.x - 1, pos.y    ] != null || // left
                m_masterLogicGrid.Grid[pos.x - 1, pos.y + 1] != null || // top-left
                m_masterLogicGrid.Grid[pos.x,     pos.y + 1] != null || // top
                m_masterLogicGrid.Grid[pos.x + 1, pos.y + 1] != null || // top-right
                m_masterLogicGrid.Grid[pos.x + 1, pos.y    ] != null || // right
                m_masterLogicGrid.Grid[pos.x + 1, pos.y - 1] != null || // bottom-right
                m_masterLogicGrid.Grid[pos.x,     pos.y - 1] != null || // bottom
                m_masterLogicGrid.Grid[pos.x + 2, pos.y - 1] != null ||
                m_masterLogicGrid.Grid[pos.x + 2, pos.y    ] != null ||
                m_masterLogicGrid.Grid[pos.x + 2, pos.y + 1] != null ||
                m_masterLogicGrid.Grid[pos.x - 2, pos.y - 1] != null ||
                m_masterLogicGrid.Grid[pos.x - 2, pos.y    ] != null ||
                m_masterLogicGrid.Grid[pos.x - 2, pos.y + 1] != null ||
                m_masterLogicGrid.Grid[pos.x - 1, pos.y - 2] != null ||
                m_masterLogicGrid.Grid[pos.x    , pos.y - 2] != null ||
                m_masterLogicGrid.Grid[pos.x + 1, pos.y - 2] != null ||
                m_masterLogicGrid.Grid[pos.x - 1, pos.y + 2] != null ||
                m_masterLogicGrid.Grid[pos.x    , pos.y + 2] != null ||
                m_masterLogicGrid.Grid[pos.x + 1, pos.y + 2] != null 
                )
            {
                return false;
            }
            return true;
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
            
            Debug.DrawLine(new Vector3(pos.x - 1, pos.y - 1), new Vector3(pos.x + 2, pos.y + 2), Color.white, 500f, false);
        }

        private void CreatePath(Move move)
        {
            move.pos = MoveCursor(move.pos, move.dir);
            AddTile(move.pos, move.dir);
            move = GetRandomDirection(move.pos, move.dir);
            while (move.dir != Direction.Stop)
            {                
                CreatePath(move);
                move = GetRandomDirection(move.pos, move.dir);
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
            switch (dir) // TODO Retirer tout les add to tilemap
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
        }

        private Move GetRandomDirection(Vector2Int pos, Direction foward)
        {
            List<int> dir = new List<int> { 0, 1, 2, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4}; // TODO look to clear useless check
            int select;

            bool LookForDirection = true;
            Direction DirectionFound = Direction.Stop;
            while (dir.Count > 0 && LookForDirection)
            {
                select = Random.Range(0, dir.Count - 1);

                switch (dir.ElementAt(select))
                {
                    case 0:
                        if (IsSpaceFreeNorth(pos))
                        {
                            DirectionFound = Direction.North;
                            LookForDirection = false;
                        }
                        break;
                    case 1:
                        if (IsSpaceFreeEast(pos))
                        {
                            DirectionFound = Direction.East;
                            LookForDirection = false;
                        }
                        break;
                    case 2:
                        if (IsSpaceFreeSouth(pos))
                        {
                            DirectionFound = Direction.South;
                            LookForDirection = false;
                        }
                        break;
                    case 3:
                        if (IsSpaceFreeWest(pos))
                        {
                            DirectionFound = Direction.West;
                            LookForDirection = false;
                        }
                        break;
                    default:
                        if (IsSpaceFree(pos, foward))
                        {
                            DirectionFound = foward;
                            LookForDirection = false;
                        }
                        break;
                }                
                if(select < 4)
                {
                    dir.RemoveAt(select);
                }
                else
                {
                    //dir.RemoveRange(5, dir.Count - 5);
                    dir.RemoveRange(4, dir.Count - 4);
                }
                if (IsNearFrontier(pos, DirectionFound) && !LookForDirection)
                {
                    if (IsFullyGoingThroughFrontier(pos, DirectionFound))
                    {
                        PassCount++;
                        Vector2Int posTemp = PassFrontier(pos, DirectionFound);
                        if (posTemp.x == -1)
                        {
                            DirectionFound = Direction.Stop;
                            LookForDirection = false;
                        }
                        else 
                        {
                            pos = posTemp;
                        }
                    }
                    else
                    {
                        LookForDirection = true;
                    }
                }

            }                    

            return new Move(pos, DirectionFound);
        }

        private Direction GetRandomDirection(Vector2Int pos)
        {
            List<int> dir = new List<int> { 0, 1, 2, 3 };

            int select;

            while (dir.Count > 0)
            {
                select = Random.Range(0, dir.Count - 1);

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

        private bool IsSpaceFreeNorth(Vector2Int pos, bool FrontierCheck = false)
        {
            if(!FrontierCheck)
            {
                if (pos.y < m_masterLogicGrid.Height - distanceForMove - m_wallThickness)
                {
                    if (CheckMoveNorth(pos))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                if (pos.y < m_masterLogicGrid.Height - distanceForJump - m_wallThickness)
                {
                    if (CheckJumpNorth(pos))
                    {
                        return true;
                    }
                }
                return false;
            }            
        }

        private bool CheckMoveNorth(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x - m_wallThickness, pos.y + distanceForMove + m_wallThickness] != null || // top-left-left
                m_masterLogicGrid.Grid[pos.x - 1,               pos.y + distanceForMove + m_wallThickness] != null || // top-left
                m_masterLogicGrid.Grid[pos.x,                   pos.y + distanceForMove + m_wallThickness] != null || // top
                m_masterLogicGrid.Grid[pos.x + 1,               pos.y + distanceForMove + m_wallThickness] != null || // top-right
                m_masterLogicGrid.Grid[pos.x + m_wallThickness, pos.y + distanceForMove + m_wallThickness] != null || // top-right-right
                m_masterLogicGrid.Grid[pos.x - m_wallThickness, pos.y + distanceForMove] != null ||
                m_masterLogicGrid.Grid[pos.x - 1,               pos.y + distanceForMove] != null ||
                m_masterLogicGrid.Grid[pos.x,                   pos.y + distanceForMove] != null ||
                m_masterLogicGrid.Grid[pos.x + 1,               pos.y + distanceForMove] != null ||
                m_masterLogicGrid.Grid[pos.x + m_wallThickness, pos.y + distanceForMove] != null
                )
            {
                return false;
            }
            return true;
        }

        private bool CheckJumpNorth(Vector2Int pos)
        {
            Vector2Int nextPos = new Vector2Int(pos.x, pos.y + distanceForJump);
            if (m_masterLogicGrid.Grid[pos.x - m_wallThickness, pos.y + distanceForJump + m_wallThickness] != null || // top-left-left
                m_masterLogicGrid.Grid[pos.x - 1,               pos.y + distanceForJump + m_wallThickness] != null || // top-left
                m_masterLogicGrid.Grid[pos.x,                   pos.y + distanceForJump + m_wallThickness] != null || // top
                m_masterLogicGrid.Grid[pos.x + 1,               pos.y + distanceForJump + m_wallThickness] != null || // top-right
                m_masterLogicGrid.Grid[pos.x + m_wallThickness, pos.y + distanceForJump + m_wallThickness] != null || // top-right-right
                m_masterLogicGrid.Grid[pos.x - m_wallThickness, pos.y + distanceForJump] != null ||
                m_masterLogicGrid.Grid[pos.x - 1,               pos.y + distanceForJump] != null ||
                m_masterLogicGrid.Grid[pos.x,                   pos.y + distanceForJump] != null ||
                m_masterLogicGrid.Grid[pos.x + 1,               pos.y + distanceForJump] != null ||
                m_masterLogicGrid.Grid[pos.x + m_wallThickness, pos.y + distanceForJump] != null ||
                IsNearFrontier(nextPos, Direction.North)
                )
            {
                return false;
            }
            return true;
        }

        private bool IsSpaceFreeEast(Vector2Int pos, bool FrontierCheck = false)
        {
            if(!FrontierCheck)
            {
                if (pos.x < m_masterLogicGrid.Width - distanceForMove - m_wallThickness)
                {
                    if (CheckMoveEast(pos))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                if (pos.x < m_masterLogicGrid.Width - distanceForJump - m_wallThickness)
                {
                    if (CheckJumpEast(pos))
                    {
                        return true;
                    }
                }
                return false;
            }            
        }
        
        private bool CheckMoveEast(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x + distanceForMove + m_wallThickness, pos.y + m_wallThickness] != null || // right-top-top
                m_masterLogicGrid.Grid[pos.x + distanceForMove + m_wallThickness, pos.y + 1              ] != null || // right-top
                m_masterLogicGrid.Grid[pos.x + distanceForMove + m_wallThickness, pos.y                  ] != null || // right
                m_masterLogicGrid.Grid[pos.x + distanceForMove + m_wallThickness, pos.y - 1              ] != null || // right-bottom
                m_masterLogicGrid.Grid[pos.x + distanceForMove + m_wallThickness, pos.y - m_wallThickness] != null || // right-bottom-bottom
                m_masterLogicGrid.Grid[pos.x + distanceForMove, pos.y + m_wallThickness] != null ||
                m_masterLogicGrid.Grid[pos.x + distanceForMove, pos.y + 1              ] != null ||
                m_masterLogicGrid.Grid[pos.x + distanceForMove, pos.y                  ] != null ||
                m_masterLogicGrid.Grid[pos.x + distanceForMove, pos.y - 1              ] != null ||
                m_masterLogicGrid.Grid[pos.x + distanceForMove, pos.y - m_wallThickness] != null
                )
            {
                return false;
            }
            return true;
        }

        private bool CheckJumpEast(Vector2Int pos)
        {
            Vector2Int nextPos = new Vector2Int(pos.x + distanceForJump, pos.y);
            if (m_masterLogicGrid.Grid[pos.x + distanceForJump + m_wallThickness, pos.y + m_wallThickness] != null || // right-top-top
                m_masterLogicGrid.Grid[pos.x + distanceForJump + m_wallThickness, pos.y + 1              ] != null || // right-top
                m_masterLogicGrid.Grid[pos.x + distanceForJump + m_wallThickness, pos.y                  ] != null || // right
                m_masterLogicGrid.Grid[pos.x + distanceForJump + m_wallThickness, pos.y - 1              ] != null || // right-bottom
                m_masterLogicGrid.Grid[pos.x + distanceForJump + m_wallThickness, pos.y - m_wallThickness] != null || // right-bottom-bottom
                m_masterLogicGrid.Grid[pos.x + distanceForJump, pos.y + m_wallThickness] != null ||
                m_masterLogicGrid.Grid[pos.x + distanceForJump, pos.y + 1              ] != null ||
                m_masterLogicGrid.Grid[pos.x + distanceForJump, pos.y                  ] != null ||
                m_masterLogicGrid.Grid[pos.x + distanceForJump, pos.y - 1              ] != null ||
                m_masterLogicGrid.Grid[pos.x + distanceForJump, pos.y - m_wallThickness] != null ||
                IsNearFrontier(nextPos, Direction.East)
                )
            {
                return false;
            }
            return true;
        }

        private bool IsSpaceFreeSouth(Vector2Int pos, bool FrontierCheck = false)
        {
            if (!FrontierCheck)
            {
                if (pos.y > distanceForMove + m_wallThickness)
                {
                    if (CheckMoveSouth(pos))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                if (pos.y > distanceForJump + m_wallThickness)
                {
                    if (CheckJumpSouth(pos))
                    {
                        return true;
                    }
                }
                return false;
            }            
        }

        private bool CheckMoveSouth(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x - m_wallThickness, pos.y - distanceForMove - m_wallThickness] != null|| // bottom-left-left
                m_masterLogicGrid.Grid[pos.x - 1,               pos.y - distanceForMove - m_wallThickness] != null|| // bottom-left
                m_masterLogicGrid.Grid[pos.x,                   pos.y - distanceForMove - m_wallThickness] != null|| // bottom
                m_masterLogicGrid.Grid[pos.x + 1,               pos.y - distanceForMove - m_wallThickness] != null|| // bottom-right
                m_masterLogicGrid.Grid[pos.x + m_wallThickness, pos.y - distanceForMove - m_wallThickness] != null|| // bottom-right-right
                m_masterLogicGrid.Grid[pos.x - m_wallThickness, pos.y - distanceForMove] != null ||
                m_masterLogicGrid.Grid[pos.x - 1,               pos.y - distanceForMove] != null ||
                m_masterLogicGrid.Grid[pos.x,                   pos.y - distanceForMove] != null ||
                m_masterLogicGrid.Grid[pos.x + 1,               pos.y - distanceForMove] != null ||
                m_masterLogicGrid.Grid[pos.x + m_wallThickness, pos.y - distanceForMove] != null
                )
            {
                return false;
            }
            return true;
        }

        private bool CheckJumpSouth(Vector2Int pos)
        {
            Vector2Int nextPos = new Vector2Int(pos.x, pos.y - distanceForJump);
            if (m_masterLogicGrid.Grid[pos.x - m_wallThickness, pos.y - distanceForJump - m_wallThickness] != null || // bottom-left-left
                m_masterLogicGrid.Grid[pos.x - 1,               pos.y - distanceForJump - m_wallThickness] != null || // bottom-left
                m_masterLogicGrid.Grid[pos.x,                   pos.y - distanceForJump - m_wallThickness] != null || // bottom
                m_masterLogicGrid.Grid[pos.x + 1,               pos.y - distanceForJump - m_wallThickness] != null || // bottom-right
                m_masterLogicGrid.Grid[pos.x + m_wallThickness, pos.y - distanceForJump - m_wallThickness] != null || // bottom-right-right
                m_masterLogicGrid.Grid[pos.x - m_wallThickness, pos.y - distanceForJump] != null ||
                m_masterLogicGrid.Grid[pos.x - 1,               pos.y - distanceForJump] != null ||
                m_masterLogicGrid.Grid[pos.x,                   pos.y - distanceForJump] != null ||
                m_masterLogicGrid.Grid[pos.x + 1,               pos.y - distanceForJump] != null ||
                m_masterLogicGrid.Grid[pos.x + m_wallThickness, pos.y - distanceForJump] != null ||
                IsNearFrontier(nextPos, Direction.South)
                )
            {
                return false;
            }
            return true;
        }

        private bool IsSpaceFreeWest(Vector2Int pos, bool FrontierCheck = false)
        {
            if (!FrontierCheck)
            {
                if (pos.x > distanceForMove + m_wallThickness)
                {
                    if (CheckMoveWest(pos))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                if (pos.x > distanceForJump + m_wallThickness)
                {
                    if (CheckJumpWest(pos))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private bool CheckMoveWest(Vector2Int pos)
        {
            if (m_masterLogicGrid.Grid[pos.x - distanceForMove - m_wallThickness, pos.y - m_wallThickness] != null || // left-bottom-bottom
                m_masterLogicGrid.Grid[pos.x - distanceForMove - m_wallThickness, pos.y - 1              ] != null || // left-bottom
                m_masterLogicGrid.Grid[pos.x - distanceForMove - m_wallThickness, pos.y                  ] != null || // left
                m_masterLogicGrid.Grid[pos.x - distanceForMove - m_wallThickness, pos.y + 1              ] != null || // left-top
                m_masterLogicGrid.Grid[pos.x - distanceForMove - m_wallThickness, pos.y + m_wallThickness] != null || // left-top-top
                m_masterLogicGrid.Grid[pos.x - distanceForMove, pos.y - m_wallThickness] != null ||
                m_masterLogicGrid.Grid[pos.x - distanceForMove, pos.y - 1              ] != null ||
                m_masterLogicGrid.Grid[pos.x - distanceForMove, pos.y                  ] != null ||
                m_masterLogicGrid.Grid[pos.x - distanceForMove, pos.y + 1              ] != null ||
                m_masterLogicGrid.Grid[pos.x - distanceForMove, pos.y + m_wallThickness] != null
                )
            {
                return false;
            }
            return true;
        }

        private bool CheckJumpWest(Vector2Int pos)
        {
            Vector2Int nextPos = new Vector2Int(pos.x - distanceForJump, pos.y);
            if (m_masterLogicGrid.Grid[pos.x - distanceForJump - m_wallThickness, pos.y - m_wallThickness] != null || // left-bottom-bottom
                m_masterLogicGrid.Grid[pos.x - distanceForJump - m_wallThickness, pos.y - 1              ] != null || // left-bottom
                m_masterLogicGrid.Grid[pos.x - distanceForJump - m_wallThickness, pos.y                  ] != null || // left
                m_masterLogicGrid.Grid[pos.x - distanceForJump - m_wallThickness, pos.y + 1              ] != null || // left-top
                m_masterLogicGrid.Grid[pos.x - distanceForJump - m_wallThickness, pos.y + m_wallThickness] != null || // left-top-top
                m_masterLogicGrid.Grid[pos.x - distanceForJump, pos.y - m_wallThickness] != null ||
                m_masterLogicGrid.Grid[pos.x - distanceForJump, pos.y - 1              ] != null ||
                m_masterLogicGrid.Grid[pos.x - distanceForJump, pos.y                  ] != null ||
                m_masterLogicGrid.Grid[pos.x - distanceForJump, pos.y + 1              ] != null ||
                m_masterLogicGrid.Grid[pos.x - distanceForJump, pos.y + m_wallThickness] != null ||
                IsNearFrontier(nextPos, Direction.West)
                )
            {
                return false;
            }
            return true;
        }

        private bool IsStartPointOnFrontier(Vector2Int pos)
        {
            if (TileOnFrontier(pos.x,     pos.y    ) ||
                TileOnFrontier(pos.x - 1, pos.y - 1) ||
                TileOnFrontier(pos.x - 1, pos.y    ) ||
                TileOnFrontier(pos.x - 1, pos.y + 1) ||
                TileOnFrontier(pos.x,     pos.y + 1) ||
                TileOnFrontier(pos.x + 1, pos.y + 1) ||
                TileOnFrontier(pos.x + 1, pos.y    ) ||
                TileOnFrontier(pos.x + 1, pos.y - 1) ||
                TileOnFrontier(pos.x,     pos.y - 1))
            {
                return true;
            }
            return false;
        }

        private Vector2Int PassFrontier(Vector2Int pos, Direction dir)
        {
            Vector2Int err = new Vector2Int(-1, -1);
            switch (dir)
            {
                case Direction.North:
                    if(IsSpaceFreeNorth(pos, true))
                    {
                        return JumpFrontier(pos, dir);
                    }
                    return err;
                case Direction.East:
                    if (IsSpaceFreeEast(pos, true))
                    {
                        return JumpFrontier(pos, dir);
                    }
                    return err;
                case Direction.South:
                    if (IsSpaceFreeSouth(pos, true))
                    {
                        return JumpFrontier(pos, dir);
                    }
                    return err;
                case Direction.West:
                    if (IsSpaceFreeWest(pos, true))
                    {
                        return JumpFrontier(pos, dir);
                    }
                    return err;
                default:
                    return err;
            }
        }

        private Vector2Int JumpFrontier(Vector2Int pos, Direction dir)
        {
            Vector2Int debut = pos;
            for (int i = 0; i < 4; i++)
            {
                pos = MoveCursor(pos, dir);
                AddTile(pos, dir);
            }
            Debug.DrawLine(new Vector3(debut.x, debut.y), new Vector3(pos.x, pos.y), Color.white, 500f, false);
            return pos;
        }

        private bool IsFullyGoingThroughFrontier(Vector2Int pos, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return IsOnFrontierNorth(pos);
                case Direction.East:
                    return IsOnFrontierEast(pos);
                case Direction.South:
                    return IsOnFrontierSouth(pos);
                case Direction.West:
                    return IsOnFrontierWest(pos);
                case Direction.Stop:
                    return false;
                default:
                    return false;
            }
        }

        private bool IsOnFrontierNorth(Vector2Int pos)
        {
            
            if(TileOnFrontier(pos.x - 2, pos.y + 2) &&
               TileOnFrontier(pos.x - 1, pos.y + 2) &&
               TileOnFrontier(pos.x,     pos.y + 2) &&
               TileOnFrontier(pos.x + 1, pos.y + 2) &&
               TileOnFrontier(pos.x + 2, pos.y + 2))
            {
                return true;                
            }                                  
            return false;
        }

        private bool IsOnFrontierEast(Vector2Int pos)
        {
            if (TileOnFrontier(pos.x + 2, pos.y - 2) &&
                TileOnFrontier(pos.x + 2, pos.y - 1) &&
                TileOnFrontier(pos.x + 2, pos.y    ) &&
                TileOnFrontier(pos.x + 2, pos.y + 1) &&
                TileOnFrontier(pos.x + 2, pos.y + 2))
            {
                return true;
            }
            return false;
        }

        private bool IsOnFrontierSouth(Vector2Int pos)
        {
            if (TileOnFrontier(pos.x - 2, pos.y - 2) &&
                TileOnFrontier(pos.x - 1, pos.y - 2) &&
                TileOnFrontier(pos.x,     pos.y - 2) &&
                TileOnFrontier(pos.x + 1, pos.y - 2) &&
                TileOnFrontier(pos.x + 2, pos.y - 2))
            {
                return true;
            }
            return false;
        }

        private bool IsOnFrontierWest(Vector2Int pos)
        {
            if (TileOnFrontier(pos.x - 2, pos.y - 2) &&
                TileOnFrontier(pos.x - 2, pos.y - 1) &&
                TileOnFrontier(pos.x - 2, pos.y    ) &&
                TileOnFrontier(pos.x - 2, pos.y + 1) &&
                TileOnFrontier(pos.x - 2, pos.y + 2))
            {
                return true;
            }
            return false;
        }

        private bool IsNearFrontier(Vector2Int pos, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return IsNearFrontierNorth(pos);
                case Direction.East:
                    return IsNearFrontierEast(pos);
                case Direction.South:
                    return IsNearFrontierSouth(pos);
                case Direction.West:
                    return IsNearFrontierWest(pos);
                case Direction.Stop:
                    return false;
                default:
                    return false;
            }
        }

        private bool IsNearFrontierNorth(Vector2Int pos)
        {

            if (TileOnFrontier(pos.x - 1, pos.y + 2) ||
                TileOnFrontier(pos.x,     pos.y + 2) ||
                TileOnFrontier(pos.x + 1, pos.y + 2))
            {
                return true;
            }
            return false;
        }

        private bool IsNearFrontierEast(Vector2Int pos)
        {
            if (TileOnFrontier(pos.x + 2, pos.y - 1) ||
                TileOnFrontier(pos.x + 2, pos.y    ) ||
                TileOnFrontier(pos.x + 2, pos.y + 1))
            {
                return true;
            }
            return false;
        }

        private bool IsNearFrontierSouth(Vector2Int pos)
        {
            if (TileOnFrontier(pos.x - 1, pos.y - 2) ||
                TileOnFrontier(pos.x,     pos.y - 2) ||
                TileOnFrontier(pos.x + 1, pos.y - 2))
            {
                return true;
            }
            return false;
        }

        private bool IsNearFrontierWest(Vector2Int pos)
        {
            if (TileOnFrontier(pos.x - 2, pos.y - 1) ||
                TileOnFrontier(pos.x - 2, pos.y    ) ||
                TileOnFrontier(pos.x - 2, pos.y + 1))
            {
                return true;
            }
            return false;
        }

        private bool TileOnFrontier(int x, int y)
        {
            if (   (x >= m_masterLogicGrid.Width / 3     && x < m_masterLogicGrid.Width * 2 / 3   && y == m_masterLogicGrid.Height * 2 / 3) // Section0_North
                || (x == m_masterLogicGrid.Width * 2 / 3 && y >= m_masterLogicGrid.Height / 3     && y < m_masterLogicGrid.Height * 2 / 3) // Section0_East
                || (x >= m_masterLogicGrid.Width / 3     && x < m_masterLogicGrid.Width * 2 / 3   && y == m_masterLogicGrid.Height / 3) // Section0_South
                || (x == m_masterLogicGrid.Width / 3     && y >= m_masterLogicGrid.Height / 3     && y < m_masterLogicGrid.Height * 2 / 3) // Section0_West
                || (x == m_masterLogicGrid.Width / 2     && y >= m_masterLogicGrid.Height * 2 / 3 && y < m_masterLogicGrid.Height) // North
                || (x >= m_masterLogicGrid.Width * 2 / 3 && x < m_masterLogicGrid.Width           && y == m_masterLogicGrid.Height / 2) // East
                || (x == m_masterLogicGrid.Width / 2     && y >= 0                                && y < m_masterLogicGrid.Height / 3) // South
                || (x >= 0                               && x < m_masterLogicGrid.Width / 3       && y == m_masterLogicGrid.Height / 2) // West
               )
            {
                return true;
            }
            return false;
        }

    }
}
