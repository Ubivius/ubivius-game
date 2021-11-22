using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class OnLobbyEnteredMessage : Serializable
    {
        public serialization.types.String StringPlayerID;
        public serialization.types.String CharacterID;

        public OnLobbyEnteredMessage()
        {
            StringPlayerID = new serialization.types.String("");
            CharacterID = new serialization.types.String("");
            InitSerializableMembers(CharacterID, StringPlayerID);
        }

        public OnLobbyEnteredMessage(string characterID, string playerID)
        {
            StringPlayerID = new serialization.types.String(playerID);
            CharacterID = new serialization.types.String(characterID);
            InitSerializableMembers(CharacterID, StringPlayerID);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOBBY_ENTER_MESSAGE;
        }
    }
}
