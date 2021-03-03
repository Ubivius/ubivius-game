﻿using UnityEngine;
using System.Collections;
using ubv.common.data;
using System.Collections.Generic;

namespace ubv.client.logic
{
    /// <summary>
    /// Instantiate players and moves them according to their player states
    /// </summary>
    public class PlayerGameObjectUpdater :  IClientStateUpdater
    {
        private const bool SMOOTH_CLIENT_CORRECTION = true;

        private PlayerSettings m_playerSettings;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Rigidbody2D m_localPlayerBody;

        private int m_playerGUID;

        private Dictionary<int, PlayerState> m_goalStates;
        
        public PlayerGameObjectUpdater(PlayerSettings playerSettings, Dictionary<int, PlayerState> playerStates, int localID)
        {
            m_playerSettings = playerSettings;
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_goalStates = new Dictionary<int, PlayerState>();
            
            foreach(int id in playerStates.Keys)
            {
                m_bodies[id] = GameObject.Instantiate(playerSettings.PlayerPrefab).GetComponent<Rigidbody2D>();
                m_bodies[id].name = "Client player " + id.ToString();
                if (id != localID)
                {
                    m_bodies[id].bodyType = RigidbodyType2D.Kinematic;
                }
                
                m_goalStates[id] = playerStates[id];
            }

            m_playerGUID = localID;
            m_localPlayerBody = m_bodies[localID];
        }

        public bool NeedsCorrection(ClientState localState, ClientState remoteState)
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

        public void SetStateAndStep(ref ClientState state, InputFrame input, float deltaTime)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                player.Position.Value = m_bodies[player.GUID.Value].position;
                player.Rotation.Value = m_bodies[player.GUID.Value].rotation;
            }
            
            common.logic.PlayerMovement.Execute(ref m_localPlayerBody, m_playerSettings.MovementSettings, input, deltaTime);
        }

        public void UpdateFromState(ClientState state)
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

        public void FixedUpdate(float deltaTime)
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
