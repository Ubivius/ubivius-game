using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;

namespace ubv.common.world.cellType
{
    public enum DoorType
    {
        Standard,
        Section_North,
        Section_East,
        Section_South,
        Section_West,
        Section0_NorthEast,
        Section0_SouthEast,
        Section0_SouthWest,
        Section0_NorthWest,
        FinalDoor
    }

    public class DoorCell : LogicCell
    {
        private serialization.types.Int32 m_doorType;
        private serialization.types.Bool m_IsClosed;

        public DoorCell(DoorType doorType) : base()
        {
            m_IsClosed = new serialization.types.Bool(false);
            m_doorType = new serialization.types.Int32((int)doorType);

            InitSerializableMembers(m_IsClosed, m_doorType);

            if (doorType == DoorType.Standard)
            {
                IsWalkable = true;
            }
            else
            {
                IsWalkable = false;
            }            
            DoorType = doorType;
        }

        public void CloseDoor()
        {
            m_IsClosed.Value = true;
            IsWalkable = false;
        }

        public void OpenDoor()
        {
            m_IsClosed.Value = (false);
            IsWalkable = true;
        }

        public DoorCell() : base()
        {
            m_IsClosed = new serialization.types.Bool(false);
            m_doorType = new serialization.types.Int32((int)DoorType.Standard);

            InitSerializableMembers(m_IsClosed, m_doorType);
        }

        public DoorType DoorType { get => (DoorType)m_doorType.Value; private set => m_doorType.Value = (int)value; }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOGIC_CELL_DOOR;
        }

        public override CellInfo.CellType GetCellType()
        {
            return CellInfo.CellType.CELL_DOOR;
        }
    }
}
