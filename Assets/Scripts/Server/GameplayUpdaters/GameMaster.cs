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

        public override void Setup()
        {
            m_sectionState = new SectionState();
        }

        public override void InitClient(ClientState state)
        {
        }

        public override void InitPlayer(PlayerState player)
        {
            m_playerMovementUpdater.SetPlayerPosition(player, GetPlayerSpawnPos());
        }

        public override void FixedUpdateFromClient(ClientState client, InputFrame frame, float deltaTime)
        {
        }

        public override void UpdateClient(ClientState client)
        {
        }

        public Vector2Int GetPlayerSpawnPos()
        {
            return m_world.GetPlayerSpawnPos();
        }

        // Callback for section door opening
        public void InteractSectionDoorButton(ubv.common.world.cellType.SectionDoorButtonCell cell)
        {
            Debug.Log("ButtonSectionDoor Pressed");
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
            if (m_sectionState.UnlockSectionAvailable(cell.Section))
            {
                // TODO: ouvrir porte final
            }
        }

        // Callback for final door opening
        public void InteractSectionButton(ubv.common.world.cellType.SectionButton cell)
        {
            Debug.Log("SectionButton Pressed");
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
                // TODO: ouvrir porte final
            }
        }

        public WorldGenerator GetWorldGenerator()
        {
            return m_world;
        }
    }
}