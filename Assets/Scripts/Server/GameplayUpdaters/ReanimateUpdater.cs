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
                    List<PlayerState> players = new List<PlayerState>();
                    Vector2 currentPlayerPosition = state.Players()[id].Position.Value;
                    foreach (int id2 in state.Players().Keys)
                    {
                        if (id != id2)
                        {
                            players.Add(state.Players()[id]);
                        }
                    }

                    foreach (PlayerState player in players)
                    {
                        Vector2 playerPos = player.Position.Value;
                        float diff = (currentPlayerPosition - playerPos).sqrMagnitude;
                        if (diff <= c_buttonDistance)
                        {
                            // Heal
                            continue;
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

