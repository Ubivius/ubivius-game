using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace common
    {
        namespace logic
        {
            /// <summary>
            /// Encapsulates enemy movement computation
            /// </summary>
            public class EnemyMovement
            {
                static public void Execute(Rigidbody2D rigidbody, Vector2 direction, float speed)
                {
                    rigidbody.velocity = direction.normalized * speed;
                }
            }
        }
    }
}
