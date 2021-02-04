using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.common.world.cellType
{
    class DoorButtonCell : LogicCell
    {
        private ubv.common.world.cellType.DoorCell m_linkedDoor;

        public DoorButtonCell(DoorCell linkedDoor)
        {
            IsWalkable = false;
            m_linkedDoor = linkedDoor;
        }
        public DoorButtonCell()
        {
            IsWalkable = false;
        }
        public void CloseDoor()
        {
            m_linkedDoor.CloseDoor();
        }

        public void OpenDoor()
        {
            m_linkedDoor.OpenDoor();
        }
    }
}
