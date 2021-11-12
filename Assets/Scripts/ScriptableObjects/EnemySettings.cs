using UnityEditor;
using UnityEngine;

namespace ubv.common
{
    [CreateAssetMenu(fileName = "SimpleSettings", menuName = "ScriptableObjects/Settings/EnemySettings", order = 1)]
    public class EnemySettings : ScriptableObject
    {
        public GameObject SimpleEnemy;
    }
}
