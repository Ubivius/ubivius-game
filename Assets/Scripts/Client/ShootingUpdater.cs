using System.Collections.Generic;
using ubv.common;
using ubv.common.data;
using UnityEngine;

namespace ubv.client.logic
{
    /// <summary>
    /// Instantiate players shooting
    /// </summary>
    public class ShootingUpdater : ClientStateUpdater
    {
        [SerializeField] private PlayerSettings m_playerSettings;
        [SerializeField] private PlayerShootingSettings m_playerShootingSettings;
        [SerializeField] private PlayerGameObjectUpdater m_playerGameObjectUpdater;
        [SerializeField] private float m_correctionTolerance = 0.05f;

        public Dictionary<int, PlayerPrefab> Players { get; private set; }

        private int m_playerGUID;
        private Dictionary<int, PlayerState> m_goalStates;

        private Dictionary<int, bool> m_isShooting;
        private Dictionary<int, Vector2> m_shootingDirection;

        public override void Init(WorldState clientState, int localID)
        {
            m_playerGUID = localID;
            m_goalStates = new Dictionary<int, PlayerState>();

            m_isShooting = new Dictionary<int, bool>();
            m_shootingDirection = new Dictionary<int, Vector2>();

            Players = m_playerGameObjectUpdater.GetPlayersGameObject();
            foreach (PlayerState state in clientState.Players().Values)
            {
                int id = state.GUID.Value;
                m_goalStates[id] = state;

                m_isShooting[id] = false;
                m_shootingDirection[id] = Vector2.zero;
            }
        }

        public override bool NeedsCorrection(WorldState localState, WorldState remoteState)
        {
            foreach (PlayerState player in remoteState.Players().Values)
            {
                int playerId = player.GUID.Value;

                if (player.States.IsTrue((int)PlayerStateEnum.IS_SHOOTING) && !localState.Players()[playerId].States.IsTrue((int)PlayerStateEnum.IS_SHOOTING)
                 || !player.States.IsTrue((int)PlayerStateEnum.IS_SHOOTING) && localState.Players()[playerId].States.IsTrue((int)PlayerStateEnum.IS_SHOOTING))
                {
                    return true;
                }
                else if (player.States.IsTrue((int)PlayerStateEnum.IS_SHOOTING))
                {
                    if (!(player.ShootingDirection.Value.x > localState.Players()[playerId].ShootingDirection.Value.x * (1 - m_correctionTolerance))
                     && !(player.ShootingDirection.Value.x < localState.Players()[playerId].ShootingDirection.Value.x * (1 + m_correctionTolerance))
                     && !(player.ShootingDirection.Value.y > localState.Players()[playerId].ShootingDirection.Value.y * (1 - m_correctionTolerance))
                     && !(player.ShootingDirection.Value.y < localState.Players()[playerId].ShootingDirection.Value.y * (1 + m_correctionTolerance)))
                    {
                        return true;
                    }
                }                
            }

            return false;
        }

        public override void UpdateStateFromWorld(ref WorldState state)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                if (player.GUID.Value != m_playerGUID)
                {
                    player.ShootingDirection.Value = m_goalStates[player.GUID.Value].ShootingDirection.Value;
                }
                player.States.Set((int)PlayerStateEnum.IS_SHOOTING, m_isShooting[player.GUID.Value]);
            }
        }

        public override void Step(InputFrame input, float deltaTime)
        {
            m_isShooting[m_playerGUID] = input.Shooting.Value;
            m_shootingDirection[m_playerGUID] = input.ShootingDirection.Value;

            foreach (PlayerState player in m_goalStates.Values)
            {
                int id = player.GUID.Value;

                if (id != m_playerGUID && m_isShooting[id])
                {
                    common.logic.PlayerShooting.Execute(Players[id], m_playerShootingSettings, m_shootingDirection[id], deltaTime);
                }
            }

            if (m_isShooting[m_playerGUID])
            {
                common.logic.PlayerShooting.Execute(Players[m_playerGUID], m_playerShootingSettings, m_shootingDirection[m_playerGUID], deltaTime);
            }
        }

        public override void UpdateWorldFromState(WorldState state)
        {
            foreach (PlayerState player in state.Players().Values)
            {
                m_isShooting[player.GUID.Value] = state.Players()[player.GUID.Value].States.IsTrue((int)PlayerStateEnum.IS_SHOOTING);
                m_shootingDirection[player.GUID.Value] = state.Players()[player.GUID.Value].ShootingDirection.Value;
            }
        }

        public override void FixedStateUpdate(float deltaTime)
        {

        }
    }
}
