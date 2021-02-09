using UnityEngine;
using System.Collections;
using ubv.common.data;
using System.Collections.Generic;

namespace ubv.client.logic
{
    /// <summary>
    /// Instantiate players and moves them according to their player states
    /// </summary>
    public class PlayerGameObjectUpdater : IClientStateUpdater
    {
        private PlayerSettings m_playerSettings;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Rigidbody2D m_localPlayerBody;

        public PlayerGameObjectUpdater(PlayerSettings playerSettings, Dictionary<int, PlayerState> playerStates, int localID)
        {
            m_playerSettings = playerSettings;
            m_bodies = new Dictionary<int, Rigidbody2D>();
            foreach(int id in playerStates.Keys)
            {
                m_bodies[id] = GameObject.Instantiate(playerSettings.PlayerPrefab).GetComponent<Rigidbody2D>();
                m_bodies[id].name = "Client player " + id.ToString();
            }

            m_localPlayerBody = m_bodies[localID];
        }

        public bool NeedsCorrection(ClientState localState, ClientState remoteState)
        {
            bool err = false;
            foreach(PlayerState state in remoteState.Players().Values)
            {
                err = (localState.Players()[state.GUID].Position.Value - state.Position).sqrMagnitude > 0.01f;
                if (err)
                {
                    return true;
                }
            }
            return err;
        }

        public void SetStateAndStep(ref ClientState state, InputFrame input, float deltaTime)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                player.Position.Set(m_bodies[player.GUID].position);
            }
            
            common.logic.PlayerMovement.Execute(ref m_localPlayerBody, m_playerSettings.MovementSettings, input, deltaTime);
        }

        public void UpdateFromState(ClientState state)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                m_bodies[player.GUID].position = player.Position;
            }
        }
    }
}
