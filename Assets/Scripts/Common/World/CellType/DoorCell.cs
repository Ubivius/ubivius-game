using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.common.world.cellType
{
    public enum DoorType
    {
        Standart,
        Section
    }

    public class DoorCell : LogicCell
    {
        private DoorType m_doorType;
        private bool m_IsClosed;

        public DoorCell(DoorType doorType)
        {
            IsWalkable = true;
            DoorType = doorType;
        }

        public void CloseDoor()
        {
            m_IsClosed = true;
            IsWalkable = false;
        }

        public void OpenDoor()
        {
            m_IsClosed = false;
            IsWalkable = true;
        }
        public DoorType DoorType { get => m_doorType; set => m_doorType = value; }
    }
}
