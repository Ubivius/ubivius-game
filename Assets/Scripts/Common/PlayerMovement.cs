using UnityEngine;
using System.Collections;

namespace ubv.common.logic
{
    /// <summary>
    /// Encapsulates player movement computation
    /// </summary>
    public class PlayerMovement
    {
        static public void Execute(ref Rigidbody2D rigidbody, StandardMovementSettings movementSettings, data.InputFrame input, float deltaTime)
        {
            rigidbody.MovePosition(rigidbody.position +
                input.Movement.Value *
                (input.Sprinting ? movementSettings.SprintVelocity : movementSettings.WalkVelocity) *
                deltaTime);
        }
    }
}
