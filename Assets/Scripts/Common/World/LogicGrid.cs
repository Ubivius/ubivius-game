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
    public class LogicGrid
    {
        private cellType.LogicCell[,] m_grid;

        public class CellInfo2DArray : serialization.types.Array2D<CellInfo>
        {
            public CellInfo2DArray(CellInfo[,] array) : base(array)
            { }
            
            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.ARRAY2D_CELLINFO;
            }
        }
        
        public LogicGrid(int x, int y) : base()
        {
            Grid = new cellType.LogicCell[x, y];
        }
        
        public LogicGrid(CellInfo[,] infoGrid)
        {
            int width = infoGrid.GetLength(0);
            int height = infoGrid.GetLength(1);
            Grid = new LogicCell[width, height];

            Dictionary<int, LogicCell> cells = new Dictionary<int, LogicCell>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Grid[x, y] = infoGrid[x, y].CellFromBytes();
                    if (Grid[x, y] != null)
                    {
                        cells[Grid[x, y].GetCellID()] = Grid[x, y];
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Grid[x, y] is DoorButtonCell doorButton)
                    {
                        if (cells[doorButton.GetLinkedDoorID()] is DoorCell door)
                        {
                            doorButton.SetLinkedDoor(door);
                        }
                    }
                }
            }
        }

        public CellInfo[,] GetCellInfo()
        {
            CellInfo[,] infoGrid = new CellInfo[Grid.GetLength(0), Grid.GetLength(1)];

            for(int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    infoGrid[x, y] = new CellInfo(Grid[x, y]);
                }
            }

            return infoGrid;
        }

        public LogicCell[,] Grid { get => m_grid; private set => m_grid = value; }
        
    }
}
