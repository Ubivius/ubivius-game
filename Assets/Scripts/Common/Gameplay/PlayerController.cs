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

        private PlayerStats m_defaultStats;

        private void Awake()
        {
            m_defaultStats = new PlayerStats(m_defaultMaxHealth, m_defaultWalkingVelocity, m_defaultRunningMultiplier);
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public PlayerStats GetStats()
        {
            return m_currentSuit ? m_currentSuit.Stats : m_defaultStats;
        }
    }
}