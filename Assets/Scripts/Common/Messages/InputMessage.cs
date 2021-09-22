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
                public serialization.types.Bool Interact;
                public NetInfo Info;

                public InputFrame()
                {
                    Sprinting = new serialization.types.Bool(false);
                    Movement = new serialization.types.Vector2(Vector2.zero);
                    Interact = new serialization.types.Bool(false);
                    Info = new NetInfo(0);

                    InitSerializableMembers(Sprinting, Movement, Interact, Info);
                }

                public InputFrame(bool sprinting, Vector2 movement, long time, int tick)
                {
                    Sprinting = new serialization.types.Bool(sprinting);
                    Movement = new serialization.types.Vector2(movement);
                    Interact = new serialization.types.Bool(false);
                    Info = new NetInfo(tick);

                    InitSerializableMembers(Sprinting, Movement, Interact, Info);
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
                public serialization.types.List<InputFrame> InputFrames;

                public InputMessage()
                {
                    InputFrames = new InputFrameList(new List<InputFrame>());
                    PlayerID = new serialization.types.Int32(0);

                    InitSerializableMembers(InputFrames, PlayerID);
                }

                public InputMessage(int startTick, List<InputFrame> frames, int id)
                {
                    InputFrames = new InputFrameList(frames);
                    PlayerID = new serialization.types.Int32(id);

                    InitSerializableMembers(InputFrames, PlayerID);
                }

                protected override ID.BYTE_TYPE SerializationID()
                {
                    return  ID.BYTE_TYPE.INPUT_MESSAGE;
                }
            }
        }
    }
}
