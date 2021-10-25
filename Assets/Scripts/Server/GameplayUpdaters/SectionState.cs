using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.world.cellType;
using UnityEngine;

namespace ubv.server.logic
{
    class SectionState
    {
        public bool _NorthEastDoorButton;
        public bool _SouthEastDoorButton;
        public bool _SouthWestDoorButton;
        public bool _NorthWestDoorButton;

        public bool _NorthEastButton;
        public bool _SouthEastButton;
        public bool _SouthWestButton;
        public bool _NorthWestButton;

        public bool _DoorNorthEastOpened;
        public bool _DoorSouthEastOpened;
        public bool _DoorSouthWestOpened;
        public bool _DoorNorthWestOpened;
        public bool _DoorNorthOpened;
        public bool _DoorEastOpened;
        public bool _DoorSouthOpened;
        public bool _DoorWestOpened;

        public SectionState()
        {
            _NorthEastDoorButton = false;
            _SouthEastDoorButton = false;
            _SouthWestDoorButton = false;
            _NorthWestDoorButton = false;

            _NorthEastButton = false;
            _SouthEastButton = false;
            _SouthWestButton = false;
            _NorthWestButton = false;

            _DoorNorthEastOpened = false;
            _DoorSouthEastOpened = false;
            _DoorSouthWestOpened = false;
            _DoorNorthWestOpened = false;
            _DoorNorthOpened = false;
            _DoorEastOpened = false;
            _DoorSouthOpened = false;
            _DoorWestOpened = false;
        }

        // TODO
        public bool UnlockSectionAvailable(DoorType type)
        {
            switch (type)
            {
                case DoorType.Section_North:
                    return _NorthEastDoorButton && _NorthWestDoorButton && !_DoorNorthOpened ? true : false;
                case DoorType.Section_East:
                    return _NorthEastDoorButton && _SouthEastDoorButton && !_DoorEastOpened ? true : false;
                case DoorType.Section_South:
                    return _SouthEastDoorButton && _SouthWestDoorButton && !_DoorSouthOpened ? true : false;
                case DoorType.Section_West:
                    return _NorthWestDoorButton && _SouthWestDoorButton && !_DoorWestOpened ? true : false;


                case DoorType.Section0_NorthEast:
                    return _NorthEastDoorButton && !_DoorNorthEastOpened ? true : false;
                case DoorType.Section0_SouthEast:
                    return _SouthEastDoorButton && !_DoorSouthEastOpened ? true : false;
                case DoorType.Section0_SouthWest:
                    return _SouthWestDoorButton && !_DoorSouthWestOpened ? true : false;
                case DoorType.Section0_NorthWest:
                    return _NorthWestDoorButton && !_DoorNorthWestOpened ? true : false;

                default:
                    return false;
            }
        }

        public bool UnlockFinalDoor()
        {
            Debug.LogWarning("_NorthEastButton : " + _NorthEastButton);
            Debug.LogWarning("_SouthEastButton : " + _SouthEastButton);
            Debug.LogWarning("_SouthWestButton : " + _SouthWestButton);
            Debug.LogWarning("_NorthWestButton : " + _NorthWestButton);
            if (_NorthEastButton && _SouthEastButton && _SouthWestButton && _NorthWestButton)
            {
                return true;
            }
            return false;
        }

    }
}
