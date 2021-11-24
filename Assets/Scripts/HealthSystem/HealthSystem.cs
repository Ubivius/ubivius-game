using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem
{
    public UnityAction OnHealthChanged;
    public UnityAction OnDead;

    private int m_healthMax;
    private int m_health;

    public bool IsDead
    {
        get { return m_health <= 0; }
    }

    public HealthSystem(int healthMax) 
    {
        m_healthMax = healthMax;
        m_health = healthMax;
    }

    public int GetHealthPoint()
    {
        return m_health;
    }
        
    public float GetHealthPercent() 
    {
        return (float)m_health / m_healthMax;
    }

    public void SetHealthPoint(int healthPoint)
    {
        m_health = healthPoint;
    }

    public void Damage(int amount) 
    {
        if (IsDead)
        {
            return;
        }

        m_health -= amount;
        if (m_health < 0) 
        {
            m_health = 0;
        }

        OnHealthChanged?.Invoke();

        if (m_health <= 0) 
        {
            Die();
        }
    }

    public void Die() 
    {
        OnDead?.Invoke();
    }

    public void Heal(int amount) 
    {
        Debug.Log("Dans HealhSystem");
        m_health += amount;
        if (m_health > m_healthMax) 
        {
            m_health = m_healthMax;
        }

        OnHealthChanged?.Invoke();
    }
}
