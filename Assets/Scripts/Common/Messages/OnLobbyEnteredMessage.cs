using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class OnLobbyEnteredMessage : Serializable
    {
        public serialization.types.String CharacterID;

        public OnLobbyEnteredMessage()
        {
            CharacterID = new serialization.types.String("");
            InitSerializableMembers(CharacterID);
        }

        public OnLobbyEnteredMessage(string characterID)
        {
            CharacterID = new serialization.types.String(characterID);
            InitSerializableMembers(CharacterID);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOBBY_ENTER_MESSAGE;
        }
    }
}
