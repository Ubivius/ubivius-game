using UnityEngine;
using System.Collections;

namespace ubv.common.gameplay
{
    [System.Serializable]
    public class PlayerStat
    {
        [SerializeField] public float Max;
        public float Value { get; private set; }
        
        public void Reduce(float amount)
        {
            Value -= amount;
            Clamp();
        }
        public void Augment(float amount)
        {
            Value += amount;
            Clamp();
        }
        

        private void Clamp()
        {
            if(Value > Max)
            {
                Value = Max;
            }

            if(Value < 0)
            {
                Value = 0;
            }
        }

        public float Percentage()
        {
            return Value / Max;
        }

        public void SetToMax()
        {
            Value = Max;
        }
    }
}
