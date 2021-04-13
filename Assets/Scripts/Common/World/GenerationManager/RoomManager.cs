using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace ubv.common.world.generationManager
{
    public struct BoxInfo
    {
        Vector2Int m_coord;
        Vector3Int m_info;

        public BoxInfo(int x, int y, int width, int height) : this()
        {
            m_coord.x = x;
            m_coord.y = y;
            m_info.x = width;
            m_info.y = height;
            m_info.z = width * height;
        }
        public Vector2Int GetOrigin()
        {
            return m_coord;
        }
        public int GetWidth()
        {
            return m_info.x;
        }
        public int GetHeight()
        {
            return m_info.y;
        }
        public int GetArea()
        {
            return m_info.z;
        }
    }
    class RoomManager
    {
        private const int c_mandatoryTry = 10000;
        private const int c_extraWidth = 2; // Il doit y avoir 2 uniter de libre autour de la room, 1 pour mur et 1 de plus pour permettre un mini corridor entre 2 pièces
        private const int c_SectionDoorWidth = 1; 

        private Vector2Int m_boundariesMap;

        // Section0
        private List<RoomInfo> m_randomRoomPoolSection0;
        private int m_numberRandomRoomSection0;
        private int m_numberofTrySection0;
        private List<RoomInfo> m_mandatoryRoomPoolSection0;
        // TopLeft
        private List<RoomInfo> m_randomRoomPoolTopLeft;
        private int m_numberRandomRoomTopLeft;
        private int m_numberofTryTopLeft;
        private List<RoomInfo> m_mandatoryRoomPoolTopLeft;
        // TopRight
        private List<RoomInfo> m_randomRoomPoolTopRight;
        private int m_numberRandomRoomTopRight;
        private int m_numberofTryTopRight;
        private List<RoomInfo> m_mandatoryRoomPoolTopRight;
        // BottomLeft
        private List<RoomInfo> m_randomRoomPoolBottomLeft;
        private int m_numberRandomRoomBottomLeft;
        private int m_numberofTryBottomLeft;
        private List<RoomInfo> m_mandatoryRoomPoolBottomLeft;
        // BottomRight
        private List<RoomInfo> m_randomRoomPoolBottomRight;
        private int m_numberRandomRoomBottomRight;
        private int m_numberofTryBottomRight;
        private List<RoomInfo> m_mandatoryRoomPoolBottomRight;

        private Grid m_grid;

        LogicGrid m_masterLogicGrid;

        private int m_wallThickness = 1;

        private List<RoomInfo> m_instantiateRoom = new List<RoomInfo>();

        public RoomManager(dataStruct.WorldGeneratorToRoomManager data)
        {
            m_boundariesMap = data.BoundariesMap;
            m_randomRoomPoolSection0 = data.RandomRoomPoolSection0;
            m_numberRandomRoomSection0 = data.NumberRandomRoomSection0;
            m_numberofTrySection0 = data.NumberofTrySection0;
            m_mandatoryRoomPoolSection0 = data.MandatoryRoomPoolSection0;
            m_randomRoomPoolTopLeft = data.RandomRoomPoolTopLeft;
            m_numberRandomRoomTopLeft = data.NumberRandomRoomTopLeft;
            m_numberofTryTopLeft = data.NumberofTryTopLeft;
            m_mandatoryRoomPoolTopLeft = data.MandatoryRoomPoolTopLeft;
            m_randomRoomPoolTopRight = data.RandomRoomPoolTopRight;
            m_numberRandomRoomTopRight = data.NumberRandomRoomTopRight;
            m_numberofTryTopRight = data.NumberofTryTopRight;
            m_mandatoryRoomPoolTopRight = data.MandatoryRoomPoolTopRight;
            m_randomRoomPoolBottomLeft = data.RandomRoomPoolBottomLeft;
            m_numberRandomRoomBottomLeft = data.NumberRandomRoomBottomLeft;
            m_numberofTryBottomLeft = data.NumberofTryBottomLeft;
            m_mandatoryRoomPoolBottomLeft = data.MandatoryRoomPoolBottomLeft;
            m_randomRoomPoolBottomRight = data.RandomRoomPoolBottomRight;
            m_numberRandomRoomBottomRight = data.NumberRandomRoomBottomRight;
            m_numberofTryBottomRight = data.NumberofTryBottomRight;
            m_mandatoryRoomPoolBottomRight = data.MandatoryRoomPoolBottomRight;
            m_grid = data.Grid;
            m_wallThickness = data.WallThickness;
        }

        public LogicGrid GenerateRoomGrid()
        {
            m_masterLogicGrid = new ubv.common.world.LogicGrid(m_boundariesMap.x, m_boundariesMap.y);

            Vector3 positionUnusedRoom = new Vector3(-250, -250, 0);
            foreach (RoomInfo room in m_mandatoryRoomPoolSection0)
            {
                RoomInfo myRoom = GameObject.Instantiate(room, positionUnusedRoom, Quaternion.identity, m_grid.transform);
                AddRoomToSection0(myRoom, true);
            }
            for (int i = 0; i < m_numberRandomRoomSection0; i++)
            {
                RoomInfo myRoom = GameObject.Instantiate(
                    m_randomRoomPoolSection0[Random.Range(0, m_randomRoomPoolSection0.Count)],
                    positionUnusedRoom,
                    Quaternion.identity,
                    m_grid.transform
                    );
                AddRoomToSection0(myRoom, false);
            }
            for (int i = 0; i < m_numberRandomRoomBottomLeft; i++)
            {
                RoomInfo myRoom = GameObject.Instantiate(
                    m_randomRoomPoolSection0[Random.Range(0, m_randomRoomPoolSection0.Count)],
                    positionUnusedRoom,
                    Quaternion.identity,
                    m_grid.transform
                    );
                AddRoomToBottomLeft(myRoom, false);
            }
            for (int i = 0; i < m_numberRandomRoomTopLeft; i++)
            {
                RoomInfo myRoom = GameObject.Instantiate(
                    m_randomRoomPoolSection0[Random.Range(0, m_randomRoomPoolSection0.Count)],
                    positionUnusedRoom,
                    Quaternion.identity,
                    m_grid.transform
                    );
                AddRoomToTopLeft(myRoom, false);
            }
            for (int i = 0; i < m_numberRandomRoomTopRight; i++)
            {
                RoomInfo myRoom = GameObject.Instantiate(
                    m_randomRoomPoolSection0[Random.Range(0, m_randomRoomPoolSection0.Count)],
                    positionUnusedRoom,
                    Quaternion.identity,
                    m_grid.transform
                    );
                AddRoomToTopRight(myRoom, false);
            }
            for (int i = 0; i < m_numberRandomRoomBottomRight; i++)
            {
                RoomInfo myRoom = GameObject.Instantiate(
                    m_randomRoomPoolSection0[Random.Range(0, m_randomRoomPoolSection0.Count)],
                    positionUnusedRoom,
                    Quaternion.identity,
                    m_grid.transform
                    );
                AddRoomToBottomRight(myRoom, false);
            }

            return m_masterLogicGrid;
        }

        public LogicGrid AddOneRoom()
        {
            m_masterLogicGrid = new ubv.common.world.LogicGrid(m_boundariesMap.x, m_boundariesMap.y);
            RoomInfo myRoom = GameObject.Instantiate(m_randomRoomPoolSection0[0], new Vector3(6, 6, 0), Quaternion.identity, m_grid.transform);
            AddToMasterGrid(myRoom, new Vector2Int(6, 6));
            return m_masterLogicGrid;
        }

        private void AddRoom(RoomInfo room, Vector2Int roomOrigin)
        {
            room.transform.position = new Vector3(roomOrigin.x, roomOrigin.y, 0);
            m_instantiateRoom.Add(room);
            AddToMasterGrid(room, roomOrigin);
        }

        private void AddToMasterGrid(RoomInfo roomInfo, Vector2Int coord)
        {
            for (int x = 0; x < roomInfo.Width; x++)
            {
                for (int y = 0; y < roomInfo.Height; y++)
                {
                    m_masterLogicGrid.Grid[x + coord.x, y + coord.y] = roomInfo.LogicGrid.Grid[x, y];
                }
            }
            // la largeur des murs autour d'une pièce est de 2
            AddVoidCell(new Vector2Int(coord.x - 1, coord.y - 1), roomInfo.Width + 4, 1);               //Section sous la room
            AddVoidCell(new Vector2Int(coord.x - 1, coord.y + roomInfo.Height), roomInfo.Width + 4, 1); //Section au dessus la room
            AddVoidCell(new Vector2Int(coord.x - 1, coord.y - 1), 1, roomInfo.Height + 1);              //Section à gauche la room
            AddVoidCell(new Vector2Int(coord.x + roomInfo.Width, coord.y), 1, roomInfo.Height + 1);     //Section à droite la room
            
        }

        private void AddVoidCell(Vector2Int coord, int width, int height)
        {
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    m_masterLogicGrid.Grid[x + coord.x, y + coord.y] = new cellType.VoidCell();
                }
            }
        }

        private void AddRoomToSection0(RoomInfo room, bool isMandatory)
        {
            Vector2Int roomOrigin;
            if (isMandatory)
            {
                roomOrigin = GetCoordInSection0(room);
            }
            else
            {
                roomOrigin = GetCoordInSection0(room, m_numberofTrySection0);
            }
            if (roomOrigin.x != -1)
            {
                AddRoom(room, roomOrigin);
                return;
            }
        }


        private void AddRoomToTopLeft(RoomInfo room, bool isMandatory)
        {
            Vector2Int roomOrigin;
            if (isMandatory)
            {
                roomOrigin = GetCoordInTopLeft(room);
            }
            else
            {
                roomOrigin = GetCoordInTopLeft(room, m_numberofTryTopLeft);
            }
            if (roomOrigin.x != -1)
            {
                AddRoom(room, roomOrigin);
                return;
            }
        }

        private void AddRoomToTopRight(RoomInfo room, bool isMandatory)
        {
            Vector2Int roomOrigin;
            if (isMandatory)
            {
                roomOrigin = GetCoordInTopRight(room);
            }
            else
            {
                roomOrigin = GetCoordInTopRight(room, m_numberofTryTopRight);
            }
            if (roomOrigin.x != -1)
            {
                AddRoom(room, roomOrigin);
                return;
            }
        }

        private void AddRoomToBottomLeft(RoomInfo room, bool isMandatory)
        {
            Vector2Int roomOrigin;
            if (isMandatory)
            {
                roomOrigin = GetCoordInBottomLeft(room);
            }
            else
            {
                roomOrigin = GetCoordInBottomLeft(room, m_numberofTryBottomLeft);
            }
            if (roomOrigin.x != -1)
            {
                AddRoom(room, roomOrigin);
                return;
            }
        }

        private void AddRoomToBottomRight(RoomInfo room, bool isMandatory)
        {
            Vector2Int roomOrigin;
            if (isMandatory)
            {
                roomOrigin = GetCoordInBottomRight(room);
            }
            else
            {
                roomOrigin = GetCoordInBottomRight(room, m_numberofTryBottomRight);
            }
            if (roomOrigin.x != -1)
            {
                AddRoom(room, roomOrigin);
                return;
            }
        }

        private Vector2Int TryGetCoord(RoomInfo roomInfo, BoxInfo bigBox, BoxInfo smallBox, int nbrTry)
        {
            Vector2Int coordTry;
            for (int i = 0; i < nbrTry; i++)
            {
                coordTry = GetCoordInSection(bigBox, smallBox);
                if (SpaceIsFree(roomInfo, coordTry))
                {
                    return coordTry;
                }
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInSection0(RoomInfo roomInfo, int nbrTry = c_mandatoryTry)
        {
            int width = (m_boundariesMap.x / 3) - 2 * c_extraWidth;
            int height = (m_boundariesMap.y / 3) - 2 * c_extraWidth;
            if(width - roomInfo.Width < 1 || height - roomInfo.Height < 1)
            {
                if(nbrTry == c_mandatoryTry)
                {
                    Debug.Log("MAP CREATION ALERT : SECTION0 to small for mandatory room, look your sizing");
                }
                return new Vector2Int(-1, -1);
            }
            BoxInfo box = new BoxInfo(m_boundariesMap.x / 3 + m_wallThickness,
                                      m_boundariesMap.y / 3 + m_wallThickness, 
                                      width - 2 * m_wallThickness - roomInfo.Width, 
                                      height- 2 * m_wallThickness - roomInfo.Height);
            Vector2Int coordTry;
            for (int i = 0; i < nbrTry; i++)
            {
                coordTry = GetRandomBoxCoord(box);
                if (SpaceIsFree(roomInfo, coordTry))
                {
                    return coordTry;
                }
            }
            if (nbrTry == c_mandatoryTry)
            {
                Debug.Log("MAP CREATION ALERT : Was not able to fit in SECTION0 mandatory room, look your sizing");
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInTopLeft(RoomInfo roomInfo, int nbrTry = c_mandatoryTry)
        {
            int bigWidth = (m_boundariesMap.x / 3) - 2 * c_extraWidth;
            int bigHeigth = (m_boundariesMap.y / 2) - 2 * c_extraWidth;
            if (bigWidth - roomInfo.Width < 1 || bigHeigth - roomInfo.Height < 1)
            {
                if (nbrTry == c_mandatoryTry)
                {
                    Debug.Log("MAP CREATION ALERT : TopLeft to small for mandatory room, look your sizing");
                }
                return new Vector2Int(-1, -1);
            }
            int SB_w = m_boundariesMap.x / 3 - m_wallThickness - roomInfo.Width;
            int SB_h = m_boundariesMap.y / 6 + m_wallThickness;
            int BB_w = m_boundariesMap.x / 2 - m_wallThickness - roomInfo.Width;
            int BB_h = m_boundariesMap.y / 2 - m_wallThickness - roomInfo.Height - SB_h;
            BoxInfo bigBox = new BoxInfo(0, 
                                         m_boundariesMap.y / 2 + SB_h + c_SectionDoorWidth,
                                         BB_w, 
                                         BB_h);
            BoxInfo smallBox = new BoxInfo(0,
                                           m_boundariesMap.y / 2 + c_SectionDoorWidth,
                                           SB_w,
                                           SB_h);
            Vector2Int coord = TryGetCoord(roomInfo, bigBox, smallBox, nbrTry);
            if (coord.x != -1)
            {
                return coord;
            }
            else if (nbrTry == c_mandatoryTry)
            {
                Debug.Log("MAP CREATION ALERT : Was not able to fit in TOPLEFT mandatory room, look your sizing");
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInTopRight(RoomInfo roomInfo, int nbrTry = c_mandatoryTry)
        {
            int bigWidth = (m_boundariesMap.x / 3) - c_extraWidth;
            int bigHeigth = (m_boundariesMap.y / 2) - c_extraWidth;
            if (bigWidth - roomInfo.Width < 1 || bigHeigth - roomInfo.Height < 1)
            {
                if (nbrTry == c_mandatoryTry)
                {
                    Debug.Log("MAP CREATION ALERT : TopRight to small for mandatory room, look your sizing");
                }
                return new Vector2Int(-1, -1);
            }
            int SB_x = m_boundariesMap.x / 6 + m_wallThickness;
            int SB_y = m_boundariesMap.y / 3 - 2 * m_wallThickness - roomInfo.Height;
            int BB_x = m_boundariesMap.x / 2 - m_wallThickness - roomInfo.Width - SB_x;
            int BB_y = m_boundariesMap.y / 2 - m_wallThickness - roomInfo.Height;
            BoxInfo bigBox = new BoxInfo(m_boundariesMap.x / 2 + SB_x + c_SectionDoorWidth,
                                         m_boundariesMap.y / 2 + c_SectionDoorWidth, 
                                         BB_x, 
                                         BB_y);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 2 + c_SectionDoorWidth, 
                                           m_boundariesMap.y * 2 / 3 + m_wallThickness + c_SectionDoorWidth, 
                                           SB_x, 
                                           SB_y);
            Vector2Int coord = TryGetCoord(roomInfo, bigBox, smallBox, nbrTry);
            if (coord.x != -1)
            {
                return coord;
            }
            else if (nbrTry == c_mandatoryTry)
            {
                Debug.Log("MAP CREATION ALERT : Was not able to fit in TOPRIGHT mandatory room, look your sizing");
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInBottomLeft(RoomInfo roomInfo, int nbrTry = c_mandatoryTry)
        {
            int bigWidth = (m_boundariesMap.x / 3) - c_extraWidth;
            int bigHeigth = (m_boundariesMap.y / 2) - c_extraWidth;
            if (bigWidth - roomInfo.Width < 1 || bigHeigth - roomInfo.Height < 1)
            {
                if (nbrTry == c_mandatoryTry)
                {
                    Debug.Log("MAP CREATION ALERT : BottomLeft to small for mandatory room, look your sizing");
                }
                return new Vector2Int(-1, -1);
            }
            int BB_x = m_boundariesMap.x / 3 - m_wallThickness - roomInfo.Width;
            int BB_y = m_boundariesMap.y / 2 - m_wallThickness - roomInfo.Height;
            int SB_x = m_boundariesMap.x / 2 - m_wallThickness - roomInfo.Width - BB_x;
            int SB_y = m_boundariesMap.y / 3 - m_wallThickness - roomInfo.Height;
            BoxInfo bigBox = new BoxInfo(0, 
                                         0, 
                                         BB_x,
                                         BB_y);
            BoxInfo smallBox = new BoxInfo(BB_x, 
                                           0, 
                                           SB_x, 
                                           SB_y);
            Vector2Int coord = TryGetCoord(roomInfo, bigBox, smallBox, nbrTry);
            if (coord.x != -1)
            {
                return coord;
            }
            else if (nbrTry == c_mandatoryTry)
            {
                Debug.Log("MAP CREATION ALERT : Was not able to fit in BOTTOMLEFT mandatory room, look your sizing");
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInBottomRight(RoomInfo roomInfo, int nbrTry = c_mandatoryTry)
        {
            int bigWidth = (m_boundariesMap.x / 3) - roomInfo.Width - c_extraWidth;
            int bigHeigth = (m_boundariesMap.y / 2) - roomInfo.Height - c_extraWidth;
            if (bigWidth - roomInfo.Width < 1 || bigHeigth - roomInfo.Height < 1)
            {
                if (nbrTry == c_mandatoryTry)
                {
                    Debug.Log("MAP CREATION ALERT : BottomRight to small for mandatory room, look your sizing");
                }
                return new Vector2Int(-1, -1);
            }
            int SB_x = m_boundariesMap.x / 6 + m_wallThickness;
            int SB_y = m_boundariesMap.y / 3 - m_wallThickness - roomInfo.Height;
            int BB_x = m_boundariesMap.x / 2 - m_wallThickness - roomInfo.Width - SB_x;
            int BB_y = m_boundariesMap.y / 2 - m_wallThickness - roomInfo.Height;
            BoxInfo bigBox = new BoxInfo(m_boundariesMap.x / 2 + SB_x + c_SectionDoorWidth, 
                                         0, 
                                         BB_x, 
                                         BB_y);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 2 + c_SectionDoorWidth, 
                                           0, 
                                           SB_x, 
                                           SB_y);
            Vector2Int coord = TryGetCoord(roomInfo, bigBox, smallBox, nbrTry);
            if (coord.x != -1)
            {
                return coord;
            }
            else if (nbrTry == c_mandatoryTry)
            {
                Debug.Log("MAP CREATION ALERT : Was not able to fit in BOTTOMRIGHT mandatory room, look your sizing");
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInSection(BoxInfo bigBox, BoxInfo smallBox)
        {

            if (Random.Range(0, (bigBox.GetArea() + smallBox.GetArea())) < bigBox.GetArea())
            {
                return GetRandomBoxCoord(bigBox);
            }
            else
            {
                return GetRandomBoxCoord(smallBox);
            }

        }

        private Vector2Int GetRandomBoxCoord(BoxInfo box)
        {
            return new Vector2Int(
                    Random.Range(box.GetOrigin().x, box.GetOrigin().x + box.GetWidth()),
                    Random.Range(box.GetOrigin().y, box.GetOrigin().y + box.GetHeight())
                    );
        }

        
        private bool SpaceIsFree(RoomInfo roomInfo, Vector2Int coord)
        {
            for (int x = coord.x - c_extraWidth; x < coord.x + roomInfo.Width + c_extraWidth; x++) 
            {
                for (int y = coord.y - c_extraWidth; y < coord.y + roomInfo.Height + c_extraWidth; y++)
                {
                    if (x < 0 || y < 0 || m_masterLogicGrid.Grid[x, y] != null || x == m_boundariesMap.x - 1 || y == m_boundariesMap.y - 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public List<RoomInfo> GetRoomInMap()
        {
            return m_instantiateRoom;
        }
    }
}
