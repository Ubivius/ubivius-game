using System.Collections.Generic;
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
        [SerializeField] private float m_lerpTime = 0.2f;
        [SerializeField] private float m_correctionTolerance = 0.01f;
        [SerializeField] private PlayerSettings m_playerSettings;
        [SerializeField] private PlayerAnimator m_playerAnimator;

        public Dictionary<int, PlayerPrefab> Players { get; private set; }
        public Dictionary<int, Rigidbody2D> Bodies { get; private set; }
        public Dictionary<int, common.gameplay.PlayerController> PlayerControllers { get; private set; }
        public Dictionary<int, PlayerAnimator> PlayerAnimators { get; private set; }
        private Rigidbody2D m_localPlayerBody;

        private Dictionary<int, bool> m_isSprinting;

        private int m_playerGUID;

        private float m_timeSinceLastGoal;
        private Dictionary<int, PlayerState> m_goalStates;

        public UnityAction OnInitialized;
        UnityAction<bool> m_sprintAction;
        private Dictionary<int, UnityAction<bool>> m_sprintActions;

        public override void Init(WorldState clientState, int localID)
        {
            m_sprintActions = new Dictionary<int, UnityAction<bool>>();
            m_timeSinceLastGoal = 0;
            Players = new Dictionary<int, PlayerPrefab>();
            Bodies = new Dictionary<int, Rigidbody2D>();
            m_isSprinting = new Dictionary<int, bool>();

            m_goalStates = new Dictionary<int, PlayerState>();
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
                m_isSprinting[id] = false;

                PlayerControllers[id] = playerGameObject.GetComponent<common.gameplay.PlayerController>();
                PlayerAnimators[id] = playerGameObject.GetComponent<PlayerAnimator>();

                if (id != localID)
                {
                    Bodies[id].bodyType = RigidbodyType2D.Kinematic;
                }

                m_goalStates[id] = state;
                m_sprintActions[id] = PlayerAnimators[id].SetSprinting;
            }

            m_playerGUID = localID;
            m_localPlayerBody = Bodies[localID];
            OnInitialized?.Invoke();
        }

        public override bool NeedsCorrection(WorldState localState, WorldState remoteState)
        {
            bool err = false;
            // mettre un bool pour IsAlreadyCorrecting ?
            // check correction on goalStates au lieu du current position TODO
            foreach(PlayerState player in remoteState.Players().Values)
            {
                err = (player.Position.Value - localState.Players()[player.GUID.Value].Position.Value).sqrMagnitude > m_correctionTolerance * m_correctionTolerance;
                if (err)
                {
                    //Debug.Log("Needing correction");
                    return true;
                }
            }
            return err;
        }

        public override void UpdateStateFromWorld(ref WorldState state)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                if (player.GUID.Value != m_playerGUID)
                {
                    player.Position.Value = m_goalStates[player.GUID.Value].Position.Value;
                    player.Rotation.Value = m_goalStates[player.GUID.Value].Rotation.Value;
                    player.Velocity.Value = m_goalStates[player.GUID.Value].Velocity.Value;
                }
                else
                {
                    player.Position.Value = Bodies[player.GUID.Value].position;
                    player.Rotation.Value = Bodies[player.GUID.Value].rotation;
                    player.Velocity.Value = Bodies[player.GUID.Value].velocity;
                }
                player.States.Set((int)PlayerStateEnum.IS_SPRINTING, m_isSprinting[player.GUID.Value]);
            }
        }

        public override void Step(InputFrame input, float deltaTime)
        {
            m_timeSinceLastGoal += deltaTime;
            m_isSprinting[m_playerGUID] = input.Sprinting.Value;

            foreach (PlayerState player in m_goalStates.Values)
            {
                int id = player.GUID.Value;

                if (id != m_playerGUID)
                {
                    LerpTowardGoalState(player, m_timeSinceLastGoal);
                    m_sprintActions[id].Invoke(player.States.IsTrue((int)PlayerStateEnum.IS_SPRINTING));
                }
                m_sprintActions[m_playerGUID].Invoke(m_isSprinting[m_playerGUID]);
            }
            common.logic.PlayerMovement.Execute(ref m_localPlayerBody, PlayerControllers[m_playerGUID].GetStats(), input, deltaTime);
        }

        public override void UpdateWorldFromState(WorldState state)
        {
            m_timeSinceLastGoal = 0;
            foreach (PlayerState player in state.Players().Values)
            {
                //Bodies[player.GUID.Value].position = player.Position.Value;
                m_goalStates[player.GUID.Value] = player;

                if (player.GUID.Value == m_playerGUID)
                {
                    Bodies[player.GUID.Value].position = player.Position.Value;
                    Bodies[player.GUID.Value].rotation = player.Rotation.Value;
                    Bodies[player.GUID.Value].velocity = player.Velocity.Value;
                }

                m_isSprinting[player.GUID.Value] = player.States.IsTrue((int)PlayerStateEnum.IS_SPRINTING);
            }
        }

        public override void FixedStateUpdate(float deltaTime)
        {

        }

        private void LerpTowardGoalState(PlayerState player, float time)
        {
            Bodies[player.GUID.Value].velocity = Vector2.Lerp(Bodies[player.GUID.Value].velocity, m_goalStates[player.GUID.Value].Velocity.Value, time / m_lerpTime);
            if ((Bodies[player.GUID.Value].velocity - m_goalStates[player.GUID.Value].Velocity.Value).sqrMagnitude < 0.01f)
            {
                Bodies[player.GUID.Value].velocity = m_goalStates[player.GUID.Value].Velocity.Value;
            }

            Bodies[player.GUID.Value].position = Vector2.Lerp(Bodies[player.GUID.Value].position, m_goalStates[player.GUID.Value].Position.Value, time / m_lerpTime);
            if ((Bodies[player.GUID.Value].position - m_goalStates[player.GUID.Value].Position.Value).sqrMagnitude < 0.01f)
            {
                Bodies[player.GUID.Value].position = m_goalStates[player.GUID.Value].Position.Value;
            }

            Bodies[player.GUID.Value].rotation = Mathf.Lerp(Bodies[player.GUID.Value].rotation, m_goalStates[player.GUID.Value].Rotation.Value, time / m_lerpTime);
            if (Bodies[player.GUID.Value].rotation - m_goalStates[player.GUID.Value].Rotation.Value < 0.01f)
            {
                Bodies[player.GUID.Value].rotation = m_goalStates[player.GUID.Value].Rotation.Value;
            }
        }

        public Transform GetLocalPlayerTransform()
        {
            return m_localPlayerBody.transform;
        }
    }
}
