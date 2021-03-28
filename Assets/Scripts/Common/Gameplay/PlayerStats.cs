using UnityEngine;
using UnityEditor;

namespace ubv.common.gameplay
{
    public class PlayerStats
    {
        public PlayerStat Health;
        public PlayerStat WalkingVelocity;
        public PlayerStat RunningMultiplier;
        
        public PlayerStats(float health, float walkingVelocity, float runningMultiplier)
        {
            Health = new PlayerStat(health);
            WalkingVelocity = new PlayerStat(walkingVelocity);
            RunningMultiplier = new PlayerStat(runningMultiplier);
        }
    }
}
