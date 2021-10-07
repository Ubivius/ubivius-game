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

namespace ubv.server.logic
{
    class GameMaster : ServerGameplayStateUpdater
    {
        [SerializeField] private ubv.common.world.WorldGenerator m_world;
        [SerializeField] private PlayerMovementUpdater m_playerMovementUpdater;
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

        private void Awake()
        {
            m_world.OnWorldGenerated += OnWorldGenerated;
        }

        public override void Setup()
        {
            m_sectionState = new SectionState();           
        }

        public override void InitWorld(WorldState state)
        {
        }
        
        public override void FixedUpdateFromClient(WorldState client, Dictionary<int, InputFrame> frames, float deltaTime)
        {
        }

        public override void UpdateWorld(WorldState client)
        {
        }

        public Vector2Int GetPlayerSpawnPos()
        {
            return m_world.GetPlayerSpawnPos();
        }

        // Callback for section door opening
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

        // Callback for final door opening
        public void InteractSectionButton(ubv.common.world.cellType.SectionButton cell)
        {
            //Debug.Log("SectionButton Pressed");
            switch (cell.Section)
            {
                case Section.NorthEast:
                    m_sectionState._NorthEastButton = true;
                    break;
                case Section.SouthEast:
                    m_sectionState._SouthEastButton = true;
                    break;
                case Section.SouthWest:
                    m_sectionState._SouthWestButton = true;
                    break;
                case Section.NorthWest:
                    m_sectionState._NorthWestButton = true;
                    break;
            }
            if (m_sectionState.UnlockFinalDoor())
            {
                RemoveDoor(m_finalDoor);
            }
        }

        private void OpeningDoor()
        {
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section0_NorthEast))
            {
                RemoveDoor(m_doorSection0NorthEast);
                Debug.Log("Section0 - North East Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section0_SouthEast))
            {
                RemoveDoor(m_doorSection0SouthEast);
                Debug.Log("Section0 - South East Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section0_SouthWest))
            {
                RemoveDoor(m_doorSection0SouthWest);
                Debug.Log("Section0 - South West Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section0_NorthWest))
            {
                RemoveDoor(m_doorSection0NorthWest);
                Debug.Log("Section0 - North West Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section_North))
            {
                RemoveDoor(m_doorNorth);
                Debug.Log("North Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section_East))
            {
                RemoveDoor(m_doorEast);
                Debug.Log("East Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section_South))
            {
                RemoveDoor(m_doorSouth);
                Debug.Log("South Opened");
            }
            if (m_sectionState.UnlockSectionAvailable(DoorType.Section_West))
            {
                RemoveDoor(m_doorWest);
                Debug.Log("West Opened");
            }
        }
        
        public void RemoveDoor(Dictionary<DoorCell, Vector2Int> dic)
        {
            foreach (DoorCell door in dic.Keys)
            {
                Vector2Int pos = dic[door];
                door.OpenDoor();
                m_world.GetDoorTilemap().SetTile(new Vector3Int(pos.x, pos.y, 0), null);
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
        }
    }
}