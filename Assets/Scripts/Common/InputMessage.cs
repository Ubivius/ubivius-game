using UnityEngine;
using System.Collections.Generic;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public class InputFrame : serialization.Serializable
            {
                public serialization.types.Bool Sprinting;
                public serialization.types.Vector2 Movement;
                public serialization.types.Uint32 Tick;

                public void SetToNeutral()
                {
                    Movement.Set(Vector2.zero);
                    Sprinting.Set(false);
                }

                protected override void InitSerializableMembers()
                {
                    Sprinting = new serialization.types.Bool(this, false);
                    Movement = new serialization.types.Vector2(this, Vector2.zero);
                    Tick = new serialization.types.Uint32(this, 0);

                    SetToNeutral();
                }

                protected override byte SerializationID()
                {
                    return (byte)serialization.ID.BYTE_TYPE.INPUT_FRAME;
                }
            }

            public class InputMessage : serialization.Serializable
            {
                public serialization.types.Int32 PlayerID;
                public serialization.types.Uint32 StartTick;
                public serialization.types.List<InputFrame> InputFrames;

                protected override void InitSerializableMembers()
                {
                    StartTick = new serialization.types.Uint32(this, 0);
                    InputFrames = new serialization.types.List<InputFrame>(this, new List<InputFrame>());
                    PlayerID = new serialization.types.Int32(this, 0);
                }

                protected override byte SerializationID()
                {
                    return (byte)serialization.ID.BYTE_TYPE.INPUT_MESSAGE;
                }
            }
        }
    }
}
