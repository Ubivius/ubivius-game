using UnityEngine;
using UnityEditor;
using ubv.common.serialization;
using ubv.common.data;

namespace ubv.client.data
{
    public class ClientCacheData : Serializable
    {
        public ubv.common.serialization.types.Bool IsInGame;
        public ServerInitMessage CachedInitMessage;

        public ClientCacheData()
        {
            InitSerializableMembers(IsInGame, CachedInitMessage);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_CACHE_DATA;
        }
    }
}
