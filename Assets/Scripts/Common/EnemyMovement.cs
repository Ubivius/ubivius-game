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
                static public void Execute(Rigidbody2D rigidbody, Vector2 goalPosition, float speed, float reachTolerance = 0.1f)
                {
                    Vector2 delta = goalPosition - rigidbody.position;
                    Vector2 vel = Vector2.zero;
                    if(delta.sqrMagnitude > reachTolerance * reachTolerance)
                    {
                        vel = delta.normalized;
                    }

                    rigidbody.velocity = vel * speed;
                }
            }
        }
    }
}
