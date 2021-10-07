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

                static public void Execute(PlayerPrefab player, PlayerShootingSettings playerShootingSettings, common.data.InputFrame input, float deltaTime)
                {
                    if (input.Shooting.Value && m_lastShot > playerShootingSettings.BulletDelay)
                    {
                        GameObject bullet = Instantiate(playerShootingSettings.BulletPrefab);
                        bullet.transform.position = player.FirePoint.transform.position;
                        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

                        Vector3 aimDirection = new Vector3(input.ShootingDirection.Value.x, input.ShootingDirection.Value.y, 0.0f);
                        rb.AddForce(aimDirection * playerShootingSettings.BulletForce, ForceMode2D.Impulse);

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