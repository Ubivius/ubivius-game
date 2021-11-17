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
                static public void Execute(ref Rigidbody2D rigidbody, Vector2 velocity)
                {
                    rigidbody.velocity = velocity;
                }

                static public Vector2 GetVelocity(Vector2 dir, bool isSprinting, gameplay.PlayerStats stats)
                {
                    return dir.normalized * stats.WalkingVelocity.Value * (isSprinting ? stats.RunningMultiplier.Value : 1f);
                }
            }
        }
    }
}
