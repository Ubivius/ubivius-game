using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using System.Collections.Generic;
using static ubv.common.data.CharacterListMessage;

namespace ubv.common.data
{
    public class ServerInitMessage : serialization.Serializable
    {
        public StringHashMap PlayerCharacters { get; protected set; }
        public world.LogicGrid.CellInfo2DArray CellInfo2DArray;

        public ServerInitMessage()
        {
            PlayerCharacters = new StringHashMap(null);
            CellInfo2DArray = new world.LogicGrid.CellInfo2DArray(null);

            InitSerializableMembers(PlayerCharacters, CellInfo2DArray);
        }

        public ServerInitMessage(Dictionary<int, serialization.types.String> playersCharacters, world.cellType.CellInfo[,] array) : base()
        {
            PlayerCharacters = new StringHashMap(playersCharacters);
            CellInfo2DArray = new world.LogicGrid.CellInfo2DArray(array);

            InitSerializableMembers(PlayerCharacters, CellInfo2DArray);
        }
                
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_INIT_MESSAGE;
        }
    }
}
