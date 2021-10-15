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
                static PlayerShooting m_instance = null;
                
                private void Awake()
                {
                    if (m_instance == null)
                    {
                        m_instance = this;
                    }

                }
                static float m_lastShot = 0;

                static public void Execute(PlayerPrefab player, PlayerShootingSettings playerShootingSettings, Vector2 aimDirection, float deltaTime)
                {
                    if (m_lastShot > playerShootingSettings.BulletDelay)
                    {
                        GameObject bullet = Instantiate(playerShootingSettings.BulletPrefab);
                        bullet.transform.position = player.FirePoint.transform.position;
                        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

                        Vector3 shootingDirection = new Vector3(aimDirection.x, aimDirection.y, 0.0f);
                        rb.AddForce(shootingDirection * playerShootingSettings.BulletForce, ForceMode2D.Impulse);

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