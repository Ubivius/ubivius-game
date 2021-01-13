using UnityEngine;
using System.Collections.Generic;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public class InputFrame : Serializable
            {
                public SerializableTypes.Bool Sprinting;
                public SerializableTypes.Vector2 Movement;
                public SerializableTypes.Uint32 Tick;

                public void SetToNeutral()
                {
                    Movement.Set(Vector2.zero);
                    Sprinting.Set(false);
                }

                protected override void InitSerializableMembers()
                {
                    Sprinting = new SerializableTypes.Bool(this, false);
                    Movement = new SerializableTypes.Vector2(this, Vector2.zero);
                    Tick = new SerializableTypes.Uint32(this, 0);

                    SetToNeutral();
                }

                protected override byte SerializationID()
                {
                    return (byte)Serialization.BYTE_TYPE.INPUT_FRAME;
                }
            }

            public class InputMessage : Serializable
            {
                public SerializableTypes.Uint32 StartTick;
                public SerializableTypes.List<InputFrame> InputFrames;

                protected override void InitSerializableMembers()
                {
                    StartTick = new SerializableTypes.Uint32(this, 0);
                    InputFrames = new SerializableTypes.List<InputFrame>(this, new List<InputFrame>());
                }

                protected override byte SerializationID()
                {
                    return (byte)Serialization.BYTE_TYPE.INPUT_MESSAGE;
                }
            }
        }
    }
}
