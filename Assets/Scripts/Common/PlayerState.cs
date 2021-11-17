using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public enum PlayerStateEnum
            {
                IS_SPRINTING = 0,
                IS_INTERACTING = 1,
                IS_SHOOTING = 2,
                ///...
            }
            
            /// <summary>
            /// Class reprensenting individual player state 
            /// Add here the data of a single player
            /// </summary>
            public class PlayerState : serialization.Serializable
            {
                public utils.Bitset States;
                public serialization.types.Vector2 Velocity;
                public serialization.types.Vector2 Position;
                public serialization.types.Vector2 ShootingDirection;
                public serialization.types.Int32 GUID;

                public PlayerState() : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Velocity = new serialization.types.Vector2(Vector2.zero);
                    ShootingDirection = new serialization.types.Vector2(Vector2.zero);
                    GUID = new serialization.types.Int32(0);
                    States = new utils.Bitset();

                    InitSerializableMembers(Position, Velocity, ShootingDirection, GUID, States);
                }

                public PlayerState(int playerID) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Velocity = new serialization.types.Vector2(Vector2.zero);
                    ShootingDirection = new serialization.types.Vector2(Vector2.zero);
                    GUID = new serialization.types.Int32(playerID);
                    States = new utils.Bitset();

                    InitSerializableMembers(Position, Velocity, ShootingDirection, GUID, States);
                }

                public PlayerState(PlayerState player) : base()
                {
                    Position = new serialization.types.Vector2(Vector2.zero);
                    Velocity = new serialization.types.Vector2(Vector2.zero);
                    ShootingDirection = new serialization.types.Vector2(Vector2.zero);
                    GUID = new serialization.types.Int32(player.GUID.Value);
                    States = new utils.Bitset();

                    InitSerializableMembers(Position, Velocity, ShootingDirection, GUID, States);
                }
                
                protected override ID.BYTE_TYPE SerializationID()
                {
                    return ID.BYTE_TYPE.PLAYER_STATE;
                }

                public bool IsPositionDifferent(PlayerState other, float errorTolerance = 0.1f)
                {
                    if (other == this) return false;

                    if ((other.Position.Value - Position.Value).sqrMagnitude > errorTolerance * errorTolerance)
                    {
                        return true;
                    }

                    return false;
                }

                public bool IsDifferent(PlayerState other, float errorTolerance = 0.1f)
                {
                    if (other == this) return false;

                    if ((other.Position.Value - Position.Value).sqrMagnitude > errorTolerance * errorTolerance)
                    {
                        return true;
                    }

                    if ((other.Velocity.Value - Velocity.Value).sqrMagnitude > errorTolerance * errorTolerance)
                    {
                        return true;
                    }

                    if (other.States.IsDifferent(States))
                    {
                        return true;
                    }

                    return false;
                }
            }
        }
    }
}
