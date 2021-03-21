using UnityEngine;
using System.Collections;
using ubv.common.data;
using System.Collections.Generic;
using ubv.common;

namespace ubv.client.logic
{
    /// <summary>
    /// Instantiate players and moves them according to their player states
    /// </summary>
    public class PlayerGameObjectUpdater :  ClientStateUpdater
    {
        [SerializeField] private PlayerSettings m_playerSettings;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, common.gameplay.PlayerController> m_playerControllers;
        private Rigidbody2D m_localPlayerBody;

        private int m_playerGUID;

        private Dictionary<int, PlayerState> m_goalStates;
        
        public override void Init(List<PlayerState> playerStates, int localID)
        {
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_goalStates = new Dictionary<int, PlayerState>();
            m_playerControllers = new Dictionary<int, common.gameplay.PlayerController>();
            int id = 0;
            foreach(PlayerState state in playerStates)
            {
                id = state.GUID.Value;
                GameObject playerGameObject = GameObject.Instantiate(m_playerSettings.PlayerPrefab);
                m_bodies[id] = playerGameObject.GetComponent<Rigidbody2D>();
                m_bodies[id].name = "Client player " + id.ToString();

                m_playerControllers[id] = playerGameObject.GetComponent<common.gameplay.PlayerController>();

                if (id != localID)
                {
                    m_bodies[id].bodyType = RigidbodyType2D.Kinematic;
                }
                
                m_goalStates[id] = state;
            }

            m_playerGUID = localID;
            m_localPlayerBody = m_bodies[localID];
        }

        public override bool NeedsCorrection(ClientState localState, ClientState remoteState)
        {
            bool err = false;
            foreach(PlayerState player in remoteState.Players().Values)
            {
                err = (player.Position.Value - localState.Players()[player.GUID.Value].Position.Value).sqrMagnitude > 0.01f;
                if (err)
                {
                    return true;
                }
            }
            return err;
        }

        public override void SetStateAndStep(ref ClientState state, InputFrame input, float deltaTime)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                player.Position.Value = m_bodies[player.GUID.Value].position;
                player.Rotation.Value = m_bodies[player.GUID.Value].rotation;
            }
            
            common.logic.PlayerMovement.Execute(ref m_localPlayerBody, m_playerControllers[state.PlayerGUID].GetStats(), input, deltaTime);
        }

        public override void UpdateFromState(ClientState state)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                if (player.GUID.Value != m_playerGUID)
                {
                    m_goalStates[player.GUID.Value] = player;
                }
                else
                {
                    m_bodies[player.GUID.Value].position = player.Position.Value;
                    m_bodies[player.GUID.Value].rotation = player.Rotation.Value;
                }
            }
        }

        public override void FixedStateUpdate(float deltaTime)
        {
            foreach (PlayerState player in m_goalStates.Values)
            {
                if (player.GUID.Value != m_playerGUID)
                {
                    m_bodies[player.GUID.Value].position = Vector2.LerpUnclamped(m_bodies[player.GUID.Value].position, m_goalStates[player.GUID.Value].Position.Value, 0.25f);
                    if((m_bodies[player.GUID.Value].position - m_goalStates[player.GUID.Value].Position.Value).sqrMagnitude < 0.01f)
                    {
                        m_bodies[player.GUID.Value].position = m_goalStates[player.GUID.Value].Position.Value;
                    }

                    m_bodies[player.GUID.Value].rotation = player.Rotation.Value;
                }
            }
        }
    }
}
