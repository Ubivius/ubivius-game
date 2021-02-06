using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gab {

    public class HealthBar : MonoBehaviour 
    {

        private HealthSystem m_healthSystem;

        public void Setup(HealthSystem m_healthSystem) {
            this.m_healthSystem = m_healthSystem;

            m_healthSystem.OnHealthChanged += m_healthSystem_Onm_healthChanged;
            Updatem_healthBar();
        }

        private void m_healthSystem_Onm_healthChanged(object sender, System.EventArgs e) {
            Updatem_healthBar();
        }
        private void Updatem_healthBar() {
            transform.Find("Bar").localScale = new Vector3(m_healthSystem.GetHealthPercent(), 1);
        }

    }

}