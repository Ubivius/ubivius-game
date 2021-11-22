using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;

namespace ubv.common.world.cellType
{
    class SectionButton : LogicCell
    {
        private serialization.types.Int32 m_section;

        public Section Section { get => (Section)m_section.Value; private set => m_section.Value = (int)value; }

        public SectionButton(Section section)
        {
            m_section = new serialization.types.Int32((int)section);
            InitSerializableMembers(m_section);
        }

        public SectionButton(): base()
        {
            m_section = new serialization.types.Int32((int)Section.None);
            InitSerializableMembers(m_section);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOGIC_CELL_SECTIONBUTTON;
        }

        public override CellInfo.CellType GetCellType()
        {
            return CellInfo.CellType.CELL_SECTIONBUTTON;
        }
    }
}
