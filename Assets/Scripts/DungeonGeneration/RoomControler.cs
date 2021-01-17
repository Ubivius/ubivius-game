using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace ubv
{
    public class RoomInfo
    {
        public string name;
        public int X;
        public int Y;
    }

    public class RoomControler : MonoBehaviour
    {
        public static RoomControler instance;

        string currentWorldName = "level_1";

        RoomInfo currentLoadRoomData;

        Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();

        public List<Room> loadedRooms = new List<Room>();

        bool isLoadingRoom = false;

        private void Awake()
        {
            instance = this;
        }

        public bool DoesRoomExist(int x, int y)
        {
            return loadedRooms.Find(item => item.X == x && item.Y == y) != null;
        }

    }
}
