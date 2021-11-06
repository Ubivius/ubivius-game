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

        public ServerStatusMessage(int playerID, bool inGame = false, bool acceptsNewPlayers = false, ServerStatus status = ServerStatus.STATUS_CLOSED)
        {
            PlayerID = new serialization.types.Int32(playerID);
            IsInServer = new serialization.types.Bool(inGame);
            GameStatus = new serialization.types.Uint32((uint)status);
            AcceptsNewPlayers = new serialization.types.Bool(acceptsNewPlayers);
            InitSerializableMembers(PlayerID, IsInServer, AcceptsNewPlayers, GameStatus);
        }

        public ServerStatusMessage()
        {
            PlayerID = new serialization.types.Int32(0);
            IsInServer = new serialization.types.Bool(false);
            AcceptsNewPlayers = new serialization.types.Bool(false);
            GameStatus = new serialization.types.Uint32((uint)ServerStatus.STATUS_CLOSED);
            InitSerializableMembers(PlayerID, IsInServer, AcceptsNewPlayers, GameStatus);
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
}
