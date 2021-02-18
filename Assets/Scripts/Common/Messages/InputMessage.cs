using UnityEngine;
using System.Collections.Generic;
using ubv.common.serialization;

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

                public InputFrame()
                {
                    Sprinting = new serialization.types.Bool(false);
                    Movement = new serialization.types.Vector2(Vector2.zero);
                    Tick = new serialization.types.Uint32(0);

                    InitSerializableMembers(Sprinting, Movement, Tick);
                }

                public InputFrame(bool sprinting, Vector2 movement, uint tick)
                {
                    Sprinting = new serialization.types.Bool(sprinting);
                    Movement = new serialization.types.Vector2(movement);
                    Tick = new serialization.types.Uint32(tick);

                    InitSerializableMembers(Sprinting, Movement, Tick);
                }
                

                public void SetToNeutral()
                {
                    Movement.Value = Vector2.zero;
                    Sprinting.Value = false;
                }
                
                protected override ID.BYTE_TYPE SerializationID()
                {
                    return  ID.BYTE_TYPE.INPUT_FRAME;
                }
            }

            public class InputMessage : Serializable
            {
                public class InputFrameList : serialization.types.List<InputFrame>
                {
                    public InputFrameList(List<InputFrame> list) : base(list)  { }

                    protected override ID.BYTE_TYPE SerializationID()
                    {
                        return ID.BYTE_TYPE.LIST_INPUTFRAME;
                    }
                }

                public serialization.types.Int32 PlayerID;
                public serialization.types.Uint32 StartTick;
                public serialization.types.List<InputFrame> InputFrames;

                public InputMessage()
                {
                    StartTick = new serialization.types.Uint32(0);
                    InputFrames = new InputFrameList(new List<InputFrame>());
                    PlayerID = new serialization.types.Int32(0);

                    InitSerializableMembers(StartTick, InputFrames, PlayerID);
                }

                public InputMessage(uint startTick, List<InputFrame> frames, int id)
                {
                    StartTick = new serialization.types.Uint32(startTick);
                    InputFrames = new InputFrameList(frames);
                    PlayerID = new serialization.types.Int32(id);

                    InitSerializableMembers(StartTick, InputFrames, PlayerID);

                }

                protected override ID.BYTE_TYPE SerializationID()
                {
                    return  ID.BYTE_TYPE.INPUT_MESSAGE;
                }
            }
        }
    }
}
