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
}
