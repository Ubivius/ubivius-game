using UnityEngine;
using UnityEngine.UI;

namespace ubv.ui.client
{
    public class ClientMyCharactersUI : TabBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientMyCharactersState m_initState;
        [SerializeField] private AddCharacterUI m_addCharacterForm;

        public void Back()
        {
            m_initState.Back();
        }
    }
}
