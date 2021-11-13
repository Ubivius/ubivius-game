using UnityEngine;
using System.Collections;

namespace ubv.common.gameplay
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerSuit m_currentSuit;

        [SerializeField] private float m_defaultMaxHealth;
        [SerializeField] private float m_defaultWalkingVelocity;
        [SerializeField] private float m_defaultRunningMultiplier;

        private HealthSystem m_healthSystem;
        private PlayerStats m_defaultStats;

        private void Awake()
        {
            m_defaultStats = new PlayerStats(m_defaultMaxHealth, m_defaultWalkingVelocity, m_defaultRunningMultiplier);
            m_healthSystem = new HealthSystem((int)m_defaultMaxHealth);
        }

        public void EquipSuit(PlayerSuit suit)
        {
            m_currentSuit = suit;
            m_currentSuit.Init();
            m_currentSuit.MainItem = GameObject.Instantiate(m_currentSuit.MainItem, transform).GetComponent<PlayerItem>();
            m_currentSuit.SideItem = GameObject.Instantiate(m_currentSuit.SideItem, transform).GetComponent<PlayerItem>();
        }
            
        public PlayerStats GetStats()
        {
            return m_currentSuit ? m_currentSuit.Stats : m_defaultStats;
        }
        
        public void ActivateMainItem()
        {
            m_currentSuit?.MainItem.Activate();
        }

        public void ActivateSideItem()
        {
            m_currentSuit?.SideItem.Activate();
        }
    }
}
