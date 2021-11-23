using System.Collections;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform m_bar;

    private HealthSystem m_healthSystem;

    public void Setup(HealthSystem healthSystem)
    {
        m_healthSystem = healthSystem;
        m_healthSystem.OnHealthChanged += HealthSystemOnHealthChanged;

        UpdateHealthBar();
    }

    public void HealthSystemOnHealthChanged()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        m_bar.localScale = new Vector3(m_healthSystem.GetHealthPercent(), 1);
    }
}
