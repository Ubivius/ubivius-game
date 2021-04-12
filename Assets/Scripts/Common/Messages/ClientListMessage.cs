using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using System.Collections.Generic;

namespace ubv.common.data
{
    public class CharacterListMessage : serialization.Serializable
    {
        public class CharacterIDList : serialization.types.List<serialization.types.String>
        {
            public CharacterIDList(List<serialization.types.String> players) : base(players)
            { }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.LIST_CHARACTERIDS;
            }
        }

        public CharacterIDList Characters { get; protected set; }

        public CharacterListMessage()
        {
            Characters = new CharacterIDList(null);

            InitSerializableMembers(Characters);
        }

        public CharacterListMessage(List<serialization.types.String> players) : base()
        {
            Characters = new CharacterIDList(players);

            InitSerializableMembers(Characters);
        }
                
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CHARACTER_LIST_MESSAGE;
        }
    }
}