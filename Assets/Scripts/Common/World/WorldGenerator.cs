using Assets.Scripts.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using ubv.common.world.cellType;
using ubv.server.logic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace ubv.common.world
{
    [RequireComponent(typeof(Grid))]
    public class WorldGenerator : ServerInitializer
    {
        [SerializeField] private Vector2Int m_boundariesMap;
        [SerializeField] private PathfindingGridManager m_pathfinder;

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
        [SerializeField] private Tilemap m_wall;
        [SerializeField] private Tilemap m_door;
        [SerializeField] private TileBase m_tileFloor;
        [SerializeField] private TileBase m_tileWall;
        [SerializeField] private TileBase m_tileDoor;
        [SerializeField] private int m_wallThickness;

        private Grid m_grid;

        private LogicGrid m_masterLogicGrid;

        private generationManager.RoomManager m_roomManager;
        private generationManager.CorridorsManager m_corridorsManager;
        private generationManager.DoorManager m_doorManager;
        private generationManager.DeadEndManager m_DeadEndManager;
        private generationManager.WallManager m_wallManager;

        private List<RoomInfo> m_roomInMap = new List<RoomInfo>();

        private dataStruct.WorldGeneratorToRoomManager m_worldGeneratorToRoomManager;
        private dataStruct.WorldGeneratorToCorridorsManager m_worldGeneratorToCorridorsManager;
        private dataStruct.WorldGeneratorToDoorManager m_worldGeneratorToDoorManager;
        private dataStruct.WorldGeneratorToDeadEndManager m_worldGeneratorToDeadEndManager;
        private dataStruct.WolrdGeneratorToWallManager m_wolrdGeneratorToWallManager;

        public UnityAction OnWorldGenerated;

        private List<Vector2Int> m_playerSpawnPos;
        private List<Vector2> m_enemySpawnPositions;

        private bool m_mapIsValid;
        private int m_mapGenerationAttempt;

        private void Awake()
        {
            m_grid = GetComponent<Grid>();

            m_mapIsValid = false;
            m_mapGenerationAttempt = 0;
            m_enemySpawnPositions = new List<Vector2>();

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

            //GenerateWithOneRoom();
            //GenerateWorld();
        }

        public void GenerateWorld()
        {
            while (!m_mapIsValid && m_mapGenerationAttempt < 1000)
            {
                try
                {
                    if (m_roomManager != null)
                    {
                        m_floor.ClearAllTiles();
                        m_door.ClearAllTiles();
                        m_wall.ClearAllTiles();                        
                        foreach (RoomInfo room in m_roomInMap)
                        {
                            Destroy(room.gameObject);
                        }
                        m_roomInMap.Clear();
                    }

                    m_roomManager = new generationManager.RoomManager(m_worldGeneratorToRoomManager);
                    m_masterLogicGrid = m_roomManager.GenerateRoomGrid();
                    m_roomInMap = m_roomManager.GetRoomInMap();

                    m_worldGeneratorToCorridorsManager = new dataStruct.WorldGeneratorToCorridorsManager(m_masterLogicGrid, m_floor, m_tileFloor, m_wallThickness);

                    m_corridorsManager = new generationManager.CorridorsManager(m_worldGeneratorToCorridorsManager);
                    m_masterLogicGrid = m_corridorsManager.GenerateCorridorsGrid();

                    m_worldGeneratorToDoorManager = new dataStruct.WorldGeneratorToDoorManager(m_masterLogicGrid, m_floor, m_door, m_tileFloor, m_tileDoor, m_roomInMap);
                    m_doorManager = new generationManager.DoorManager(m_worldGeneratorToDoorManager);
                    m_masterLogicGrid = m_doorManager.GenerateDoorGrid();

                    m_worldGeneratorToDeadEndManager = new dataStruct.WorldGeneratorToDeadEndManager(m_masterLogicGrid, m_floor, m_door, m_corridorsManager.GetEnds());
                    m_DeadEndManager = new generationManager.DeadEndManager(m_worldGeneratorToDeadEndManager);
                    m_masterLogicGrid = m_DeadEndManager.GenerateDeadEndGrid();

                    m_wolrdGeneratorToWallManager = new dataStruct.WolrdGeneratorToWallManager(m_masterLogicGrid, m_wall, m_tileWall);
                    m_wallManager = new generationManager.WallManager(m_wolrdGeneratorToWallManager);
                    m_masterLogicGrid = m_wallManager.GenerateWallGrid();

                    m_floor.RefreshAllTiles();
                    m_door.RefreshAllTiles();
                    m_wall.RefreshAllTiles();

                    SetPlayerSpawnPosList();

                    OnWorldGenerated?.Invoke();


                    Vector2 centralPos = GetCentralPiecePos();
                    SetPlayerSpawnPosList();
                    foreach (Vector2Int spawn in m_playerSpawnPos)
                    {
                        PathRoute route = m_pathfinder.GetPathRoute(spawn, centralPos);
                        if (route == null || route.PathVectorList.Count < 1)
                        {
                            throw new MapCreationException("MAP CREATION ALERT : SECTION0 to small for mandatory room, look your sizing");
                        }
                    }
                    AddSpawns();
                    m_mapIsValid = true;

                }
                catch (MapCreationException)
                {
                    if (m_roomManager != null)
                    {
                        m_roomInMap = m_roomManager.GetRoomInMap();
                    }
                    if (m_mapGenerationAttempt > 200)
                    {
                        m_boundariesMap = new Vector2Int(m_boundariesMap.x + 50, m_boundariesMap.y + 50);
                    }
                    m_mapGenerationAttempt++;
                }
            }            
            
        }
        
        public void GenerateWithOneRoom() // For test only 
        {
            m_roomManager = new generationManager.RoomManager(m_worldGeneratorToRoomManager);
            m_masterLogicGrid = m_roomManager.AddOneRoom();
        }

        private void SetPlayerSpawnPosList()
        {
            m_playerSpawnPos = new List<Vector2Int>();
            for (int x = 0; x < m_masterLogicGrid.Width; x++) // On ne veut pas regarder en dehors du tableau
            {
                for (int y = 0; y < m_masterLogicGrid.Height; y++)
                {
                    if (m_masterLogicGrid.Grid[x,y].GetCellType() == cellType.CellInfo.CellType.CELL_PLAYERSPAWN)
                    {
                        m_playerSpawnPos.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        private void AddSpawns()
        {
            int width = m_masterLogicGrid.Width;
            int height = m_masterLogicGrid.Height;

            const int MAX_ENEMY_SPAWNS = 100;

            for (int i = 0; i < MAX_ENEMY_SPAWNS; i++)
            {
                Vector2 pos = Vector2.zero;

                do
                {
                    int xPos = Random.Range(3, width - 4);
                    int yPos = Random.Range(3, height - 4);
                    pos = new Vector2(xPos, yPos);
                 } while (m_pathfinder.GetNodeIfWalkable(pos.x, pos.y) == null
                          || m_pathfinder.GetPathRoute(pos, GetCentralPiecePos()) != null
                           || m_pathfinder.GetPathRoute(pos, GetFinalButtonPos()) != null);
                
                m_enemySpawnPositions.Add(pos);
            }
        }

        private Vector2 GetCentralPiecePos()
        {
            int width = m_masterLogicGrid.Width;
            int height = m_masterLogicGrid.Height;
            for (int x = 0; x < m_masterLogicGrid.Width; x++) // On ne veut pas regarder en dehors du tableau
            {
                for (int y = 0; y < m_masterLogicGrid.Height; y++)
                {
                    if (m_masterLogicGrid.Grid[x, y].GetCellType() == cellType.CellInfo.CellType.CELL_SECTIONDOORBUTTON)
                    {
                        return new Vector2(x, y - 1);
                    }
                }
            }
            return new Vector2(0, 0);
        }

        private Vector2 GetFinalButtonPos()
        {
            int width = m_masterLogicGrid.Width;
            int height = m_masterLogicGrid.Height;
            for (int x = 0; x < m_masterLogicGrid.Width; x++) // On ne veut pas regarder en dehors du tableau
            {
                for (int y = 0; y < m_masterLogicGrid.Height; y++)
                {
                    if (m_masterLogicGrid.Grid[x, y].GetCellType() == cellType.CellInfo.CellType.CELL_FINALBUTTON)
                    {
                        return new Vector2(x, y - 1);
                    }
                }
            }
            return new Vector2(0, 0);
        }

        public Vector2Int GetPlayerSpawnPos()
        {
            int select = Random.Range(0, m_playerSpawnPos.Count - 1);
            Vector2Int pos = m_playerSpawnPos[select];
            m_playerSpawnPos.RemoveAt(select);
            return pos;
        }

        public Vector2 GetEnemySpawnPosition()
        {
            if (m_enemySpawnPositions.Count == 0)
                AddSpawns();
            int select = Random.Range(0, m_enemySpawnPositions.Count - 1);
            Vector2 pos = m_enemySpawnPositions[select];
            m_enemySpawnPositions.RemoveAt(select);
            return pos;
        }

        public List<T> FetchAll<T>() where T : LogicCell
        {
            List<T> list = new List<T>();
            LogicGrid grid = GetMasterLogicGrid();
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid.Grid[x, y] is T)
                    {
                        list.Add(grid.Grid[x, y] as T);
                    }
                }
            }

            return list;
        }

        public Dictionary<T, Vector2Int> FetchAllWithPosition<T>() where T : LogicCell
        {
            Dictionary<T, Vector2Int> dictionary = new Dictionary<T, Vector2Int>();
            LogicGrid grid = GetMasterLogicGrid();
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid.Grid[x, y] is T)
                    {
                        dictionary.Add(grid.Grid[x, y] as T, new Vector2Int(x, y));
                    }
                }
            }

            return dictionary;
        }

        public List<DoorCell> FetchDoor(DoorType type)
        {
            List<DoorCell> list = new List<DoorCell>();
            LogicGrid grid = GetMasterLogicGrid();
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid.Grid[x, y] is DoorCell)
                    {
                        DoorCell tmp = grid.Grid[x, y] as DoorCell;
                        if (tmp.DoorType == type)
                        {
                            list.Add(tmp);
                        }                        
                    }
                }
            }
            return list;
        }

        public Dictionary<DoorCell, Vector2Int> FetchDoorWithPosition(DoorType type)
        {
            Dictionary<DoorCell, Vector2Int> dictionary = new Dictionary<DoorCell, Vector2Int>();
            LogicGrid grid = GetMasterLogicGrid();
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid.Grid[x, y] is DoorCell)
                    {
                        DoorCell tmp = grid.Grid[x, y] as DoorCell;
                        if (tmp.DoorType == type)
                        {
                            dictionary.Add(tmp, new Vector2Int(x, y));
                        }
                    }
                }
            }
            return dictionary;
        }

        public cellType.CellInfo[,] GetCellInfoArray()
        {
            return m_masterLogicGrid.GetCellInfo();
        }

        public LogicGrid GetMasterLogicGrid()
        {
            return m_masterLogicGrid;
        }

        public override void Init()
        {
            GenerateWorld();
        }

        public Tilemap GetDoorTilemap()
        {
            return m_door;
        }
    }


    [System.Serializable]
    public class MapCreationException : System.Exception
    {
        public MapCreationException() { }
        public MapCreationException(string message) : base(message) { }
        public MapCreationException(string message, System.Exception inner) : base(message, inner) { }
        protected MapCreationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}

