﻿using System.Collections;
using System.Collections.Generic;
using ubv.common.world.cellType;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ubv.common.world
{
    public class RoomInfo : MonoBehaviour
    {
        [SerializeField] private string m_name;

        [SerializeField] private Tilemap m_floor;
        [SerializeField] private Tilemap m_wall;
        [SerializeField] private Tilemap m_door;
        [SerializeField] private Tilemap m_interactable;
        [SerializeField] private Tilemap m_finalDoor;

        [SerializeField] private Tilemap m_sectionDoorButton_NorthEast;
        [SerializeField] private Tilemap m_sectionDoorButton_SouthEast;
        [SerializeField] private Tilemap m_sectionDoorButton_SouthWest;
        [SerializeField] private Tilemap m_sectionDoorButton_NorthWest;

        [SerializeField] private Tilemap m_sectionButton_NorthEast;
        [SerializeField] private Tilemap m_sectionButton_SouthEast;
        [SerializeField] private Tilemap m_sectionButton_SouthWest;
        [SerializeField] private Tilemap m_sectionButton_NorthWest;

        [SerializeField] private Tilemap m_finalButton;
        [SerializeField] private Tilemap m_playerSpawnZone;

        [SerializeField] private int m_height;
        [SerializeField] private int m_width;
        private Vector3Int m_roomOrigin;

        private LogicGrid m_logicGrid;

        public int Height { get => m_height; private set => m_height = value; }
        public int Width { get => m_width; private set => m_width = value; }
        public LogicGrid LogicGrid { get => m_logicGrid; private set => m_logicGrid = value; }

        private void Awake()
        {
#if DEBUG
            Debug.Assert(m_floor != null);
#endif // DEBUG

            RoomManagement();

            LogicGrid = new LogicGrid(Width, Height);

            DoorManagement();
            InteractableManagement();
            WallManagement();
            PlayerSpawnManagement();

            FinalDoorManagement();
            FinalButtonManagement();
            SectionDoorButtonManagement(m_sectionDoorButton_NorthEast, Section.NorthEast);
            SectionDoorButtonManagement(m_sectionDoorButton_SouthEast, Section.SouthEast);
            SectionDoorButtonManagement(m_sectionDoorButton_SouthWest, Section.SouthWest);
            SectionDoorButtonManagement(m_sectionDoorButton_NorthWest, Section.NorthWest);
            SectionButtonManagement(m_sectionButton_NorthEast, Section.NorthEast);
            SectionButtonManagement(m_sectionButton_SouthEast, Section.SouthEast);
            SectionButtonManagement(m_sectionButton_SouthWest, Section.SouthWest);
            SectionButtonManagement(m_sectionButton_NorthWest, Section.NorthWest);

            FloorManagement();
        }
        private void RoomManagement()
        {
            m_floor.CompressBounds();
            Width = m_floor.cellBounds.size.x;
            Height = m_floor.cellBounds.size.y;
            m_roomOrigin = m_floor.origin;
        }
        private void FloorManagement()
        {
            m_floor.CompressBounds();
            Vector3Int iterateur = new Vector3Int();
            for (iterateur.x = m_roomOrigin.x; iterateur.x < m_floor.cellBounds.size.x + m_roomOrigin.x; iterateur.x++)
            {
                for (iterateur.y = m_roomOrigin.y; iterateur.y < m_floor.cellBounds.size.y + m_roomOrigin.y; iterateur.y++)
                {
                    if (m_floor.HasTile(iterateur))
                    {
                        if (LogicGrid.Grid[iterateur.x, iterateur.y] == null)
                        {
                            LogicGrid.Grid[iterateur.x, iterateur.y] = new cellType.FloorCell();
                        }
                    }
                }
            }
        }
        private void WallManagement()
        {
            if (m_wall)
            {
                m_wall.CompressBounds();
                Vector3Int originOffset = m_wall.origin - m_roomOrigin;
                Vector3Int iterateur = new Vector3Int();
                for (iterateur.x = originOffset.x; iterateur.x < m_wall.cellBounds.size.x + originOffset.x; iterateur.x++)
                {
                    for (iterateur.y = originOffset.y; iterateur.y < m_wall.cellBounds.size.y + originOffset.y; iterateur.y++)
                    {
                        if (m_wall.HasTile(iterateur))
                        {
                            if (LogicGrid.Grid[iterateur.x, iterateur.y] == null)
                            {
                                LogicGrid.Grid[iterateur.x,iterateur.y] = new cellType.WallCell();
                            }                            
                        }
                    }
                }
            }
        }
        private void DoorManagement()
        {
            if (m_door)
            {
                m_door.CompressBounds();
                Vector3Int originOffset = m_door.origin - m_roomOrigin;
                Vector3Int iterateur = new Vector3Int();
                for (iterateur.x = originOffset.x; iterateur.x < m_door.cellBounds.size.x + originOffset.x; iterateur.x++)
                {
                    for (iterateur.y = originOffset.y; iterateur.y < m_door.cellBounds.size.y + originOffset.y; iterateur.y++)
                    {
                        if (m_door.HasTile(iterateur))
                        {
                            if (LogicGrid.Grid[iterateur.x, iterateur.y] == null)
                            {
                                LogicGrid.Grid[iterateur.x, iterateur.y] = new cellType.DoorCell(cellType.DoorType.Standard);
                            }                        
                        }
                    }
                }
            }
        }
        private void InteractableManagement()
        {
            if (m_interactable)
            {
                m_interactable.CompressBounds();
                Vector3Int originOffset = m_interactable.origin - m_roomOrigin;
                Vector3Int iterateur = new Vector3Int();
                for (iterateur.x = originOffset.x; iterateur.x < m_interactable.cellBounds.size.x + originOffset.x; iterateur.x++)
                {
                    for (iterateur.y = originOffset.y; iterateur.y < m_interactable.cellBounds.size.y + originOffset.y; iterateur.y++)
                    {
                        if (m_interactable.HasTile(iterateur))
                        {
                            if (LogicGrid.Grid[iterateur.x, iterateur.y] == null)
                            {
                                LogicGrid.Grid[iterateur.x, iterateur.y] = new cellType.DoorButtonCell();
                            }
                        }
                    }
                }
            }
        }

        private void PlayerSpawnManagement()
        {
            if (m_playerSpawnZone)
            {
                m_playerSpawnZone.CompressBounds();
                Vector3Int originOffset = m_playerSpawnZone.origin - m_roomOrigin;
                Vector3Int iterateur = new Vector3Int();
                for (iterateur.x = originOffset.x; iterateur.x < m_playerSpawnZone.cellBounds.size.x + originOffset.x; iterateur.x++)
                {
                    for (iterateur.y = originOffset.y; iterateur.y < m_playerSpawnZone.cellBounds.size.y + originOffset.y; iterateur.y++)
                    {
                        if (m_playerSpawnZone.HasTile(iterateur))
                        {
                            if (LogicGrid.Grid[iterateur.x, iterateur.y] == null)
                            {
                                LogicGrid.Grid[iterateur.x, iterateur.y] = new cellType.PlayerSpawnCell();
                            }
                        }
                    }
                }
            }
        }

        private void SectionDoorButtonManagement(Tilemap tilemap, Section section)
        {
            if (tilemap)
            {
                tilemap.CompressBounds();
                Vector3Int originOffset = tilemap.origin - m_roomOrigin;
                Vector3Int iterateur = new Vector3Int();
                for (iterateur.x = originOffset.x; iterateur.x < tilemap.cellBounds.size.x + originOffset.x; iterateur.x++)
                {
                    for (iterateur.y = originOffset.y; iterateur.y < tilemap.cellBounds.size.y + originOffset.y; iterateur.y++)
                    {
                        if (tilemap.HasTile(iterateur))
                        {
                            if (LogicGrid.Grid[iterateur.x, iterateur.y] == null)
                            {
                                LogicGrid.Grid[iterateur.x, iterateur.y] = new cellType.SectionDoorButtonCell(section);
                            }
                        }
                    }
                }
            }
        }

        // Paramètrer sivoupla
        private void SectionButtonManagement(Tilemap tilemap, Section section)
        {
            if (tilemap)
            {
                tilemap.CompressBounds();
                Vector3Int originOffset = tilemap.origin - m_roomOrigin;
                Vector3Int iterateur = new Vector3Int();
                for (iterateur.x = originOffset.x; iterateur.x < tilemap.cellBounds.size.x + originOffset.x; iterateur.x++)
                {
                    for (iterateur.y = originOffset.y; iterateur.y < tilemap.cellBounds.size.y + originOffset.y; iterateur.y++)
                    {
                        if (tilemap.HasTile(iterateur))
                        {
                            if (LogicGrid.Grid[iterateur.x, iterateur.y] == null)
                            {
                                LogicGrid.Grid[iterateur.x, iterateur.y] = new cellType.SectionButton(section);
                            }
                        }
                    }
                }
            }
        }

        private void FinalButtonManagement()
        {
            if (m_finalButton)
            {
                m_finalButton.CompressBounds();
                Vector3Int originOffset = m_finalButton.origin - m_roomOrigin;
                Vector3Int iterateur = new Vector3Int();
                for (iterateur.x = originOffset.x; iterateur.x < m_finalButton.cellBounds.size.x + originOffset.x; iterateur.x++)
                {
                    for (iterateur.y = originOffset.y; iterateur.y < m_finalButton.cellBounds.size.y + originOffset.y; iterateur.y++)
                    {
                        if (m_finalButton.HasTile(iterateur))
                        {
                            if (LogicGrid.Grid[iterateur.x, iterateur.y] == null)
                            {
                                LogicGrid.Grid[iterateur.x, iterateur.y] = new cellType.FinalButtonCell();
                            }
                        }
                    }
                }
            }
        }

        private void FinalDoorManagement()
        {
            if (m_finalDoor)
            {
                m_finalDoor.CompressBounds();
                Vector3Int originOffset = m_finalDoor.origin - m_roomOrigin;
                Vector3Int iterateur = new Vector3Int();
                for (iterateur.x = originOffset.x; iterateur.x < m_finalDoor.cellBounds.size.x + originOffset.x; iterateur.x++)
                {
                    for (iterateur.y = originOffset.y; iterateur.y < m_finalDoor.cellBounds.size.y + originOffset.y; iterateur.y++)
                    {
                        if (m_finalDoor.HasTile(iterateur))
                        {
                            if (LogicGrid.Grid[iterateur.x, iterateur.y] == null)
                            {
                                LogicGrid.Grid[iterateur.x, iterateur.y] = new cellType.DoorCell(cellType.DoorType.FinalDoor);
                            }
                        }
                    }
                }
            }
        }

        Vector2Int GetDimension()
        {
            return new Vector2Int(Width, Height);
        }

    }

}
