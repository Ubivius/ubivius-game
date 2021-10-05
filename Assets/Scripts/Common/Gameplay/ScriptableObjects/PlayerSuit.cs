using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.common.gameplay
{
    [CreateAssetMenu(fileName = "PlayerSuit", menuName = "ScriptableObjects/Gameplay/PlayerSuit", order = 1)]
    public class PlayerSuit : ScriptableObject
    {
        public PlayerItem MainItem;
        public PlayerItem SideItem;
        
        [SerializeField] private float m_defaultMaxHealth;
        [SerializeField] private float m_defaultWalkingVelocity;
        [SerializeField] private float m_defaultRunningMultiplier;

        public PlayerStats Stats;

        public void Init()
        {
            Stats = new PlayerStats(m_defaultMaxHealth, m_defaultWalkingVelocity, m_defaultRunningMultiplier);
        }
    }
}
