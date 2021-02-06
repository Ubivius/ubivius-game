using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour 
{
    private HealthSystem m_healthSystem;

    public void Setup(HealthSystem healthSystem) 
    {
        this.m_healthSystem = healthSystem;

        this.m_healthSystem.onHealthChanged += HealthSystemOnHealthChanged;
        UpdateHealthBar();
    }

    private void HealthSystemOnHealthChanged(object sender, System.EventArgs e) 
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar() 
    {
        transform.Find("Bar").localScale = new Vector3(m_healthSystem.GetHealthPercent(), 1);
    }
}