﻿using System.Collections.Generic;
using ubv.common;
using ubv.common.data;
using UnityEngine;
using UnityEngine.Events;

namespace ubv.client.logic
{
    /// <summary>
    /// Instantiate players and moves them according to their player states
    /// </summary>
    public class PlayerGameObjectUpdater : ClientStateUpdater
    {
        [SerializeField] private int m_maxLerpFrames = 10;
        [SerializeField] private float m_correctionTolerance = 0.01f;
        [SerializeField] private PlayerSettings m_playerSettings;
        [SerializeField] private PlayerAnimator m_playerAnimator;

        public Dictionary<int, PlayerPrefab> Players { get; private set; }
        public Dictionary<int, Rigidbody2D> Bodies { get; private set; }
        public Dictionary<int, common.gameplay.PlayerController> PlayerControllers { get; private set; }
        public Dictionary<int, PlayerAnimator> PlayerAnimators { get; private set; }
        private Rigidbody2D m_localPlayerBody;
        
        private int m_playerGUID;
        
        private Dictionary<int, PlayerState> m_players;

        public UnityAction OnInitialized;
        private UnityAction<bool> m_sprintAction;
        private Dictionary<int, UnityAction<bool>> m_sprintActions;

        public override void Init(WorldState clientState, int localID)
        {
            m_sprintActions = new Dictionary<int, UnityAction<bool>>();
            Players = new Dictionary<int, PlayerPrefab>();
            Bodies = new Dictionary<int, Rigidbody2D>();

            m_players = new Dictionary<int, PlayerState>();
            PlayerControllers = new Dictionary<int, common.gameplay.PlayerController>();
            PlayerAnimators = new Dictionary<int, PlayerAnimator>();
            int id = 0;
            
            foreach(PlayerState state in clientState.Players().Values)
            {
                id = state.GUID.Value;
                PlayerPrefab playerGameObject = GameObject.Instantiate(m_playerSettings.PlayerPrefab);
                Players[id] = playerGameObject;
                Bodies[id] = playerGameObject.GetComponent<Rigidbody2D>();
                Bodies[id].name = "Client player " + id.ToString();

                PlayerControllers[id] = playerGameObject.GetComponent<common.gameplay.PlayerController>();
                PlayerAnimators[id] = playerGameObject.GetComponent<PlayerAnimator>();

                if (id != localID)
                {
                    Bodies[id].bodyType = RigidbodyType2D.Kinematic;
                }

                m_players[id] = state;
                m_sprintActions[id] = PlayerAnimators[id].SetSprinting;
            }

            m_playerGUID = localID;
            m_localPlayerBody = Bodies[localID];
            OnInitialized?.Invoke();
        }

        public override bool IsPredictionWrong(WorldState localState, WorldState remoteState)
        {
            PlayerState localPlayer = localState.Players()[m_playerGUID];
            PlayerState remotePlayer = remoteState.Players()[m_playerGUID];
            bool isDiff = localPlayer.IsPositionDifferent(remotePlayer, m_correctionTolerance);
            if (isDiff) Debug.LogWarning("Player and server positions are different.");
            return isDiff;
        }

        public override void SaveSimulationInState(ref WorldState state)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                int id = player.GUID.Value;
                float walkVelocity = PlayerControllers[id].GetStats().WalkingVelocity.Value;
                bool isSprinting = Bodies[id].velocity.sqrMagnitude > walkVelocity * walkVelocity;
                player.Position.Value = Bodies[id].position;
                player.Velocity.Value = Bodies[id].velocity;
                player.States.Set((int)PlayerStateEnum.IS_SPRINTING, isSprinting);
            }
        }

        public override void Step(InputFrame input, float deltaTime)
        {
            m_sprintActions[m_playerGUID].Invoke(input.Sprinting.Value);

            Vector2 velocity = common.logic.PlayerMovement.GetVelocity(input.Movement.Value,
                input.Sprinting.Value,
                PlayerControllers[m_playerGUID].GetStats());

            common.logic.PlayerMovement.Execute(ref m_localPlayerBody, velocity);
        }

        public override void ResetSimulationToState(WorldState state)
        {
            int localID = m_playerGUID;
            PlayerState remote = state.Players()[localID];
            m_players[localID] = remote;
            
            foreach (int id in Bodies.Keys)
            {
                if (id != m_playerGUID)
                {
                    Rigidbody2D body = Bodies[id];
                    body.simulated = false;
                }
            }
        }

        public override void FixedStateUpdate(float deltaTime)
        {
            foreach (int id in Bodies.Keys)
            {
                if (id != m_playerGUID)
                {
                    Rigidbody2D body = Bodies[id];
                    body.simulated = true;
                }
            }
            // update every remote player
            foreach (PlayerState player in m_players.Values)
            {
                int id = player.GUID.Value;

                if (id != m_playerGUID)
                {
                    float walkVelocity = PlayerControllers[id].GetStats().WalkingVelocity.Value;
                    float sprintVelocity = walkVelocity * PlayerControllers[id].GetStats().RunningMultiplier.Value;
                    Rigidbody2D body = Bodies[id];

                    Vector2 vel = player.Velocity.Value;
                    Vector2 deltaPos = player.Position.Value - body.position;

                    if (deltaPos.sqrMagnitude > Mathf.Pow(sprintVelocity * deltaTime * m_maxLerpFrames, 2))
                    {
                        body.position = player.Position.Value;
                    }
                    else
                    {
                        float speed = vel.magnitude;
                        vel += deltaPos;
                        vel *= speed / vel.magnitude;
                    }
                    
                    common.logic.PlayerMovement.Execute(ref body, vel);

                    bool isSprinting = body.velocity.sqrMagnitude > walkVelocity * walkVelocity;
                    m_sprintActions[player.GUID.Value].Invoke(isSprinting);
                }
            }
        }
        
        public Transform GetLocalPlayerTransform()
        {
            return m_localPlayerBody.transform;
        }

        public override void UpdateSimulationFromState(WorldState localState, WorldState remoteState)
        {
            // update remote players 
            foreach(int id in remoteState.Players().Keys)
            {
                m_players[id] = remoteState.Players()[id];
            }
        }

        public override void DisableSimulation()
        {
            foreach (int id in Bodies.Keys)
            {
                Rigidbody2D body = Bodies[id];
                body.simulated = false;
            }
        }

        public override void EnableSimulation()
        {
            foreach (int id in Bodies.Keys)
            {
                Rigidbody2D body = Bodies[id];
                body.simulated = true;
            }
        }

        public Dictionary<int, PlayerPrefab> GetPlayersGameObject()
        {
            return Players;
        }
    }
}
