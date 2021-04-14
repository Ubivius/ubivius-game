using System.Collections;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private HealthSystem m_healthSystem;
    //Passer la healthbar via seraizable

    public void Setup(HealthSystem healthSystem)
    {
        m_healthSystem = healthSystem;
        m_healthSystem.OnHealthChanged.AddListener(HealthSystemOnHealthChanged);

        UpdateHealthBar();
    }

    public void HealthSystemOnHealthChanged(int amount)
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        //transform.Find("Bar").localScale = new Vector3(m_healthSystem.GetHealthPercent(), 1);
    }
}
