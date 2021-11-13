using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.data;
using ubv.common.serialization;
using ubv.common;
using UnityEngine;
using ubv.common.world.cellType;
using ubv.common.world;
using UnityEngine.Tilemaps;

namespace ubv.server.logic
{
    class GameMaster : ServerGameplayStateUpdater
    {
        [SerializeField] private ubv.common.world.WorldGenerator m_world;
        [SerializeField] private PlayerMovementUpdater m_playerMovementUpdater;
        [SerializeField] private GameplayState m_gamePlayState;

        SectionState m_sectionState;
        List<SectionDoorButtonCell> m_sectionDoorButtonCells;

        private Dictionary<DoorCell, Vector2Int> m_finalDoor;
        private Dictionary<DoorCell, Vector2Int> m_doorNorth;
        private Dictionary<DoorCell, Vector2Int> m_doorEast;
        private Dictionary<DoorCell, Vector2Int> m_doorSouth;
        private Dictionary<DoorCell, Vector2Int> m_doorWest;
        private Dictionary<DoorCell, Vector2Int> m_doorSection0NorthEast;
        private Dictionary<DoorCell, Vector2Int> m_doorSection0SouthEast;
        private Dictionary<DoorCell, Vector2Int> m_doorSection0SouthWest;
        private Dictionary<DoorCell, Vector2Int> m_doorSection0NorthWest;

        private List<common.serialization.types.Int32> m_openingDoorList;

        private Tilemap m_finalDoorTilemap;
        private bool m_alreadyOpen;

        private void Awake()
        {
            m_world.OnWorldGenerated += OnWorldGenerated;
            m_openingDoorList = new List<common.serialization.types.Int32>();
            m_alreadyOpen = false;
        }

        public override void Setup()
        {
            m_sectionState = new SectionState();           
        }

        public override void InitWorld(WorldState state)
        {
        }
        
        public override void FixedUpdateFromClient(WorldState state, Dictionary<int, InputFrame> frames, float deltaTime)
        {
            state.SetOpeningDoor(m_openingDoorList);
        }

        public override void UpdateWorld(WorldState state)
        {
        }

        public Vector2Int GetPlayerSpawnPos()
        {
            return m_world.GetPlayerSpawnPos();
        }

        public void InteractSectionDoorButton(ubv.common.world.cellType.SectionDoorButtonCell cell)
        {
            //Debug.Log("ButtonSectionDoor Pressed");
            switch (cell.Section)
            {
                case Section.NorthEast:
                    m_sectionState._NorthEastDoorButton = true;
                    break;
                case Section.SouthEast:
                    m_sectionState._SouthEastDoorButton = true;
                    break;
                case Section.SouthWest:
                    m_sectionState._SouthWestDoorButton = true;
                    break;
                case Section.NorthWest:
                    m_sectionState._NorthWestDoorButton = true;
                    break;
            }
            OpeningDoor();
        }

        public void InteractSectionButton(ubv.common.world.cellType.SectionButton cell)
        {
            //Debug.Log("SectionButton Pressed");
            switch (cell.Section)
            {
                case Section.NorthEast:
                    m_sectionState._NorthEastButton = true;
                    //Debug.Log("Section North East");
                    break;
                case Section.SouthEast:
                    m_sectionState._SouthEastButton = true;
                    //Debug.Log("Section South East");
                    break;
                case Section.SouthWest:
                    m_sectionState._SouthWestButton = true;
                    //Debug.Log("Section South West");
                    break;
                case Section.NorthWest:
                    m_sectionState._NorthWestButton = true;
                    //Debug.Log("Section North West");
                    break;
            }
            if (m_sectionState.UnlockFinalDoor() && !m_alreadyOpen)
            {
                m_alreadyOpen = true;
                //Debug.LogWarning("Dans le débareur de porte final");
                m_openingDoorList.Add(new common.serialization.types.Int32((int)DoorType.FinalDoor));

                foreach (DoorCell door in m_finalDoor.Keys)
                {
                    door.OpenDoor();
                }

                m_finalDoorTilemap.ClearAllTiles();
                m_finalDoorTilemap.RefreshAllTiles();
            }
        }

        public void InteractFinalButton()
        {
            Debug.LogWarning("PARTIE FINI FINI FINI FINI FINI FINI FINI");

            m_gamePlayState.EndGame();
        }

        private void OpeningDoor()
        {
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section0_NorthEast))
            {
                RemoveDoor(m_doorSection0NorthEast);
                m_openingDoorList.Add(new common.serialization.types.Int32((int)DoorType.Section0_NorthEast));
                m_sectionState._DoorNorthEastOpened = true;
                //Debug.Log("Section0 - North East Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section0_SouthEast))
            {
                RemoveDoor(m_doorSection0SouthEast);
                m_openingDoorList.Add(new common.serialization.types.Int32((int)DoorType.Section0_SouthEast));
                m_sectionState._DoorSouthEastOpened = true;
                //Debug.Log("Section0 - South East Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section0_SouthWest))
            {
                RemoveDoor(m_doorSection0SouthWest);
                m_openingDoorList.Add(new common.serialization.types.Int32((int)DoorType.Section0_SouthWest));
                m_sectionState._DoorSouthWestOpened = true;
                //Debug.Log("Section0 - South West Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section0_NorthWest))
            {
                RemoveDoor(m_doorSection0NorthWest);
                m_openingDoorList.Add(new common.serialization.types.Int32((int)DoorType.Section0_NorthWest));
                m_sectionState._DoorNorthWestOpened = true;
                //Debug.Log("Section0 - North West Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section_North))
            {
                RemoveDoor(m_doorNorth);
                m_openingDoorList.Add(new common.serialization.types.Int32((int)DoorType.Section_North));
                m_sectionState._DoorNorthOpened = true;
                //Debug.Log("North Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section_East))
            {
                RemoveDoor(m_doorEast);
                m_openingDoorList.Add(new common.serialization.types.Int32((int)DoorType.Section_East));
                m_sectionState._DoorEastOpened = true;
                //Debug.Log("East Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section_South))
            {
                RemoveDoor(m_doorSouth);
                m_openingDoorList.Add(new common.serialization.types.Int32((int)DoorType.Section_South));
                m_sectionState._DoorSouthOpened = true;
                //Debug.Log("South Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section_West))
            {
                RemoveDoor(m_doorWest);
                m_openingDoorList.Add(new common.serialization.types.Int32((int)DoorType.Section_West));
                m_sectionState._DoorWestOpened = true;
                //Debug.Log("West Opened");
            }
        }
        
        public void RemoveDoor(Dictionary<DoorCell, Vector2Int> dic)
        {
            foreach (DoorCell door in dic.Keys)
            {
                Vector3Int pos = new Vector3Int(dic[door].x, dic[door].y, 0);
                door.OpenDoor();
                m_world.GetDoorTilemap().SetTile(pos, null);
                m_world.GetDoorTilemap().RefreshTile(pos);
            }
        }


        public WorldGenerator GetWorldGenerator()
        {
            return m_world;
        }

        private void OnWorldGenerated()
        {
            m_finalDoor = m_world.FetchDoorWithPosition(DoorType.FinalDoor);
            m_doorNorth = m_world.FetchDoorWithPosition(DoorType.Section_North);
            m_doorEast = m_world.FetchDoorWithPosition(DoorType.Section_East);
            m_doorSouth = m_world.FetchDoorWithPosition(DoorType.Section_South);
            m_doorWest = m_world.FetchDoorWithPosition(DoorType.Section_West);
            m_doorSection0NorthEast = m_world.FetchDoorWithPosition(DoorType.Section0_NorthEast);
            m_doorSection0SouthEast = m_world.FetchDoorWithPosition(DoorType.Section0_SouthEast);
            m_doorSection0SouthWest = m_world.FetchDoorWithPosition(DoorType.Section0_SouthWest);
            m_doorSection0NorthWest = m_world.FetchDoorWithPosition(DoorType.Section0_NorthWest);

            m_finalDoorTilemap = GameObject.Find("Tilemap_finalDoor").GetComponent<Tilemap>();
        }
    }
}