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
    /// Instantiate players shooting
    /// </summary>
    public class ShootingUpdater : ClientStateUpdater
    {
        [SerializeField] private PlayerSettings m_playerSettings;
        [SerializeField] private PlayerShootingSettings m_playerShootingSettings;

        public Dictionary<int, PlayerPrefab> Players { get; private set; }

        private int m_playerGUID;
        private Dictionary<int, PlayerState> m_goalStates;

        private Dictionary<int, bool> m_isShooting;
        private Dictionary<int, Vector2> m_shootingDirection;

        public override void Init(WorldState clientState, int localID)
        {
            PlayerPrefab playerGameObject = GameObject.Instantiate(m_playerSettings.PlayerPrefab);

            m_playerGUID = localID;
            m_goalStates = new Dictionary<int, PlayerState>();
            Players = new Dictionary<int, PlayerPrefab>();

            m_isShooting = new Dictionary<int, bool>();
            m_shootingDirection = new Dictionary<int, Vector2>();

            foreach (PlayerState state in clientState.Players().Values)
            {
                int id = state.GUID.Value;
                m_goalStates[id] = state;
                Players[id] = playerGameObject;
                
                m_isShooting[id] = false;
                m_shootingDirection[id] = new Vector2(0, 0);
            }
        }

        public override bool NeedsCorrection(WorldState localState, WorldState remoteState)
        {
            return false;
        }

        public override void UpdateStateFromWorld(ref WorldState state)
        {
            
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
                    common.logic.PlayerShooting.Execute(Players[id], m_playerShootingSettings, input, deltaTime);
                }
            }

            common.logic.PlayerShooting.Execute(Players[m_playerGUID], m_playerShootingSettings, input, deltaTime);
        }

        public override void UpdateWorldFromState(WorldState state)
        {
            
        }

        public override void FixedStateUpdate(float deltaTime)
        {

        }
    }
}
