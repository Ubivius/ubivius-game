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
        private cellType.LogicCell[,] m_masterLogicGrid;

        public LogicGrid(int x, int y)
        {
            MasterLogicGrid = new cellType.LogicCell[x, y];
        }

        public LogicCell[,] MasterLogicGrid { get => m_masterLogicGrid; set => m_masterLogicGrid = value; }
    }
}
