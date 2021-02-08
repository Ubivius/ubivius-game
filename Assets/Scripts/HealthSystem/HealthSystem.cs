using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{

    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDead;

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

        if (this.OnHealthChanged != null) this.OnHealthChanged.Invoke(amount);

        if (m_health <= 0) 
        {
            this.Die();
        }
    }

    public void Die() 
    {
        if (this.OnDead != null) this.OnDead.Invoke();
    }

    public void Heal(int amount) 
    {
        m_health += amount;
        if (m_health > m_healthMax) 
        {
            m_health = m_healthMax;
        }

        if (this.OnHealthChanged != null) this.OnHealthChanged.Invoke(amount);
    }
}

//Example of use
/*
public class HealthBar : MonoBehaviour
{
    private HealthSystem m_healthSystem;

    public void Setup(HealthSystem healthSystem)
    {
        m_healthSystem = healthSystem;
        m_healthSystem.OnHealthChanged.AddListener(HealthSystemOnHealthChanged);

        UpdateHealthBar();
    }

    public void HealthSystemOnHealthChanged()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        //transform.Find("Bar").localScale = new Vector3(m_healthSystem.GetHealthPercent(), 1);
    }
}*/