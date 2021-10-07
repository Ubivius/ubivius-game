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

                static public void Execute(PlayerPrefab player, PlayerShootingSettings playerShootingSettings, Camera cam, common.data.InputFrame input, float deltaTime)
                {
                    if (input.Shooting.Value && m_lastShot > playerShootingSettings.BulletDelay)
                    {
                        GameObject bullet = Instantiate(playerShootingSettings.BulletPrefab);
                        bullet.transform.position = player.FirePoint.transform.position;
                        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

                        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                        Vector3 aimDir = mousePos - player.transform.position;
                        aimDir.z = 0;
                        Vector3 aimDirNorm = aimDir.normalized;

                        Debug.Log("Aim: " + aimDir + ", AimNorm: " + aimDirNorm);

                        rb.AddForce(aimDirNorm * playerShootingSettings.BulletForce, ForceMode2D.Impulse);

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