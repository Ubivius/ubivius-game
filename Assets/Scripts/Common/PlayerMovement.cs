using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace common
    {
        namespace logic
        {
            /// <summary>
            /// Encapsulates player movement computation
            /// </summary>
            public class PlayerMovement
            {
                static public void Execute(ref Rigidbody2D rigidbody, StandardMovementSettings movementSettings, common.data.InputFrame input, float deltaTime)
                {
                    rigidbody.velocity = input.Movement.Value * (input.Sprinting.Value ? movementSettings.SprintVelocity : movementSettings.WalkVelocity);
                }
            }
        }
    }
}
