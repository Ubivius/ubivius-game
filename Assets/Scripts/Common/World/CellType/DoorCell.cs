using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.common.world.cellType
{
    public enum DoorType
    {
        Standard,
        Section
    }

    public class DoorCell : LogicCell
    {
        private serialization.types.Int32 m_doorType;
        private serialization.types.Bool m_IsClosed;

        protected override void InitSerializableMembers()
        {
            base.InitSerializableMembers();
            m_IsClosed = new serialization.types.Bool(this, false);
            m_doorType = new serialization.types.Int32(this, (int)DoorType.Standard);
        }

        public DoorCell(DoorType doorType)
        {
            IsWalkable = true;
            DoorType = doorType;
        }

        public void CloseDoor()
        {
            m_IsClosed.Set(true);
            IsWalkable = false;
        }

        public void OpenDoor()
        {
            m_IsClosed.Set(false);
            IsWalkable = true;
        }
        public DoorType DoorType { get => (DoorType)m_doorType.Value; private set => m_doorType.Set((int)value); }

        protected override byte SerializationID()
        {
            return (byte)serialization.ID.BYTE_TYPE.LOGIC_CELL_DOOR;
        }
    }
}
