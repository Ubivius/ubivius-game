using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ubv
{
    namespace common
    {
        namespace logic
        {
            /// <summary>
            /// Encapsulates player shooting computation
            /// </summary>
            public class PlayerShooting : MonoBehaviour
            {
                static public void Execute(Transform firePoint, GameObject bulletPrefab, float bulletForce)
                {
                    GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    rb.AddForce(firePoint.right * bulletForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}
