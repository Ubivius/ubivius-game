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
                static public void Execute(ref Rigidbody2D rigidbody, gameplay.PlayerStats stats, common.data.InputFrame input, float deltaTime)
                {
                    rigidbody.velocity = input.Movement.Value * stats.WalkingVelocity.Value * (input.Sprinting.Value ? stats.RunningMultiplier.Value : 1f);
                }
            }
        }
    }
}
