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
        public bool UnlockSectionAvailable(Section section)
        {
            switch(section)
            {
                case Section.NorthEast:
                    return true;
                case Section.SouthEast:
                    return true;
                case Section.SouthWest:
                    return true;
                case Section.NorthWest:
                    return true;
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
