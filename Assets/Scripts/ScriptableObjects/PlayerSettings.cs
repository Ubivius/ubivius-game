using UnityEngine;
using UnityEditor;

namespace ubv.client
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/Settings/PlayerSettings", order = 1)]
    public class PlayerSettings : ScriptableObject
    {
        public common.StandardMovementSettings MovementSettings;
        public GameObject PlayerPrefab;
    }
    
}
