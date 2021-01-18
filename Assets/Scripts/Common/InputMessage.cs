using UnityEngine;
using System.Collections.Generic;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public class InputFrame : udp.Serializable
            {
                public udp.SerializableTypes.Bool Sprinting;
                public udp.SerializableTypes.Vector2 Movement;
                public udp.SerializableTypes.Uint32 Tick;

                public void SetToNeutral()
                {
                    Movement.Set(Vector2.zero);
                    Sprinting.Set(false);
                }

                protected override void InitSerializableMembers()
                {
                    Sprinting = new udp.SerializableTypes.Bool(this, false);
                    Movement = new udp.SerializableTypes.Vector2(this, Vector2.zero);
                    Tick = new udp.SerializableTypes.Uint32(this, 0);

                    SetToNeutral();
                }

                protected override byte SerializationID()
                {
                    return (byte)Serialization.BYTE_TYPE.INPUT_FRAME;
                }
            }

            public class InputMessage : udp.Serializable
            {
                public udp.SerializableTypes.Uint32 StartTick;
                public udp.SerializableTypes.List<InputFrame> InputFrames;

                protected override void InitSerializableMembers()
                {
                    StartTick = new udp.SerializableTypes.Uint32(this, 0);
                    InputFrames = new udp.SerializableTypes.List<InputFrame>(this, new List<InputFrame>());
                }

                protected override byte SerializationID()
                {
                    return (byte)Serialization.BYTE_TYPE.INPUT_MESSAGE;
                }
            }
        }
    }
}
