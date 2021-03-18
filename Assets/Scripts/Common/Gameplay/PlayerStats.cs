using UnityEngine;
using UnityEditor;

namespace ubv.common.gameplay
{
    public class PlayerStats : MonoBehaviour
    {
        public PlayerStat Health;
        public PlayerStat WalkingVelocity;
        public PlayerStat RunningMultiplier;

        public void Init()
        {
            Health.SetToMax();
            WalkingVelocity.SetToMax();
            RunningMultiplier.SetToMax();
        }
        
    }
}
