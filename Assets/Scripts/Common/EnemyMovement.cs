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
            public class EnemyMovement
            {
                static public void Execute(Rigidbody2D rigidbody, Vector2 targetPosition, float speed)
                {
                    rigidbody.velocity = (targetPosition - rigidbody.position).normalized * speed;
                }
            }
        }
    }
}
