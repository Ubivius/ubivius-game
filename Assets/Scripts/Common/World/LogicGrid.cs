using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;
using ubv.common.world.cellType;
using UnityEngine;

namespace ubv.common.world
{
    public class LogicGrid : serialization.Serializable
    {
        private cellType.LogicCell[,] m_grid;
        private serialization.types.Array2D<cellType.CellInfo> m_infoGrid;

        public class CellInfo2DArray : serialization.types.Array2D<CellInfo>
        {
            public CellInfo2DArray(int width, int height) : base(width, height)
            { }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.ARRAY2D_CELLINFO;
            }
        }

        public LogicGrid() : base()
        {
            Grid = null;
            m_infoGrid = new CellInfo2DArray(0, 0);
            InitSerializableMembers(m_infoGrid);
        }

        public LogicGrid(int x, int y) : base()
        {
            Grid = new cellType.LogicCell[x, y];
            m_infoGrid = new CellInfo2DArray(x, y);
            InitSerializableMembers(m_infoGrid);
        }

        public void CreateGridFromCellInfo()
        {

        }

        public void CreateCellInfoFromGrid()
        {

        }

        public LogicCell[,] Grid { get => m_grid; private set => m_grid = value; }
        
        protected override ID.BYTE_TYPE SerializationID()
        {
            return  ID.BYTE_TYPE.LOGIC_GRID;
        }
    }
}
