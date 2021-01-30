using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.common.world.cellType
{
    abstract public class LogicCell
    {
        private bool m_isWalkable;

        public bool IsWalkable { get => m_isWalkable; set => m_isWalkable = value; }
    }
}
