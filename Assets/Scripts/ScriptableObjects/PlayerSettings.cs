using UnityEngine;
using UnityEditor;

namespace ubv.common
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/Settings/PlayerSettings", order = 1)]
    public class PlayerSettings : ScriptableObject
    {
        public GameObject PlayerPrefab;
    }
    
}
