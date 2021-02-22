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
        private int m_width;
        private int m_height;

        public LogicGrid(int x, int y)
        {
            Grid = new cellType.LogicCell[x, y];
            Width = x;
            Height = y;
        }

        public LogicCell[,] Grid { get => m_grid; private set => m_grid = value; }
        public int Width { get => m_width; private set => m_width = value; }
        public int Height { get => m_height; private set => m_height = value; }
    }
}
