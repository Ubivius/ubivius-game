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
    public class PlayerGameObjectUpdater :  ClientStateUpdater
    {
        [SerializeField] private PlayerSettings m_playerSettings;

        public Dictionary<int, Rigidbody2D> Bodies { get; private set; }
        public Dictionary<int, common.gameplay.PlayerController> PlayerControllers { get; private set; }
        private Rigidbody2D m_localPlayerBody;

        private int m_playerGUID;

        //private Dictionary<int, PlayerState> m_goalStates;

        public UnityAction OnInitialized;
        
        public override void Init(List<PlayerState> playerStates, int localID)
        {
            Bodies = new Dictionary<int, Rigidbody2D>();
            //m_goalStates = new Dictionary<int, PlayerState>();
            PlayerControllers = new Dictionary<int, common.gameplay.PlayerController>();
            int id = 0;
            foreach(PlayerState state in playerStates)
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
                
                //m_goalStates[id] = state;
            }

            m_playerGUID = localID;
            m_localPlayerBody = Bodies[localID];
            OnInitialized?.Invoke();
        }

        public override bool NeedsCorrection(ClientState localState, ClientState remoteState)
        {
            bool err = false;
            // mettre un bool pour IsAlreadyCorrecting ?
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
                player.Position.Value = Bodies[player.GUID.Value].position;
                player.Rotation.Value = Bodies[player.GUID.Value].rotation;
            }
            
            common.logic.PlayerMovement.Execute(ref m_localPlayerBody, PlayerControllers[state.PlayerGUID].GetStats(), input, deltaTime);
        }

        public override void UpdateFromState(ClientState state)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                Bodies[player.GUID.Value].position = player.Position.Value;
                /*if (player.GUID.Value != m_playerGUID)
                {
                    m_goalStates[player.GUID.Value] = player;
                }
                else
                {
                    Bodies[player.GUID.Value].position = player.Position.Value;
                    Bodies[player.GUID.Value].rotation = player.Rotation.Value;
                }*/
            }
        }

        public override void FixedStateUpdate(float deltaTime)
        {
            /*foreach (PlayerState player in m_goalStates.Values)
            {
                if (player.GUID.Value != m_playerGUID)
                {
                    Bodies[player.GUID.Value].position = Vector2.LerpUnclamped(Bodies[player.GUID.Value].position, m_goalStates[player.GUID.Value].Position.Value, 0.25f);
                    if((Bodies[player.GUID.Value].position - m_goalStates[player.GUID.Value].Position.Value).sqrMagnitude < 0.01f)
                    {
                        Bodies[player.GUID.Value].position = m_goalStates[player.GUID.Value].Position.Value;
                    }

                    Bodies[player.GUID.Value].rotation = player.Rotation.Value;
                }
            }*/
        }

        public Transform GetLocalPlayerTransform()
        {
            return m_localPlayerBody.transform;
        }
    }
}
