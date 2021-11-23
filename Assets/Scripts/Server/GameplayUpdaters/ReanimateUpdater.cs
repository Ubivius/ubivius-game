using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;
using ubv.common.world.cellType;

namespace ubv.server.logic
{
    public class ReanimateUpdater : ServerGameplayStateUpdater
    {
        [SerializeField] PlayerMovementUpdater m_playerMouvementUpdater;

        const float c_buttonDistance = 1.5f;

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
                    List<int> playersId = new List<int>();
                    Vector2 currentPlayerPosition = state.Players()[id].Position.Value;
                    foreach (int id2 in state.Players().Keys)
                    {
                        if (id != id2)
                        {
                            playersId.Add(id2);
                        }
                    }

                    foreach (int id2 in playersId)
                    {
                        Vector2 playerPos = state.Players()[id2].Position.Value;
                        float diff = (currentPlayerPosition - playerPos).sqrMagnitude;
                        if (diff <= c_buttonDistance)
                        {
                            if (!m_playerMouvementUpdater.IsPlayerAlive(id2))
                            {
                                m_playerMouvementUpdater.Heal(id2);
                            }
                        }
                    }

                }
            }

        }

        public override void UpdateWorld(WorldState client)
        {
        }

    }
}

