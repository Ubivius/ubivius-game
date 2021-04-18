using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using System.Collections.Generic;
using static ubv.common.data.CharacterListMessage;

namespace ubv.common.data
{
    public class ServerInitMessage : serialization.Serializable
    {
        public serialization.types.Int32 SimulationBuffer { get; protected set; }
        public StringHashMap PlayerCharacters { get; protected set; }
        public world.LogicGrid.CellInfo2DArray CellInfo2DArray;

        public ServerInitMessage()
        {
            SimulationBuffer = new serialization.types.Int32(0);
            PlayerCharacters = new StringHashMap(null);
            CellInfo2DArray = new world.LogicGrid.CellInfo2DArray(null);

            InitSerializableMembers(SimulationBuffer, PlayerCharacters, CellInfo2DArray);
        }

        public ServerInitMessage(int simulationBuffer, Dictionary<int, serialization.types.String> playersCharacters, world.cellType.CellInfo[,] array) : base()
        {
            SimulationBuffer = new serialization.types.Int32(simulationBuffer);
            PlayerCharacters = new StringHashMap(playersCharacters);
            CellInfo2DArray = new world.LogicGrid.CellInfo2DArray(array);

            InitSerializableMembers(SimulationBuffer, PlayerCharacters, CellInfo2DArray);
        }
                
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_INIT_MESSAGE;
        }
    }
}
