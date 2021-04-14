using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace ubv.ui.client
{
    public class ClientGameSearchUI : MonoBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientSyncInit m_initState;
        [SerializeField] private TextMeshProUGUI m_characterName;

        // TODO LATER:
        // offer choice to choose among living characters/make a new one ?

        private void Update()
        {
            if (Time.frameCount % 69 == 0)
            {
                m_characterName.text = m_initState.GetActiveCharacter()?.Name;
            }
        }
    }
}
