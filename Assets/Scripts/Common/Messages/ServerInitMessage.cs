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

    /// <summary>
    /// Stats for ONE player in game
    /// </summary>
    public class ServerPlayerGameStatsMessage : serialization.Serializable
    {
        public serialization.types.Int32 NumberOfKills;
        public serialization.types.Bool Win;
        public serialization.types.Int32 NumberOfDowns;
        public serialization.types.Int32 NumberOfHelps;
        public serialization.types.Float GameDuration;
        public serialization.types.Int32 PlayerScore;

        public ServerPlayerGameStatsMessage()
        {
            NumberOfHelps = new serialization.types.Int32(0);
            NumberOfDowns = new serialization.types.Int32(0);
            NumberOfKills = new serialization.types.Int32(0);
            GameDuration = new serialization.types.Float(0);
            PlayerScore = new serialization.types.Int32(0);
            Win = new serialization.types.Bool(false);
            InitSerializableMembers(NumberOfDowns, NumberOfKills, Win, NumberOfHelps, GameDuration, PlayerScore);
        }
        
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_PLAYER_STATS_MESSAGE;
        }
    }
}
