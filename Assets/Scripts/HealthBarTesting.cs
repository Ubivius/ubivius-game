using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.server.testing
{
    public class HealthBarTesting : MonoBehaviour
    {
        [SerializeField] private HealthBar m_healthBar;
        private HealthSystem m_healthSystem;

        // Start is called before the first frame update
        void Start()
        {
            m_healthSystem = new HealthSystem(100);
            m_healthBar.Setup(m_healthSystem);
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
