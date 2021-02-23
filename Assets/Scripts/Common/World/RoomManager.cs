using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace ubv.common.world
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

        private ubv.common.world.LogicGrid m_masterLogicGrid;

        public RoomManager(dataStruct.WorldGeneratorToRoomManager data)
        {
            m_boundariesMap = data.boundariesMap;
            m_randomRoomPoolSection0 = data.randomRoomPoolSection0;
            m_numberRandomRoomSection0 = data.numberRandomRoomSection0;
            m_numberofTrySection0 = data.numberofTrySection0;
            m_mandatoryRoomPoolSection0 = data.mandatoryRoomPoolSection0;
            m_randomRoomPoolTopLeft = data.randomRoomPoolTopLeft;
            m_numberRandomRoomTopLeft = data.numberRandomRoomTopLeft;
            m_numberofTryTopLeft = data.numberofTryTopLeft;
            m_mandatoryRoomPoolTopLeft = data.mandatoryRoomPoolTopLeft;
            m_randomRoomPoolTopRight = data.randomRoomPoolTopRight;
            m_numberRandomRoomTopRight = data.numberRandomRoomTopRight;
            m_numberofTryTopRight = data.numberofTryTopRight;
            m_mandatoryRoomPoolTopRight = data.mandatoryRoomPoolTopRight;
            m_randomRoomPoolBottomLeft = data.randomRoomPoolBottomLeft;
            m_numberRandomRoomBottomLeft = data.numberRandomRoomBottomLeft;
            m_numberofTryBottomLeft = data.numberofTryBottomLeft;
            m_mandatoryRoomPoolBottomLeft = data.mandatoryRoomPoolBottomLeft;
            m_randomRoomPoolBottomRight = data.randomRoomPoolBottomRight;
            m_numberRandomRoomBottomRight = data.numberRandomRoomBottomRight;
            m_numberofTryBottomRight = data.numberofTryBottomRight;
            m_mandatoryRoomPoolBottomRight = data.mandatoryRoomPoolBottomRight;
            m_grid = data.grid;
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

        private void AddRoom(RoomInfo room, Vector2Int roomOrigin)
        {
            room.transform.position = new Vector3(roomOrigin.x, roomOrigin.y, 0);
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
            AddVoidCell(new Vector2Int(coord.x - 2, coord.y - 2), roomInfo.Width + 4, 2);               //Section sous la room
            AddVoidCell(new Vector2Int(coord.x - 2, coord.y + roomInfo.Height), roomInfo.Width + 4, 2); //Section au dessus la room
            AddVoidCell(new Vector2Int(coord.x, coord.y - 2), 2, roomInfo.Height);                      //Section à gauche la room
            AddVoidCell(new Vector2Int(coord.x + roomInfo.Width, coord.y), 2, roomInfo.Height);         //Section à droite la room
            
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
            BoxInfo box = new BoxInfo(m_boundariesMap.x / 3, m_boundariesMap.y / 3, m_boundariesMap.x / 3, m_boundariesMap.y / 3);
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
                Debug.LogError("MAP CREATION ALERT : Was not able to fit in SECTION0 mandatory room, look your sizing");
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInTopLeft(RoomInfo roomInfo, int nbrTry = c_mandatoryTry)
        {
            BoxInfo bigBox = new BoxInfo(0, m_boundariesMap.y / 2, m_boundariesMap.x / 3, m_boundariesMap.y / 2);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 3, m_boundariesMap.y / 3, m_boundariesMap.x / 6, m_boundariesMap.y / 3);
            Vector2Int coord = TryGetCoord(roomInfo, bigBox, smallBox, nbrTry);
            if (coord.x != -1)
            {
                return coord;
            }
            else if (nbrTry == c_mandatoryTry)
            {
                Debug.LogError("MAP CREATION ALERT : Was not able to fit in TOPLEFT mandatory room, look your sizing");
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInTopRight(RoomInfo roomInfo, int nbrTry = c_mandatoryTry)
        {
            BoxInfo bigBox = new BoxInfo(m_boundariesMap.x * 2 / 3, m_boundariesMap.y / 2, m_boundariesMap.x / 3, m_boundariesMap.y / 2);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 2, m_boundariesMap.y * 2 / 3, m_boundariesMap.x / 6, m_boundariesMap.y / 3);
            Vector2Int coord = TryGetCoord(roomInfo, bigBox, smallBox, nbrTry);
            if (coord.x != -1)
            {
                return coord;
            }
            else if (nbrTry == c_mandatoryTry)
            {
                Debug.LogError("MAP CREATION ALERT : Was not able to fit in TOPRIGHT mandatory room, look your sizing");
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInBottomLeft(RoomInfo roomInfo, int nbrTry = c_mandatoryTry)
        {
            BoxInfo bigBox = new BoxInfo(0, 0, m_boundariesMap.x / 3, m_boundariesMap.y / 2);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 3, 0, m_boundariesMap.x / 6, m_boundariesMap.y / 3);
            Vector2Int coord = TryGetCoord(roomInfo, bigBox, smallBox, nbrTry);
            if (coord.x != -1)
            {
                return coord;
            }
            else if (nbrTry == c_mandatoryTry)
            {
                Debug.LogError("MAP CREATION ALERT : Was not able to fit in BOTTOMLEFT mandatory room, look your sizing");
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetCoordInBottomRight(RoomInfo roomInfo, int nbrTry = c_mandatoryTry)
        {
            BoxInfo bigBox = new BoxInfo(m_boundariesMap.x * 2 / 3, 0, m_boundariesMap.x / 3, m_boundariesMap.y / 2);
            BoxInfo smallBox = new BoxInfo(m_boundariesMap.x / 2, 0, m_boundariesMap.x / 6, m_boundariesMap.y / 3);
            Vector2Int coord = TryGetCoord(roomInfo, bigBox, smallBox, nbrTry);
            if (coord.x != -1)
            {
                return coord;
            }
            else if (nbrTry == c_mandatoryTry)
            {
                Debug.LogError("MAP CREATION ALERT : Was not able to fit in BOTTOMRIGHT mandatory room, look your sizing");
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
            for (int x = coord.x - 6; x < coord.x + roomInfo.Width + 6; x++) // Il doit y avoir 6 uniter de libre autour de la room, 2 pour mur, 3 pour corridor et 1 pour mur
            {
                for (int y = coord.y - 6; y < coord.y + roomInfo.Height + 6; y++)
                {
                    if (x < 0 || y < 0 || m_masterLogicGrid.Grid[x, y] != null || x == m_boundariesMap.x - 1 || y == m_boundariesMap.y - 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
