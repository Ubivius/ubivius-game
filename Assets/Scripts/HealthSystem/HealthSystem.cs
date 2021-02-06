using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{

    public UnityEvent onHealthChanged;
    public UnityEvent onDead;

    private int m_healthMax;
    private int m_health;

    public HealthSystem(int healthMax) 
    {
        this.m_healthMax = healthMax;
        this.m_health = healthMax;
    }
        
    public float GetHealthPercent() 
    {
        return (float)m_health / m_healthMax;
    }

    public void Damage(int amount) 
    {
        m_health -= amount;
        if (m_health < 0) 
        {
            m_health = 0;
        }

        if (onHealthChanged != null) onHealthChanged(this, EventArgs.Empty);

        if (m_health <= 0) 
        {
            Die();
        }
    }

    public void Die() 
    {
        if (onDead != null) onDead(this, EventArgs.Empty);
    }

    public void Heal(int amount) 
    {
        m_health += amount;
        if (m_health > m_healthMax) 
        {
            m_health = m_healthMax;
        }

        if (onHealthChanged != null) onHealthChanged(this, EventArgs.Empty);
    }
}