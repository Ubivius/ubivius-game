using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.world.cellType;
using UnityEngine;

namespace ubv.common.world
{
    public class LogicGrid
    {
        private Vector2Int m_boundaries;
        private cellType.LogicCell[,] m_masterLogicGrid;

        public LogicGrid()
        {
            m_masterLogicGrid = new cellType.LogicCell[m_boundaries.x,m_boundaries.y];
        }
    }
}
