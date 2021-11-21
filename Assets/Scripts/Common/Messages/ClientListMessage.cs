using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using System.Collections.Generic;
using ubv.common.serialization.types;

namespace ubv.common.data
{
    public class StringHashMap : HashMap<String>
    {
        public StringHashMap(Dictionary<int, String> strings) : base(strings)
        { }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.HASHMAP_INT_STRING;
        }
    }

    public class BoolHashMap : HashMap<Bool>
    {
        public BoolHashMap(Dictionary<int, Bool> flags) : base(flags)
        { }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.HASHMAP_INT_BOOL;
        }
    }

    public class CharacterListMessage : serialization.Serializable
    {
        public StringHashMap PlayerCharacters { get; protected set; }

        public CharacterListMessage()
        {
            PlayerCharacters = new StringHashMap(null);

            InitSerializableMembers(PlayerCharacters);
        }

        public CharacterListMessage(Dictionary<int, String> players) : base()
        {
            PlayerCharacters = new StringHashMap(players);

            InitSerializableMembers(PlayerCharacters);
        }
                
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CHARACTER_LIST_MESSAGE;
        }
    }

    public class ClientStatusMessage : serialization.Serializable
    {
        public BoolHashMap ClientReadyStatus { get; protected set; }

        public ClientStatusMessage() : base()
        {
            ClientReadyStatus = new BoolHashMap(null);
            InitSerializableMembers(ClientReadyStatus);
        }


        public ClientStatusMessage(Dictionary<int, Bool> players) : base()
        {
            ClientReadyStatus = new BoolHashMap(players);
            InitSerializableMembers(ClientReadyStatus);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_STATUS_MESSAGE;
        }
    }
}
