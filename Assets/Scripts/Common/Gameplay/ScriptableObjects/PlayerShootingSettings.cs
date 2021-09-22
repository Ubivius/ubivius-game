using UnityEngine;
using UnityEditor;

namespace ubv.common
{
    [CreateAssetMenu(fileName = "PlayerShootingSettings", menuName = "ScriptableObjects/Gameplay/PlayerShootingSettings", order = 1)]
    public class PlayerShootingSettings : ScriptableObject
    {
        public GameObject BulletPrefab;
        public float BulletForce;
    }
}