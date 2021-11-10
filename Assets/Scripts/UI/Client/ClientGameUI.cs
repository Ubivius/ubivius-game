using UnityEngine;
using System.Collections;
using ubv.client.logic;
using System.Collections.Generic;
using ubv.microservices;

namespace ubv.ui.client
{
    public class ClientGameUI : MonoBehaviour
    {
        [SerializeField] private PlayerGameObjectUpdater m_playerUpdater;
        [SerializeField] private ClientSyncPlay m_clientPlayState;
        [SerializeField] private CharacterUI m_characterUIPrefab;

        // healthbar dans le HUD ?

        private Dictionary<int, CharacterUI> m_characterUIs;

        // Use this for initialization
        void Start()
        {
            m_clientPlayState.OnInitializationDone += InitFromPlayState;
        }

        void InitFromPlayState()
        {
            m_characterUIs = new Dictionary<int, CharacterUI>();
            foreach(int id in m_playerUpdater.Bodies.Keys)
            {
                CharacterUI characterUI = GameObject.Instantiate(m_characterUIPrefab, transform);
                m_characterUIs[id] = characterUI;
                string strID = ubv.client.data.LoadingData.ServerInit.PlayerCharacters.Value[id].Value;
                ClientSyncState.CharacterService.Request(new GetSingleCharacterRequest(strID, (CharacterDataService.CharacterData[] character) =>
                {
                    characterUI.SetName(character[0].Name);
                    characterUI.TargetPlayerTransform = m_playerUpdater.Bodies[id].transform;
                }));
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
