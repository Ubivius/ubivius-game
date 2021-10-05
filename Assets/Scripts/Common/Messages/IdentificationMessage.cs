using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public class IdentificationMessage : Serializable
            {
                public IdentificationMessage()
                {
                    InitSerializableMembers();
                }
                
                protected override ID.BYTE_TYPE SerializationID()
                {
                    return ID.BYTE_TYPE.IDENTIFICATION_MESSAGE;
                }
            }
        }
    }
}
