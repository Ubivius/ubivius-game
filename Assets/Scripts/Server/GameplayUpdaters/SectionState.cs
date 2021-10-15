using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.world.cellType;

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
        }

        // TODO
        public bool UnlockSectionAvailable(DoorType type)
        {
            switch (type)
            {
                case DoorType.Section_North:
                    return _NorthEastDoorButton && _NorthWestDoorButton ? true : false;
                case DoorType.Section_East:
                    return _NorthEastDoorButton && _SouthEastDoorButton ? true : false;
                case DoorType.Section_South:
                    return _SouthEastDoorButton && _SouthWestDoorButton ? true : false;
                case DoorType.Section_West:
                    return _NorthWestDoorButton && _SouthWestDoorButton ? true : false;


                case DoorType.Section0_NorthEast:
                    return _NorthEastDoorButton;
                case DoorType.Section0_SouthEast:
                    return _SouthEastDoorButton;
                case DoorType.Section0_SouthWest:
                    return _SouthWestDoorButton;
                case DoorType.Section0_NorthWest:
                    return _NorthWestDoorButton;

                default:
                    return false;
            }
        }

        public bool UnlockFinalDoor()
        {
            if (_NorthEastButton && _NorthEastButton && _NorthEastButton && _NorthEastButton)
            {
                return true;
            }
            return false;
        }

    }
}
