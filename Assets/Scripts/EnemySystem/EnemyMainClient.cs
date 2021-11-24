using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ubv.client.logic;
using UnityEngine;
using ubv.server.logic.ai;
using ubv.common.gameplay.shooting;
using ubv.common.gameplay;

public class EnemyMainClient : MonoBehaviour
{
    [SerializeField] public int MaxHealthPoint = 100;
    [SerializeField] private int m_damagePoints = 25;
    [SerializeField] public PlayerAnimator playerAnimator;

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
        return (Vector2)transform.position;
    }

    private void Hit()
    {
        HealthSystem.Damage(m_damagePoints);
    }

    public void DestroySelf()
    {
        //play dead animation
    }
}
