using UnityEngine;
using System.Collections;

namespace ubv.common.data
{
    /// <summary>
    /// Class reprensenting individual player state 
    /// Add here the data of a single player
    /// </summary>
    public class PlayerState : serialization.Serializable
    {
        public serialization.types.Vector2 Position;
        public serialization.types.Quaternion Rotation;
        public serialization.types.Int32 GUID;

        public PlayerState() : base() { }

        public PlayerState(PlayerState player) : base()
        {
            Position.Set(player.Position);
            Rotation.Set(player.Rotation);
            GUID.Set(player.GUID);
        }

        protected override void InitSerializableMembers()
        {
            Position = new serialization.types.Vector2(this, Vector2.zero);
            Rotation = new serialization.types.Quaternion(this, Quaternion.identity);
            GUID = new serialization.types.Int32(this, -1);
        }

        protected override byte SerializationID()
        {
            return (byte)serialization.ID.PLAYER_STATE;
        }

    }
}
