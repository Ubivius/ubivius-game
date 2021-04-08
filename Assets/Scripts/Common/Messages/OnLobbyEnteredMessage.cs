using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class OnLobbyEnteredMessage : Serializable
    {
        public OnLobbyEnteredMessage()
        {
            InitSerializableMembers();
        }
                
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOBBY_ENTER_MESSAGE;
        }
    }
}
