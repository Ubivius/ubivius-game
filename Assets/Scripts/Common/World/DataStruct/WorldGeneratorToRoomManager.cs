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
        public Vector2Int boundariesMap;

        // Section0
        public List<RoomInfo> randomRoomPoolSection0;
        public int numberRandomRoomSection0;
        public int numberofTrySection0;
        public List<RoomInfo> mandatoryRoomPoolSection0;
        // TopLeft
        public List<RoomInfo> randomRoomPoolTopLeft;
        public int numberRandomRoomTopLeft;
        public int numberofTryTopLeft;
        public List<RoomInfo> mandatoryRoomPoolTopLeft;
        // TopRight
        public List<RoomInfo> randomRoomPoolTopRight;
        public int numberRandomRoomTopRight;
        public int numberofTryTopRight;
        public List<RoomInfo> mandatoryRoomPoolTopRight;
        // BottomLeft
        public List<RoomInfo> randomRoomPoolBottomLeft;
        public int numberRandomRoomBottomLeft;
        public int numberofTryBottomLeft;
        public List<RoomInfo> mandatoryRoomPoolBottomLeft;
        // BottomRight
        public List<RoomInfo> randomRoomPoolBottomRight;
        public int numberRandomRoomBottomRight;
        public int numberofTryBottomRight;
        public List<RoomInfo> mandatoryRoomPoolBottomRight;

        public Grid grid;

        public WorldGeneratorToRoomManager(
            Vector2Int boundariesMap, 
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
            Grid grid)
        {
            this.boundariesMap = boundariesMap;
            this.randomRoomPoolSection0 = randomRoomPoolSection0;
            this.numberRandomRoomSection0 = numberRandomRoomSection0;
            this.numberofTrySection0 = numberofTrySection0;
            this.mandatoryRoomPoolSection0 = mandatoryRoomPoolSection0;
            this.randomRoomPoolTopLeft = randomRoomPoolTopLeft;
            this.numberRandomRoomTopLeft = numberRandomRoomTopLeft;
            this.numberofTryTopLeft = numberofTryTopLeft;
            this.mandatoryRoomPoolTopLeft = mandatoryRoomPoolTopLeft;
            this.randomRoomPoolTopRight = randomRoomPoolTopRight;
            this.numberRandomRoomTopRight = numberRandomRoomTopRight;
            this.numberofTryTopRight = numberofTryTopRight;
            this.mandatoryRoomPoolTopRight = mandatoryRoomPoolTopRight;
            this.randomRoomPoolBottomLeft = randomRoomPoolBottomLeft;
            this.numberRandomRoomBottomLeft = numberRandomRoomBottomLeft;
            this.numberofTryBottomLeft = numberofTryBottomLeft;
            this.mandatoryRoomPoolBottomLeft = mandatoryRoomPoolBottomLeft;
            this.randomRoomPoolBottomRight = randomRoomPoolBottomRight;
            this.numberRandomRoomBottomRight = numberRandomRoomBottomRight;
            this.numberofTryBottomRight = numberofTryBottomRight;
            this.mandatoryRoomPoolBottomRight = mandatoryRoomPoolBottomRight;
            this.grid = grid;
        }
    }
}
