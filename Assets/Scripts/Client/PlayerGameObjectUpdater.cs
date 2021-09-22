using System.Collections.Generic;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;
using ubv.common;
using UnityEngine;
using UnityEngine.Events;

namespace ubv.client.logic
{
    /// <summary>
    /// Instantiate players and moves them according to their player states
    /// </summary>
    public class PlayerGameObjectUpdater :  ClientStateUpdater
    {
        [SerializeField] private float m_lerpTime = 0.2f;
        [SerializeField] private float m_correctionTolerance = 0.01f;
        [SerializeField] private PlayerSettings m_playerSettings;
        [SerializeField] private PlayerAnimator m_playerAnimator;

        public Dictionary<int, Rigidbody2D> Bodies { get; private set; }
        public Dictionary<int, common.gameplay.PlayerController> PlayerControllers { get; private set; }
        private Rigidbody2D m_localPlayerBody;

        private int m_playerGUID;

        private float m_timeSinceLastGoal;
        private Dictionary<int, PlayerState> m_goalStates;

        public UnityAction OnInitialized;
        UnityAction<bool> m_sprintAction;
        private List<UnityAction<bool>> m_sprintActions;

        private bool[] m_isSprinting;

        public override void Init(ClientState clientState, int localID)
        {
            m_timeSinceLastGoal = 0;
            Bodies = new Dictionary<int, Rigidbody2D>();
            m_goalStates = new Dictionary<int, PlayerState>();
            PlayerControllers = new Dictionary<int, common.gameplay.PlayerController>();
            int id = 0;
            foreach(PlayerState state in clientState.Players().Values)
            {
                id = state.GUID.Value;
                GameObject playerGameObject = GameObject.Instantiate(m_playerSettings.PlayerPrefab);
                Bodies[id] = playerGameObject.GetComponent<Rigidbody2D>();
                Bodies[id].name = "Client player " + id.ToString();

                PlayerControllers[id] = playerGameObject.GetComponent<common.gameplay.PlayerController>();

                if (id != localID)
                {
                    Bodies[id].bodyType = RigidbodyType2D.Kinematic;
                }
                
                m_goalStates[id] = state;
                /*
                m_sprintActions.Add(m_sprintAction);
                m_sprintActions[id] += playerGameObject.GetComponent<PlayerAnimator>().SetSprinting;
                */
            }

            m_playerGUID = localID;
            m_localPlayerBody = Bodies[localID];
            OnInitialized?.Invoke();
            
        }

        public override bool NeedsCorrection(ClientState localState, ClientState remoteState)
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

        public override void UpdateStateFromWorld(ref ClientState state)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                if (player.GUID.Value != m_playerGUID)
                {
                    player.Position.Value = m_goalStates[player.GUID.Value].Position.Value; 
                    player.Rotation.Value = m_goalStates[player.GUID.Value].Rotation.Value;
                }
                else
                {
                    player.Position.Value = Bodies[player.GUID.Value].position;
                    player.Rotation.Value = Bodies[player.GUID.Value].rotation;
                }
            }
        }

        public override void Step(InputFrame input, float deltaTime)
        {
            m_timeSinceLastGoal += deltaTime;
            foreach (PlayerState player in m_goalStates.Values)
            {
                if (player.GUID.Value != m_playerGUID)
                {
                    LerpTowardGoalState(player, m_timeSinceLastGoal);
                }
            }
            common.logic.PlayerMovement.Execute(ref m_localPlayerBody, PlayerControllers[m_playerGUID].GetStats(), input, deltaTime);
        }

        public override void UpdateWorldFromState(ClientState state)
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
                    
                    if (player.IsSprinting.Value != m_isSprinting[player.GUID.Value])
                    {
                        m_isSprinting[player.GUID.Value] = player.IsSprinting.Value;
                        m_sprintActions[player.GUID.Value].Invoke(m_isSprinting[player.GUID.Value]);
                    }
                    
                }
            }
        }

        public override void FixedStateUpdate(float deltaTime)
        {

        }

        private void LerpTowardGoalState(PlayerState player, float time)
        {
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
