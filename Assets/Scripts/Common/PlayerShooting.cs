using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ubv.common.gameplay.shooting;

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
                        Vector3 shootingDirection = new Vector3(aimDirection.x, aimDirection.y, 0.0f);
                        RaycastHit2D[] hits = Physics2D.RaycastAll(player.transform.position, shootingDirection);

                        RaycastHit2D hit = hits[2]; // ignore the first -> player
                        if (hit.collider != null)
                        {
                            Hittable hittable = hit.collider.GetComponent<Hittable>();
                            if (hittable != null)
                            {
                                hittable.OnHit();
                            }
                            Debug.DrawLine(player.transform.position, hit.point, Color.green, 1.0f);
                        }

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