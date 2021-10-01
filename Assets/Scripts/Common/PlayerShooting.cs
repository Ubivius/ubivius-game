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
                static float m_lastShot = 0;

                static public void Execute(Transform firePoint, PlayerShootingSettings playerShootingSettings, common.data.InputFrame input, float deltaTime)
                {
                    if (input.Shooting.Value && m_lastShot > playerShootingSettings.BulletDelay)
                    {
                        GameObject bullet = Instantiate(playerShootingSettings.BulletPrefab, firePoint);
                        bullet.transform.localPosition = Vector3.zero;
                        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                        rb.AddForce(firePoint.right * playerShootingSettings.BulletForce, ForceMode2D.Impulse);

                        m_lastShot = 0;
                    }
                    else
                    {
                        m_lastShot += deltaTime;
                    }
                }
            }
        }
    }
}
