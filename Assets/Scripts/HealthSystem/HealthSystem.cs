using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gab {

    public class HealthSystem {

        public event EventHandler OnHealthChanged;
        public event EventHandler OnDead;

        private int m_healthMax;
        private int m_health;

        public HealthSystem(int m_healthMax) 
        {
            this.m_healthMax = m_healthMax;
            m_healthMax = m_healthMax;
        }
        
        public float GetHealthPercent() 
        {
            return (float)m_health / m_healthMax;
        }

        public void Damage(int amount) 
        {
            m_health -= amount;
            if (m_health < 0) {
                m_health = 0;
            }
            if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);

            if (m_health <= 0) {
                Die();
            }
        }

        public void Die() 
        {
            if (OnDead != null) OnDead(this, EventArgs.Empty);
        }

        public void Heal(int amount) 
        {
            m_health += amount;
            if (m_health > m_healthMax) 
            {
                m_health = m_healthMax;
            }
            if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
        }

    }

}