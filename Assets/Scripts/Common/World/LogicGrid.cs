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
        private cellType.LogicCell[,] m_grid;

        public LogicGrid(int x, int y)
        {
            Grid = new cellType.LogicCell[x, y];
        }

        public LogicCell[,] Grid { get => m_grid; private set => m_grid = value; }
    }
}
