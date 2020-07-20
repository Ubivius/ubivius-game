using UnityEngine;
using UnityEditor;

namespace UBV
{
    [CreateAssetMenu(fileName = "StandardMovementSettings", menuName = "ScriptableObjects/Settings/StandardMovementSettings", order = 1)]
    public class StandardMovementSettings : ScriptableObject
    {
        public float WalkVelocity;
        public float SprintVelocity;
        public float Acceleration;
    }
}