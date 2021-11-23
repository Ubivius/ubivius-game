using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ubv.server.logic.ai;
using ubv.common.gameplay.shooting;
using ubv.common.gameplay;

public class EnemyMainServer : MonoBehaviour
{
    [SerializeField] public int MaxHealthPoint = 100;
    [SerializeField] private int m_damagePoints = 25;
    [SerializeField] private int m_attackPoints = 10;
    public Rigidbody2D EnemyRigidbody2D { get; private set; }
    public HealthSystem HealthSystem { get; private set; }

    private void Awake()
    {
        EnemyRigidbody2D = GetComponent<Rigidbody2D>();

        HealthSystem = new HealthSystem(MaxHealthPoint);
        Hittable hit = GetComponent<Hittable>();
        hit.OnHit += Hit;

        HealthSystem.OnDead += DestroySelf;
    }

    public Vector2 GetPosition()
    {
        return (Vector2) transform.position;
    }

    private void Hit()
    {
        HealthSystem.Damage(m_damagePoints);
    }

    public void DestroySelf()
    {
        Destroy(transform.gameObject);
        //ex when we'll need to add animation
        //Destroy(GetComponent<BoxCollider>());
    }
    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Damage(m_attackPoints);
        }
    }
}
