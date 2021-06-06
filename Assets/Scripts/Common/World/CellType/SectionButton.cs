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
        private Section m_section;

        public Section Section { get => m_section; private set => m_section = value; }

        public SectionButton(Section section)
        {
            Section = section;
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
