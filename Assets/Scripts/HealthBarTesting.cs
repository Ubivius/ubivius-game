using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.server.testing
{
    public class HealthBarTesting : MonoBehaviour
    {
        private HealthSystem m_healthSystem;
        public Transform PfHealthBar;

        // Start is called before the first frame update
        void Start()
        {
            m_healthSystem = new HealthSystem(100);
            HealthBar healthBar = PfHealthBar.GetComponent<HealthBar>();
            healthBar.Setup(m_healthSystem);
        }

        public void Heal()
        {
            m_healthSystem.Heal(10);
        }

        public void Damage()
        {
            m_healthSystem.Damage(10);
        }
    }
}
