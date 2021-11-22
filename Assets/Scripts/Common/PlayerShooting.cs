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
                static private Dictionary<PlayerPrefab, float> m_playerLastShot = new Dictionary<PlayerPrefab, float>();

                static public void Execute(PlayerPrefab player, PlayerShootingSettings playerShootingSettings, Vector2 aimDirection, float deltaTime)
                {
                    if (!m_playerLastShot.ContainsKey(player))
                    {
                        m_playerLastShot.Add(player, playerShootingSettings.BulletDelay + 1);
                    }

                    if (m_playerLastShot[player] > playerShootingSettings.BulletDelay)
                    {
                        Vector3 shootingDirection = new Vector3(aimDirection.x, aimDirection.y, 0.0f);
                        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, shootingDirection, playerShootingSettings.MaxShootingDist, ~LayerMask.GetMask("Players"));

                        if (hit.collider != null)
                        {
                            Hittable hittable = hit.collider.GetComponent<Hittable>();
                            if (hittable != null)
                            {
                                hittable.OnHit();
                            }
                        }
                        else
                        {
                            hit.point = player.transform.position + (shootingDirection.normalized * playerShootingSettings.MaxShootingDist);
                        }

                        // animation de gun ou qqchose vient se plug ici ?
                        player.PlayerAnimator.Attack();

                        Debug.DrawLine(player.transform.position, hit.point, Color.green, 0.25f);

                        m_playerLastShot[player] = 0;
                    }
                    else
                    {
                        m_playerLastShot[player] += deltaTime;
                    }
                }
            }
        }
    }
}