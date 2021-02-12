using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using ubv.common.world;

namespace ubv.common.World
{
    class CorridorsManager
    {
        private Grid m_grid;

        private ubv.common.world.LogicGrid m_masterLogicGrid;

        public CorridorsManager(Grid grid, LogicGrid masterLogicGrid)
        {
            m_grid = grid;
            m_masterLogicGrid = masterLogicGrid;
        }

        public LogicGrid GenerateCorridorsGrid()
        {


            return m_masterLogicGrid;
        }
    }
}
