using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using ubv.common.world;
using UnityEngine;

namespace ubv.common.world
{
    class DoorManager
    {
        // effective range to look at ==> 4 <= x <= 6
        private const int c_lowerLookRange = 4;
        private const int c_upperLookRange = 6;

        private ubv.common.world.LogicGrid m_masterLogicGrid;
        private Tilemap m_floor;
        private Tilemap m_door;
        private TileBase m_tileFloor;
        private TileBase m_tileDoor;
        private List<RoomInfo> m_roomInMap;

        private int DoorCount = 0;

        public DoorManager(dataStruct.WorldGeneratorToDoorManager worldGeneratorToDoorManager)
        {
            m_masterLogicGrid = worldGeneratorToDoorManager.masterLogicGrid;
            m_floor = worldGeneratorToDoorManager.floor;
            m_door = worldGeneratorToDoorManager.door;
            m_tileFloor = worldGeneratorToDoorManager.tileFloor;
            m_tileDoor = worldGeneratorToDoorManager.tileDoor;
            m_roomInMap = worldGeneratorToDoorManager.roomInMap;
        }

        public LogicGrid GenerateDoorGrid()
        {
            foreach (RoomInfo room in m_roomInMap)
            {
                if (!GenerateDoorForRoom(room))
                {
                    Debug.LogError("MAP CREATION ALERT : Some room does not have any door");
                }
            }
            AddSectionDoorNorth();
            AddSectionDoorEast();
            AddSectionDoorSouth();
            AddSectionDoorWest();

            AddSection0NorthEast();
            AddSection0SouthEast();
            AddSection0SouthWest();
            AddSection0NorthWest();

            Debug.LogError("Frontier Door Count : " + DoorCount);

            m_floor.RefreshAllTiles();
            m_door.RefreshAllTiles();
            return m_masterLogicGrid;
        }

        private bool GenerateDoorForRoom(RoomInfo room)
        {
            int numberDoor = 0;

            numberDoor += GenerateDoorNorth(room);
            numberDoor += GenerateDoorEast(room);
            numberDoor += GenerateDoorSouth(room);
            numberDoor += GenerateDoorWest(room);

            return (numberDoor == 0 ? false : true);
        }

        private int GenerateDoorNorth(RoomInfo room)
        {
            Vector2Int wallOrigin = new Vector2Int((int)room.transform.position.x, (int)room.transform.position.y + room.Height);
            List<Vector2Int> possibleDoor = new List<Vector2Int>();
            if ((int)room.transform.position.y - c_upperLookRange > 0 && (int)room.transform.position.y + room.Height + c_upperLookRange < m_masterLogicGrid.Height)
            {
                for (int i = 0 + 1; i < room.Width - 1; i++)
                {
                    if ((m_masterLogicGrid.Grid[wallOrigin.x + i - 1, wallOrigin.y])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // 1 before
                        &&
                        (m_masterLogicGrid.Grid[wallOrigin.x + i, wallOrigin.y])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // center
                        &&
                        (m_masterLogicGrid.Grid[wallOrigin.x + i + 1, wallOrigin.y])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)// 1 after
                    {
                        CompleteNorthDoor(wallOrigin, i);
                        return 1;
                    }
                    else if ((m_masterLogicGrid.Grid[wallOrigin.x + i - 1, wallOrigin.y + c_upperLookRange - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // 1 before
                            &&
                            (m_masterLogicGrid.Grid[wallOrigin.x + i, wallOrigin.y + c_upperLookRange - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // center
                            &&
                            (m_masterLogicGrid.Grid[wallOrigin.x + i + 1, wallOrigin.y + c_upperLookRange - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)// 1 after
                    {
                        possibleDoor.Add(new Vector2Int(wallOrigin.x + i, wallOrigin.y));
                    }
                }
                Vector2Int doorPosition;
                if (possibleDoor.Count > 0)
                {
                    doorPosition = possibleDoor[Random.Range(0, possibleDoor.Count - 1)];
                    AddDoorNorth(doorPosition);
                    return 1;
                } 
            }
            return 0;
        }

        private void AddDoorNorth(Vector2Int doorPosition)
        {
            m_masterLogicGrid.Grid[doorPosition.x - 1, doorPosition.y] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[doorPosition.x,     doorPosition.y] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[doorPosition.x + 1, doorPosition.y] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_door.SetTile(new Vector3Int(doorPosition.x - 1, doorPosition.y, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(doorPosition.x,     doorPosition.y, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(doorPosition.x + 1, doorPosition.y, 0), m_tileDoor);
            for (int i = 1; i < c_upperLookRange; i++)
            {   
                m_masterLogicGrid.Grid[doorPosition.x - 1, doorPosition.y + i] = new world.cellType.FloorCell();
                m_masterLogicGrid.Grid[doorPosition.x,     doorPosition.y + i] = new world.cellType.FloorCell();
                m_masterLogicGrid.Grid[doorPosition.x + 1, doorPosition.y + i] = new world.cellType.FloorCell();
                m_floor.SetTile(new Vector3Int(doorPosition.x - 1, doorPosition.y + i, 0), m_tileFloor);
                m_floor.SetTile(new Vector3Int(doorPosition.x,     doorPosition.y + i, 0), m_tileFloor);
                m_floor.SetTile(new Vector3Int(doorPosition.x + 1, doorPosition.y + i, 0), m_tileFloor);
            }
        }

        private void CompleteNorthDoor(Vector2Int wallOrigin, int i)
        {
            m_masterLogicGrid.Grid[wallOrigin.x + i - 1, wallOrigin.y] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[wallOrigin.x + i, wallOrigin.y] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[wallOrigin.x + i + 1, wallOrigin.y] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_floor.SetTile(new Vector3Int(wallOrigin.x + i - 1, wallOrigin.y, 0), null);
            m_floor.SetTile(new Vector3Int(wallOrigin.x + i, wallOrigin.y, 0), null);
            m_floor.SetTile(new Vector3Int(wallOrigin.x + i + 1, wallOrigin.y, 0), null);
            m_door.SetTile(new Vector3Int(wallOrigin.x + i - 1, wallOrigin.y, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(wallOrigin.x + i, wallOrigin.y, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(wallOrigin.x + i + 1, wallOrigin.y, 0), m_tileDoor);
        }

        private int GenerateDoorEast(RoomInfo room)
        {
            Vector2Int wallOrigin = new Vector2Int((int)room.transform.position.x + room.Width, (int)room.transform.position.y);
            List<Vector2Int> possibleDoor = new List<Vector2Int>();
            if ((int)room.transform.position.x - c_upperLookRange > 0 && (int)room.transform.position.x + room.Width + c_upperLookRange < m_masterLogicGrid.Width)
            {
                for (int i = 0 + 1; i < room.Height - 1; i++)
                {
                    if ((m_masterLogicGrid.Grid[wallOrigin.x, wallOrigin.y + i - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // 1 before
                        &&
                        (m_masterLogicGrid.Grid[wallOrigin.x, wallOrigin.y + i])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // center
                        &&
                        (m_masterLogicGrid.Grid[wallOrigin.x, wallOrigin.y + i + 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)// 1 after
                    {
                        CompleteEastDoor(wallOrigin, i);
                        return 1;
                    }
                    else if ((m_masterLogicGrid.Grid[wallOrigin.x + c_upperLookRange - 1, wallOrigin.y + i - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // 1 before
                            &&
                            (m_masterLogicGrid.Grid[wallOrigin.x + c_upperLookRange - 1, wallOrigin.y + i])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // center
                            &&
                            (m_masterLogicGrid.Grid[wallOrigin.x + c_upperLookRange - 1, wallOrigin.y + i + 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)// 1 after
                    {
                        possibleDoor.Add(new Vector2Int(wallOrigin.x, wallOrigin.y + i));
                    }
                }
                Vector2Int doorPosition;
                if (possibleDoor.Count > 0)
                {
                    doorPosition = possibleDoor[Random.Range(0, possibleDoor.Count - 1)];
                    AddDoorEast(doorPosition);
                    return 1;
                }
            }
            return 0;
        }

        private void AddDoorEast(Vector2Int doorPosition)
        {
            m_masterLogicGrid.Grid[doorPosition.x, doorPosition.y - 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[doorPosition.x, doorPosition.y    ] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[doorPosition.x, doorPosition.y + 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_door.SetTile(new Vector3Int(doorPosition.x, doorPosition.y - 1, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(doorPosition.x, doorPosition.y,     0), m_tileDoor);
            m_door.SetTile(new Vector3Int(doorPosition.x, doorPosition.y + 1, 0), m_tileDoor);
            for (int i = 1; i < c_upperLookRange; i++)
            {
                m_masterLogicGrid.Grid[doorPosition.x + i, doorPosition.y - 1] = new world.cellType.FloorCell();
                m_masterLogicGrid.Grid[doorPosition.x + i, doorPosition.y    ] = new world.cellType.FloorCell();
                m_masterLogicGrid.Grid[doorPosition.x + i, doorPosition.y + 1] = new world.cellType.FloorCell();
                m_floor.SetTile(new Vector3Int(doorPosition.x + i, doorPosition.y - 1, 0), m_tileFloor);
                m_floor.SetTile(new Vector3Int(doorPosition.x + i, doorPosition.y,     0), m_tileFloor);
                m_floor.SetTile(new Vector3Int(doorPosition.x + i, doorPosition.y + 1, 0), m_tileFloor);
            }
        }

        private void CompleteEastDoor(Vector2Int wallOrigin, int i)
        {
            m_masterLogicGrid.Grid[wallOrigin.x, wallOrigin.y + i - 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[wallOrigin.x, wallOrigin.y + i] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[wallOrigin.x, wallOrigin.y + i + 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_floor.SetTile(new Vector3Int(wallOrigin.x, wallOrigin.y + i - 1, 0), null);
            m_floor.SetTile(new Vector3Int(wallOrigin.x, wallOrigin.y + i, 0), null);
            m_floor.SetTile(new Vector3Int(wallOrigin.x, wallOrigin.y + i + 1, 0), null);
            m_door.SetTile(new Vector3Int(wallOrigin.x, wallOrigin.y + i - 1, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(wallOrigin.x, wallOrigin.y + i, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(wallOrigin.x, wallOrigin.y + i + 1, 0), m_tileDoor);
        }

            private int GenerateDoorSouth(RoomInfo room)
        {
            Vector2Int wallOrigin = new Vector2Int((int)room.transform.position.x, (int)room.transform.position.y);
            List<Vector2Int> possibleDoor = new List<Vector2Int>();
            if ((int)room.transform.position.y - c_upperLookRange > 0 && (int)room.transform.position.y + room.Height + c_upperLookRange < m_masterLogicGrid.Height)
            {
                for (int i = 0 + 1; i < room.Width - 1; i++)
                {
                    if ((m_masterLogicGrid.Grid[wallOrigin.x + i - 1, wallOrigin.y - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // 1 before
                        &&
                        (m_masterLogicGrid.Grid[wallOrigin.x + i, wallOrigin.y - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // center
                        &&
                        (m_masterLogicGrid.Grid[wallOrigin.x + i + 1, wallOrigin.y - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)// 1 after)
                    {
                        CompleteSouthDoor(wallOrigin, i);
                        return 1;
                    }
                    else if ((m_masterLogicGrid.Grid[wallOrigin.x + i - 1, wallOrigin.y - c_upperLookRange - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // 1 before
                            &&
                            (m_masterLogicGrid.Grid[wallOrigin.x + i, wallOrigin.y - c_upperLookRange - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // center
                            &&
                            (m_masterLogicGrid.Grid[wallOrigin.x + i + 1, wallOrigin.y - c_upperLookRange - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)// 1 after
                    {
                        possibleDoor.Add(new Vector2Int(wallOrigin.x + i, wallOrigin.y - 1));
                    }
                }
                Vector2Int doorPosition;
                if (possibleDoor.Count > 0)
                {
                    doorPosition = possibleDoor[Random.Range(0, possibleDoor.Count - 1)];
                    AddDoorSouth(doorPosition);
                    return 1;
                }
            }
            return 0;
        }

        private void AddDoorSouth(Vector2Int doorPosition)
        {
            m_masterLogicGrid.Grid[doorPosition.x - 1, doorPosition.y] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[doorPosition.x,     doorPosition.y] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[doorPosition.x + 1, doorPosition.y] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_door.SetTile(new Vector3Int(doorPosition.x - 1, doorPosition.y, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(doorPosition.x,     doorPosition.y, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(doorPosition.x + 1, doorPosition.y, 0), m_tileDoor);
            for (int i = 1; i < c_upperLookRange; i++)
            {
                m_masterLogicGrid.Grid[doorPosition.x - 1, doorPosition.y - i] = new world.cellType.FloorCell();
                m_masterLogicGrid.Grid[doorPosition.x,     doorPosition.y - i] = new world.cellType.FloorCell();
                m_masterLogicGrid.Grid[doorPosition.x + 1, doorPosition.y - i] = new world.cellType.FloorCell();
                m_floor.SetTile(new Vector3Int(doorPosition.x - 1, doorPosition.y - i, 0), m_tileFloor);
                m_floor.SetTile(new Vector3Int(doorPosition.x,     doorPosition.y - i, 0), m_tileFloor);
                m_floor.SetTile(new Vector3Int(doorPosition.x + 1, doorPosition.y - i, 0), m_tileFloor);
            }
        }

        private void CompleteSouthDoor(Vector2Int wallOrigin, int i)
        {
            m_masterLogicGrid.Grid[wallOrigin.x + i - 1, wallOrigin.y - 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[wallOrigin.x + i, wallOrigin.y - 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[wallOrigin.x + i + 1, wallOrigin.y - 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_floor.SetTile(new Vector3Int(wallOrigin.x + i - 1, wallOrigin.y - 1, 0), null);
            m_floor.SetTile(new Vector3Int(wallOrigin.x + i, wallOrigin.y - 1, 0), null);
            m_floor.SetTile(new Vector3Int(wallOrigin.x + i + 1, wallOrigin.y - 1, 0), null);
            m_door.SetTile(new Vector3Int(wallOrigin.x + i - 1, wallOrigin.y - 1, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(wallOrigin.x + i, wallOrigin.y - 1, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(wallOrigin.x + i + 1, wallOrigin.y - 1, 0), m_tileDoor);
        }

            private int GenerateDoorWest(RoomInfo room)
        {
            Vector2Int wallOrigin = new Vector2Int((int)room.transform.position.x, (int)room.transform.position.y);
            List<Vector2Int> possibleDoor = new List<Vector2Int>();
            if ((int)room.transform.position.x - c_upperLookRange > 0 && (int)room.transform.position.x + room.Width + c_upperLookRange < m_masterLogicGrid.Width)
            {
                for (int i = 0 + 1; i < room.Height - 1; i++)
                {
                    if ((m_masterLogicGrid.Grid[wallOrigin.x - 1, wallOrigin.y + i - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // 1 before
                        &&
                        (m_masterLogicGrid.Grid[wallOrigin.x - 1, wallOrigin.y + i    ])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // center
                        &&
                        (m_masterLogicGrid.Grid[wallOrigin.x - 1, wallOrigin.y + i + 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)// 1 after
                    {
                        CompleteWestDoor(wallOrigin, i);
                        return 1;
                    }
                    else if ((m_masterLogicGrid.Grid[wallOrigin.x - c_upperLookRange - 1, wallOrigin.y + i - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // 1 before
                            &&
                            (m_masterLogicGrid.Grid[wallOrigin.x - c_upperLookRange - 1, wallOrigin.y + i])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR // center
                            &&
                            (m_masterLogicGrid.Grid[wallOrigin.x - c_upperLookRange - 1, wallOrigin.y + i + 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)// 1 after
                    {
                        possibleDoor.Add(new Vector2Int(wallOrigin.x - 1, wallOrigin.y + i));
                    }
                }
                Vector2Int doorPosition;
                if (possibleDoor.Count > 0)
                {
                    doorPosition = possibleDoor[Random.Range(0, possibleDoor.Count - 1)];
                    AddDoorWest(doorPosition);
                    return 1;
                }
            }
            return 0;
        }

        private void AddDoorWest(Vector2Int doorPosition)
        {
            m_masterLogicGrid.Grid[doorPosition.x, doorPosition.y - 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[doorPosition.x, doorPosition.y    ] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[doorPosition.x, doorPosition.y + 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_door.SetTile(new Vector3Int(doorPosition.x, doorPosition.y - 1, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(doorPosition.x, doorPosition.y,     0), m_tileDoor);
            m_door.SetTile(new Vector3Int(doorPosition.x, doorPosition.y + 1, 0), m_tileDoor);
            for (int i = 1; i < c_upperLookRange; i++)
            {
                m_masterLogicGrid.Grid[doorPosition.x - i, doorPosition.y - 1] = new world.cellType.FloorCell();
                m_masterLogicGrid.Grid[doorPosition.x - i, doorPosition.y    ] = new world.cellType.FloorCell();
                m_masterLogicGrid.Grid[doorPosition.x - i, doorPosition.y + 1] = new world.cellType.FloorCell();
                m_floor.SetTile(new Vector3Int(doorPosition.x - i, doorPosition.y - 1, 0), m_tileFloor);
                m_floor.SetTile(new Vector3Int(doorPosition.x - i, doorPosition.y,     0), m_tileFloor);
                m_floor.SetTile(new Vector3Int(doorPosition.x - i, doorPosition.y + 1, 0), m_tileFloor);
            }
        }

        private void CompleteWestDoor(Vector2Int wallOrigin, int i)
        {
            m_masterLogicGrid.Grid[wallOrigin.x - 1, wallOrigin.y + i - 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[wallOrigin.x - 1, wallOrigin.y + i] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_masterLogicGrid.Grid[wallOrigin.x - 1, wallOrigin.y + i + 1] = new world.cellType.DoorCell(cellType.DoorType.Standard);
            m_floor.SetTile(new Vector3Int(wallOrigin.x - 1, wallOrigin.y + i - 1, 0), null);
            m_floor.SetTile(new Vector3Int(wallOrigin.x - 1, wallOrigin.y + i, 0), null);
            m_floor.SetTile(new Vector3Int(wallOrigin.x - 1, wallOrigin.y + i + 1, 0), null);
            m_door.SetTile(new Vector3Int(wallOrigin.x - 1, wallOrigin.y + i - 1, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(wallOrigin.x - 1, wallOrigin.y + i, 0), m_tileDoor);
            m_door.SetTile(new Vector3Int(wallOrigin.x - 1, wallOrigin.y + i + 1, 0), m_tileDoor);
        }

        private void AddSectionDoorNorth()
        {
            int x = m_masterLogicGrid.Width / 2;
            int yMin = m_masterLogicGrid.Height * 2 / 3;
            int yMax = m_masterLogicGrid.Height;

            AddDoorY(x, yMin, yMax, cellType.DoorType.Section_North);
        }

        private void AddSectionDoorEast()
        {
            int xMin = m_masterLogicGrid.Width * 2 / 3;
            int xMax = m_masterLogicGrid.Width;
            int y = m_masterLogicGrid.Height / 2;

            AddDoorX(xMin, xMax, y, cellType.DoorType.Section_East);
        }

        private void AddSectionDoorSouth()
        {
            int x = m_masterLogicGrid.Width / 2;
            int yMin = 0;
            int yMax = m_masterLogicGrid.Height / 3;

            AddDoorY(x, yMin, yMax, cellType.DoorType.Section_South);
        }

        private void AddSectionDoorWest()
        {
            int xMin = 0;
            int xMax = m_masterLogicGrid.Width / 3;
            int y = m_masterLogicGrid.Height / 2;

            AddDoorX(xMin, xMax, y, cellType.DoorType.Section_West);
        }

        private void AddSection0NorthEast()
        {
            int xMin = m_masterLogicGrid.Width / 2;
            int xMax = m_masterLogicGrid.Width *2 / 3;
            int y = m_masterLogicGrid.Height * 2 / 3;

            AddDoorX(xMin, xMax, y, cellType.DoorType.Section0_NorthEast);

            int x = m_masterLogicGrid.Width * 2 / 3;
            int yMin = m_masterLogicGrid.Height / 2;
            int yMax = m_masterLogicGrid.Height * 2 / 3;

            AddDoorY(x, yMin, yMax, cellType.DoorType.Section0_NorthEast);
        }

        private void AddSection0SouthEast()
        {
            int xMin = m_masterLogicGrid.Width / 2;
            int xMax = m_masterLogicGrid.Width * 2 / 3;
            int y = m_masterLogicGrid.Height / 3;

            AddDoorX(xMin, xMax, y, cellType.DoorType.Section0_SouthEast);

            int x = m_masterLogicGrid.Width * 2 / 3;
            int yMin = m_masterLogicGrid.Height / 3;
            int yMax = m_masterLogicGrid.Height / 2;

            AddDoorY(x, yMin, yMax, cellType.DoorType.Section0_SouthEast);
        }

        private void AddSection0SouthWest()
        {
            int xMin = m_masterLogicGrid.Width / 3;
            int xMax = m_masterLogicGrid.Width / 2;
            int y = m_masterLogicGrid.Height / 3;

            AddDoorX(xMin, xMax, y, cellType.DoorType.Section0_SouthWest);

            int x = m_masterLogicGrid.Width / 3;
            int yMin = m_masterLogicGrid.Height / 3;
            int yMax = m_masterLogicGrid.Height / 2;

            AddDoorY(x, yMin, yMax, cellType.DoorType.Section0_SouthWest);
        }

        private void AddSection0NorthWest()
        {
            int xMin = m_masterLogicGrid.Width / 3;
            int xMax = m_masterLogicGrid.Width / 2;
            int y = m_masterLogicGrid.Height * 2 / 3;

            AddDoorX(xMin, xMax, y, cellType.DoorType.Section0_NorthWest);

            int x = m_masterLogicGrid.Width / 3;
            int yMin = m_masterLogicGrid.Height / 2;
            int yMax = m_masterLogicGrid.Height * 2 / 3;

            AddDoorY(x, yMin, yMax, cellType.DoorType.Section0_NorthWest);
        }

        private void AddDoorX(int xMin, int xMax, int y, cellType.DoorType type)
        {
            for (int x = xMin + 1; x < xMax - 1; x++)
            {
                if ((m_masterLogicGrid.Grid[x - 1, y])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR &&
                    (m_masterLogicGrid.Grid[x,     y])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR &&
                    (m_masterLogicGrid.Grid[x + 1, y])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)
                {
                    m_masterLogicGrid.Grid[x - 1, y] = new world.cellType.DoorCell(type);
                    m_masterLogicGrid.Grid[x,     y] = new world.cellType.DoorCell(type);
                    m_masterLogicGrid.Grid[x + 1, y] = new world.cellType.DoorCell(type);
                    m_floor.SetTile(new Vector3Int(x - 1, y, 0), null); // Remove tile
                    m_floor.SetTile(new Vector3Int(x,     y, 0), null); // Remove tile
                    m_floor.SetTile(new Vector3Int(x + 1, y, 0), null); // Remove tile
                    m_door.SetTile(new Vector3Int(x - 1, y, 0), m_tileDoor);
                    m_door.SetTile(new Vector3Int(x,     y, 0), m_tileDoor);
                    m_door.SetTile(new Vector3Int(x + 1, y, 0), m_tileDoor);
                    DoorCount++;
                }
            }
        }

        private void AddDoorY(int x, int yMin, int yMax, cellType.DoorType type)
        {
            for (int y = yMin + 1; y < yMax - 1; y++)
            {
                if ((m_masterLogicGrid.Grid[x, y - 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR &&
                    (m_masterLogicGrid.Grid[x, y    ])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR &&
                    (m_masterLogicGrid.Grid[x, y + 1])?.GetCellType() == cellType.CellInfo.CellType.CELL_FLOOR)
                {
                    m_masterLogicGrid.Grid[x, y - 1] = new world.cellType.DoorCell(type);
                    m_masterLogicGrid.Grid[x, y    ] = new world.cellType.DoorCell(type);
                    m_masterLogicGrid.Grid[x, y + 1] = new world.cellType.DoorCell(type);
                    m_floor.SetTile(new Vector3Int(x, y - 1, 0), null); // Remove tile
                    m_floor.SetTile(new Vector3Int(x, y,     0), null); // Remove tile
                    m_floor.SetTile(new Vector3Int(x, y + 1, 0), null); // Remove tile
                    m_door.SetTile(new Vector3Int(x, y - 1, 0), m_tileDoor);
                    m_door.SetTile(new Vector3Int(x, y,     0), m_tileDoor);
                    m_door.SetTile(new Vector3Int(x, y + 1, 0), m_tileDoor);
                    DoorCount++;
                }
            }
        }
    }
}
