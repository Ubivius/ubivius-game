using UnityEngine;
using System.Collections;
using TMPro;

namespace ubv.ui.client
{
    /// <summary>
    /// Describes the basic UI paired with a single player (health bar, name, etc.)
    /// </summary>
    public class CharacterUI : MonoBehaviour
    {
        public Transform TargetPlayerTransform;
        [SerializeField] private TextMeshPro m_name;
        // [SerializeField] private HealthBar, mettons
        
        public void SetName(string name)
        {
            m_name.text = name;
        }

        private void Update()
        {
            if (TargetPlayerTransform != null)
            {
                transform.position = TargetPlayerTransform.position;
            }
        }
    }
}
