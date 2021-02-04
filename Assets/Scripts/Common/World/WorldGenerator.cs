using System.Collections;
using System.Collections.Generic;
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

    [RequireComponent(typeof(Grid))]

    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2Int m_boundariesMap;

        // Section0
        [SerializeField] private List<GameObject> m_randomRoomPoolSection0;
        [SerializeField] private int m_numberRandomRoomSection0;
        [SerializeField] private int m_numberofTrySection0;
        [SerializeField] private List<GameObject> m_mandatoryRoomPoolSection0;
        // TopLeft
        [SerializeField] private List<GameObject> m_randomRoomPoolTopLeft;
        [SerializeField] private int m_numberRandomRoomTopLeft;
        [SerializeField] private int m_numberofTryTopLeft;
        [SerializeField] private List<GameObject> m_mandatoryRoomPoolTopLeft;
        // TopRight
        [SerializeField] private List<GameObject> m_randomRoomPoolTopRight;
        [SerializeField] private int m_numberRandomRoomTopRight;
        [SerializeField] private int m_numberofTryTopRight;
        [SerializeField] private List<GameObject> m_mandatoryRoomPoolTopRight;
        // BottomLeft
        [SerializeField] private List<GameObject> m_randomRoomPoolBottomLeft;
        [SerializeField] private int m_numberRandomRoomBottomLeft;
        [SerializeField] private int m_numberofTryBottomLeft;
        [SerializeField] private List<GameObject> m_mandatoryRoomPoolBottomLeft;
        // BottomRight
        [SerializeField] private List<GameObject> m_randomRoomPoolBottomRight;
        [SerializeField] private int m_numberRandomRoomBottomRight;
        [SerializeField] private int m_numberofTryBottomRight;
        [SerializeField] private List<GameObject> m_mandatoryRoomPoolBottomRight;

        private Grid m_grid;

        private const int c_mandatoryTry = 1000;

        private ubv.common.world.LogicGrid m_masterLogicGrid;

        private void Start()
        {
            m_grid = GetComponent<Grid>();

            m_masterLogicGrid = new ubv.common.world.LogicGrid(m_boundariesMap.x, m_boundariesMap.y);

            foreach (GameObject room in m_mandatoryRoomPoolSection0)
            {
                AddRoomToSection0(room, true);
                break;
            }
            
        }

        private void AddRoom(GameObject room, Vector2Int roomOrigin)
        {
            Instantiate(room, new Vector3(roomOrigin.x, roomOrigin.y, 0), Quaternion.identity, m_grid.transform);
            AddToMasterGrid(room.GetComponent<RoomInfo>(), roomOrigin);
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
        }

        private void AddRoomToSection0(GameObject room, bool isMandatory)
        {
            Vector2Int roomOrigin;
            if (isMandatory)
            {
                roomOrigin = GetCoordInSection0(room.GetComponent<RoomInfo>());
            }
            else 
            {
                roomOrigin = GetCoordInSection0(room.GetComponent<RoomInfo>(), m_numberofTrySection0);
            }            
            if (roomOrigin.x != -1)
            {
                AddRoom(room, roomOrigin);
                return;
            }
        }


        private void AddRoomToTopLeft(GameObject room, bool isMandatory)
        {
            Vector2Int roomOrigin;
            if (isMandatory)
            {
                roomOrigin = GetCoordInTopLeft(room.GetComponent<RoomInfo>());
            }
            else
            {
                roomOrigin = GetCoordInTopLeft(room.GetComponent<RoomInfo>(), m_numberofTryTopLeft);
            }
            if (roomOrigin.x != -1)
            {
                AddRoom(room, roomOrigin);
                return;
            }
        }

        private void AddRoomToTopRight(GameObject room, bool isMandatory)
        {
            Vector2Int roomOrigin;
            if (isMandatory)
            {
                roomOrigin = GetCoordInTopRight(room.GetComponent<RoomInfo>());
            }
            else
            {
                roomOrigin = GetCoordInTopRight(room.GetComponent<RoomInfo>(), m_numberofTryTopRight);
            }
            if (roomOrigin.x != -1)
            {
                AddRoom(room, roomOrigin);
                return;
            }
        }

        private void AddRoomToBottomLeft(GameObject room, bool isMandatory)
        {
            Vector2Int roomOrigin = GetCoordInBottomLeft(room.GetComponent<RoomInfo>());
            if (roomOrigin.x != -1)
            {
                AddRoom(room, roomOrigin);
                return;
            }
        }

        private void AddRoomToBottomRight(GameObject room)
        {
            Vector2Int roomOrigin = GetCoordInBottomRight(room.GetComponent<RoomInfo>());
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
            if(nbrTry == c_mandatoryTry)
            {
                Debug.LogError("MAP CREATION ALERT : Was not able to fit in SECTION0 mandatory room, look your sizing");
            }            
            return new Vector2Int(-1,-1);
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
            else if(nbrTry == c_mandatoryTry)
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

            if (Random.Range(0,(bigBox.GetArea() + smallBox.GetArea())) < bigBox.GetArea())
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
            for (int x = coord.x; x < coord.x + roomInfo.Width; x++)
            {
                for (int y = coord.x; y < coord.y + roomInfo.Height; y++)
                {
                    if(m_masterLogicGrid.Grid[x, y] != null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


    }
}

