using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace ubv.common.world.dataStruct
{
    struct WorldGeneratorToRoomManager
    {
        public Vector2Int BoundariesMap;

        // Section0
        public List<RoomInfo> RandomRoomPoolSection0;
        public int NumberRandomRoomSection0;
        public int NumberofTrySection0;
        public List<RoomInfo> MandatoryRoomPoolSection0;
        // TopLeft
        public List<RoomInfo> RandomRoomPoolTopLeft;
        public int NumberRandomRoomTopLeft;
        public int NumberofTryTopLeft;
        public List<RoomInfo> MandatoryRoomPoolTopLeft;
        // TopRight
        public List<RoomInfo> RandomRoomPoolTopRight;
        public int NumberRandomRoomTopRight;
        public int NumberofTryTopRight;
        public List<RoomInfo> MandatoryRoomPoolTopRight;
        // BottomLeft
        public List<RoomInfo> RandomRoomPoolBottomLeft;
        public int NumberRandomRoomBottomLeft;
        public int NumberofTryBottomLeft;
        public List<RoomInfo> MandatoryRoomPoolBottomLeft;
        // BottomRight
        public List<RoomInfo> RandomRoomPoolBottomRight;
        public int NumberRandomRoomBottomRight;
        public int NumberofTryBottomRight;
        public List<RoomInfo> MandatoryRoomPoolBottomRight;

        public Grid Grid;
        public int WallThickness;

        public WorldGeneratorToRoomManager(Vector2Int boundariesMap, 
                                           List<RoomInfo> randomRoomPoolSection0, 
                                           int numberRandomRoomSection0, 
                                           int numberofTrySection0, 
                                           List<RoomInfo> mandatoryRoomPoolSection0, 
                                           List<RoomInfo> randomRoomPoolTopLeft, 
                                           int numberRandomRoomTopLeft, 
                                           int numberofTryTopLeft, 
                                           List<RoomInfo> mandatoryRoomPoolTopLeft, 
                                           List<RoomInfo> randomRoomPoolTopRight, 
                                           int numberRandomRoomTopRight, 
                                           int numberofTryTopRight, 
                                           List<RoomInfo> mandatoryRoomPoolTopRight, 
                                           List<RoomInfo> randomRoomPoolBottomLeft, 
                                           int numberRandomRoomBottomLeft, 
                                           int numberofTryBottomLeft, 
                                           List<RoomInfo> mandatoryRoomPoolBottomLeft, 
                                           List<RoomInfo> randomRoomPoolBottomRight, 
                                           int numberRandomRoomBottomRight, 
                                           int numberofTryBottomRight, 
                                           List<RoomInfo> mandatoryRoomPoolBottomRight, 
                                           Grid grid, 
                                           int wallThickness)
        {
            BoundariesMap = boundariesMap;
            RandomRoomPoolSection0 = randomRoomPoolSection0;
            NumberRandomRoomSection0 = numberRandomRoomSection0;
            NumberofTrySection0 = numberofTrySection0;
            MandatoryRoomPoolSection0 = mandatoryRoomPoolSection0;
            RandomRoomPoolTopLeft = randomRoomPoolTopLeft;
            NumberRandomRoomTopLeft = numberRandomRoomTopLeft;
            NumberofTryTopLeft = numberofTryTopLeft;
            MandatoryRoomPoolTopLeft = mandatoryRoomPoolTopLeft;
            RandomRoomPoolTopRight = randomRoomPoolTopRight;
            NumberRandomRoomTopRight = numberRandomRoomTopRight;
            NumberofTryTopRight = numberofTryTopRight;
            MandatoryRoomPoolTopRight = mandatoryRoomPoolTopRight;
            RandomRoomPoolBottomLeft = randomRoomPoolBottomLeft;
            NumberRandomRoomBottomLeft = numberRandomRoomBottomLeft;
            NumberofTryBottomLeft = numberofTryBottomLeft;
            MandatoryRoomPoolBottomLeft = mandatoryRoomPoolBottomLeft;
            RandomRoomPoolBottomRight = randomRoomPoolBottomRight;
            NumberRandomRoomBottomRight = numberRandomRoomBottomRight;
            NumberofTryBottomRight = numberofTryBottomRight;
            MandatoryRoomPoolBottomRight = mandatoryRoomPoolBottomRight;
            Grid = grid;
            WallThickness = wallThickness;
        }
    }
}
