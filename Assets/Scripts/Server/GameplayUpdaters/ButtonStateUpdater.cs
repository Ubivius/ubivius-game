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
        private Dictionary<FinalButtonCell, Vector2Int> m_finalButtonList;

        const float c_buttonDistance = 1.5f;

        public void Awake()
        {
            m_gameMaster.GetWorldGenerator().OnWorldGenerated += OnWorldGenerated;

        }

        public override void Setup()
        {

        }

        public override void InitWorld(WorldState state)
        {
        }


        public override void FixedUpdateFromClient(WorldState state, Dictionary<int, InputFrame> frame, float deltaTime)
        {
            foreach (int id in state.Players().Keys)
            {
                // Faire le check présence client
                if (frame[id].Interact.Value)
                {
                    Vector2 playerPosition = state.Players()[id].Position.Value;

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

                    foreach (FinalButtonCell button in m_finalButtonList.Keys)
                    {
                        Vector2 buttonPos = m_finalButtonList[button];
                        float diff = (playerPosition - buttonPos).sqrMagnitude;
                        if (diff <= c_buttonDistance)
                        {
                            m_gameMaster.InteractFinalButton();
                            continue;
                        }
                    }

                }
            }
            
        }

        public override void UpdateWorld(WorldState client)
        {
        }

        private void OnWorldGenerated()
        {
            m_sectionDoorButtonList =  m_gameMaster.GetWorldGenerator().FetchAllWithPosition<SectionDoorButtonCell>();

            m_buttonSectionList = m_gameMaster.GetWorldGenerator().FetchAllWithPosition<SectionButton>();

            m_finalButtonList = m_gameMaster.GetWorldGenerator().FetchAllWithPosition<FinalButtonCell>(); 
        }

    }
}

