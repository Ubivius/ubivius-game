using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace common
    {
        namespace logic
        {
            public class PlayerMovement
            {
                static public void Execute(ref Rigidbody2D rigidbody, StandardMovementSettings movementSettings, common.data.InputFrame input, float deltaTime)
                {
                    rigidbody.MovePosition(rigidbody.position +
                        input.Movement.Value *
                        (input.Sprinting ? movementSettings.SprintVelocity : movementSettings.WalkVelocity) *
                        deltaTime);
                }
            }
        }
    }
}