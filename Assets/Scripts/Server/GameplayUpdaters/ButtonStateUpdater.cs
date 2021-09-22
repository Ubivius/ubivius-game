using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;
using ubv.common.world.cellType;

namespace ubv.server.logic
{
    public class ButtonStateUpdater : ServerGameplayStateUpdater
    {
        [SerializeField] private GameMaster m_gameMaster;

        private Dictionary<SectionDoorButtonCell, Vector2Int> m_sectionDoorButtonList;
        private Dictionary<SectionButton, Vector2Int> m_buttonSectionList;

        const float c_buttonDistance = 1f;

        public void Awake()
        {
            m_gameMaster.GetWorldGenerator().OnWorldGenerated += OnWorldGenerated;

        }

        public override void Setup()
        {

        }

        public override void InitClient(ClientState state)
        {
        }

        public override void InitPlayer(PlayerState player)
        {
        }

        public override void FixedUpdateFromClient(ClientState client, InputFrame frame, float deltaTime)
        {
            // Faire le check présence client
            if (frame.Interact.Value)
            {
                Vector2 playerPosition = client.GetPlayer().Position.Value;

                foreach (SectionDoorButtonCell button in m_sectionDoorButtonList.Keys)
                {
                    Vector2 buttonPos = m_sectionDoorButtonList[button];
                    float diff = (playerPosition - buttonPos).sqrMagnitude;
                    if (diff <= c_buttonDistance)
                    {
                        m_gameMaster.InteractSectionDoorButton(button);
                        continue;
                    }
                }

                foreach (SectionButton button in m_buttonSectionList.Keys)
                {
                    Vector2 buttonPos = m_buttonSectionList[button];
                    float diff = (playerPosition - buttonPos).sqrMagnitude;
                    if (diff <= c_buttonDistance)
                    {
                        m_gameMaster.InteractSectionButton(button);
                        continue;
                    }
                }

            }
            
        }

        public override void UpdateClient(ClientState client)
        {
        }

        private void OnWorldGenerated()
        {
            m_sectionDoorButtonList =  m_gameMaster.GetWorldGenerator().FetchAllWithPosition<SectionDoorButtonCell>();

            m_buttonSectionList = m_gameMaster.GetWorldGenerator().FetchAllWithPosition<SectionButton>();
        }

    }
}

