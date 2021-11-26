using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class ServerStatusMessage : Serializable
    {
        public enum ServerStatus
        {
            STATUS_LOBBY = 0,
            STATUS_GAME = 1,
            STATUS_CLOSED = 2
        }

        public serialization.types.Int32 PlayerID;
        public serialization.types.Bool IsInServer;
        public serialization.types.Bool AcceptsNewPlayers;
        public serialization.types.Uint32 GameStatus;
        public serialization.types.String CharacterID;
        public serialization.types.String GameChatID;

        public ServerStatusMessage(int playerID, bool inGame = false, bool acceptsNewPlayers = false, ServerStatus status = ServerStatus.STATUS_CLOSED)
        {
            PlayerID = new serialization.types.Int32(playerID);
            IsInServer = new serialization.types.Bool(inGame);
            GameStatus = new serialization.types.Uint32((uint)status);
            AcceptsNewPlayers = new serialization.types.Bool(acceptsNewPlayers);
            CharacterID = new serialization.types.String(string.Empty);
            GameChatID = new serialization.types.String(string.Empty);
            InitSerializableMembers(PlayerID, IsInServer, AcceptsNewPlayers, GameStatus, CharacterID, GameChatID);
        }

        public ServerStatusMessage()
        {
            PlayerID = new serialization.types.Int32(0);
            IsInServer = new serialization.types.Bool(false);
            AcceptsNewPlayers = new serialization.types.Bool(false);
            GameStatus = new serialization.types.Uint32((uint)ServerStatus.STATUS_CLOSED);
            CharacterID = new serialization.types.String(string.Empty);
            GameChatID = new serialization.types.String(string.Empty);
            InitSerializableMembers(PlayerID, IsInServer, AcceptsNewPlayers, GameStatus, CharacterID, GameChatID);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_STATUS_MESSAGE;
        }
    }

    public class ServerStartsMessage : Serializable
    {
        public ServerStartsMessage()
        {
            InitSerializableMembers();
        }
        
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_STARTS_MESSAGE;
        }
    }

    public class ServerEndsMessage : Serializable
    {
        public serialization.types.Int32 NumberOfKills;
        public serialization.types.Bool Win;
        public serialization.types.Int32 NumberOfDowns;
        public serialization.types.Int32 NumberOfHelps;
        public serialization.types.Float GameDuration;
        public serialization.types.Int32 PlayerScore;
        
        public ServerEndsMessage()
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
            return ID.BYTE_TYPE.SERVER_ENDS_MESSAGE;
        }
    }
}
