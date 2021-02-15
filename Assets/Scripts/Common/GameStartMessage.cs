using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using System.Collections.Generic;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public class GameStartMessage : serialization.Serializable
            {
                public class PlayerStateList : serialization.types.List<PlayerState>
                {
                    public PlayerStateList(List<PlayerState> players) : base(players)
                    { }

                    protected override ID.BYTE_TYPE SerializationID()
                    {
                        return ID.BYTE_TYPE.LIST_PLAYERSTATE;
                    }
                }

                public serialization.types.Int32 SimulationBuffer { get; protected set; }
                public PlayerStateList Players { get; protected set; }

                public GameStartMessage()
                {
                    SimulationBuffer = new serialization.types.Int32(0);
                    Players = new PlayerStateList(new List<PlayerState>());

                    InitSerializableMembers(SimulationBuffer, Players);
                }

                public GameStartMessage(int simulationBuffer, List<PlayerState> players) : base()
                {
                    SimulationBuffer = new serialization.types.Int32(simulationBuffer);
                    Players = new PlayerStateList(players);

                    InitSerializableMembers(SimulationBuffer, Players);
                }
                
                protected override ID.BYTE_TYPE SerializationID()
                {
                    return ID.BYTE_TYPE.START_MESSAGE;
                }
            }
        }
    }
}
