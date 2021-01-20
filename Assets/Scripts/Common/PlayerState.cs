using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace common
    {
        /// <summary>
        /// Class reprensenting individual player state 
        /// Add here the data of a single player
        /// </summary>
        public class PlayerState : udp.Serializable
        {
            public udp.SerializableTypes.Uint32 ID;
            public udp.SerializableTypes.Vector2 Position;
            public udp.SerializableTypes.Quaternion Rotation;
            
            protected override void InitSerializableMembers()
            {
                ID = new udp.SerializableTypes.Uint32(this, 0); // TEMPORARY while we have no auth
                Position = new udp.SerializableTypes.Vector2(this, Vector2.zero);
                Rotation = new udp.SerializableTypes.Quaternion(this, Quaternion.identity);
            }

            protected override byte SerializationID()
            {
                return (byte)udp.Serialization.BYTE_TYPE.PLAYER_STATE;
            }
            
        }
    }
}
