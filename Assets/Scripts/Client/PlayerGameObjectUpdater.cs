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
        [SerializeField]
        private float m_velocityLerpTime = 0.3f;
        [SerializeField] private float m_maxPositionError = 1f;
        [SerializeField] private float m_positionLerpTime = 0.2f;
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
        private UnityAction<bool> m_sprintAction;
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

        public override bool IsPredictionWrong(WorldState localState, WorldState remoteState)
        {
            bool err = false;
            // mettre un bool pour IsAlreadyCorrecting ?
            // check correction on goalStates au lieu du current position TODO
            foreach(PlayerState player in remoteState.Players().Values)
            {
                PlayerState localPlayer = localState.Players()[player.GUID.Value];
                if (localPlayer.IsDifferent(player, m_correctionTolerance))
                {
                    //Debug.Log("Needing correction");
                    return true;
                }
            }
            return err;
        }

        public override void SaveSimulationInState(ref WorldState state)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                if (player.GUID.Value != m_playerGUID)
                {
                    player.Position.Value = m_goalStates[player.GUID.Value].Position.Value; 
                    player.Velocity.Value = m_goalStates[player.GUID.Value].Velocity.Value;
                }
                else
                {
                    player.Position.Value = Bodies[player.GUID.Value].position;
                    player.Velocity.Value = Bodies[player.GUID.Value].velocity;
                }
                player.States.Set((int)PlayerStateEnum.IS_SPRINTING, m_isSprinting[player.GUID.Value]);
            }
        }

        public override void Step(InputFrame input, float deltaTime)
        {
            m_timeSinceLastGoal += deltaTime;
            foreach (PlayerState player in m_goalStates.Values)
            {
                int id = player.GUID.Value;

                if (id != m_playerGUID)
                {
                    LerpTowardsState(player, m_timeSinceLastGoal);
                    m_sprintActions[player.GUID.Value].Invoke(player.States.IsTrue((int)PlayerStateEnum.IS_SPRINTING));
                }
            }
            m_sprintActions[m_playerGUID].Invoke(m_isSprinting[m_playerGUID]);
            common.logic.PlayerMovement.Execute(ref m_localPlayerBody, PlayerControllers[m_playerGUID].GetStats(), input, deltaTime);
        }

        public override void ResetSimulationToState(WorldState state)
        {
            m_timeSinceLastGoal = 0;
            foreach (PlayerState player in state.Players().Values)
            {
                int id = player.GUID.Value;
                m_goalStates[id] = player;

                Vector2 currentPos = Bodies[id].position;
                Vector2 goalPos = m_goalStates[id].Position.Value;

                if ((goalPos - currentPos).sqrMagnitude > m_maxPositionError * m_maxPositionError)
                {
                    currentPos = goalPos;
                    Bodies[id].position = currentPos;
                }
                m_isSprinting[id] = player.States.IsTrue((int)PlayerStateEnum.IS_SPRINTING);
            }
        }

        public override void FixedStateUpdate(float deltaTime)
        {

        }

        private void LerpTowardsState(PlayerState player, float timeSinceLastGoal)
        {
            // if position joueur est trop éloignée, snap
            // else:
            // pas de lerp sur la vitesse, mais on rajoute à la vitesse le déplacement vers la position voulue
            int id = player.GUID.Value;
            Vector2 currentPos = Bodies[id].position;
            Vector2 goalPos = m_goalStates[id].Position.Value;
            Vector2 velocity = m_goalStates[id].Velocity.Value;
            Vector2 delta = goalPos - currentPos;
            if (delta.sqrMagnitude > m_correctionTolerance * m_correctionTolerance)
            {
                float progression = timeSinceLastGoal / m_positionLerpTime;
                if (progression > 1)
                {
                    Bodies[id].position = goalPos;
                }
                else
                {
                    Bodies[id].position = Vector2.Lerp(currentPos, goalPos, progression);
                }
            }

            Bodies[id].velocity = velocity;
        }

        public Transform GetLocalPlayerTransform()
        {
            return m_localPlayerBody.transform;
        }

        public override void UpdateSimulationFromState(WorldState localState, WorldState remoteState)
        {
        }

        public override void DisableSimulation()
        {
            foreach (Rigidbody2D body in Bodies.Values)
            {
                body.simulated = false;
            }
        }

        public override void EnableSimulation()
        {
            foreach (Rigidbody2D body in Bodies.Values)
            {
                body.simulated = true;
            }
        }

        public Dictionary<int, PlayerPrefab> GetPlayersGameObject()
        {
            return Players;
        }
    }
}
