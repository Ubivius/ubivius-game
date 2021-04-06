using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;

namespace ubv.common.world.cellType
{
    class PlayerSpawnCell : FloorCell
    {
        public PlayerSpawnCell() : base()
        {
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOGIC_CELL_PLAYERSPAWN;
        }

        public override CellInfo.CellType GetCellType()
        {
            return CellInfo.CellType.CELL_PLAYERSPAWN;
        }
    }
}
