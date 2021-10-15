using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;

namespace ubv.server.logic
{
    public class PlayerShootingUpdater : ServerGameplayStateUpdater
    {
        [SerializeField] private PlayerShootingSettings m_playerShootingSettings;
        [SerializeField] private PlayerMovementUpdater m_playerMovementUpdater;

        private Dictionary<int, PlayerPrefab> m_playersGameObjects;

        private Dictionary<int, bool> m_isShooting;
        private Dictionary<int, Vector2> m_shootingDirection;

        public override void Setup()
        {
            m_playersGameObjects = new Dictionary<int, PlayerPrefab>();

            m_isShooting = new Dictionary<int, bool>();
            m_shootingDirection = new Dictionary<int, Vector2>();
        }

        public override void InitWorld(WorldState state)
        {
            m_playersGameObjects = m_playerMovementUpdater.GetPlayersGameObject();
            foreach (int id in state.Players().Keys)
            {
                m_isShooting[id] = false;
                m_shootingDirection[id] = Vector2.zero;
            }
        }

        public override void FixedUpdateFromClient(WorldState client, Dictionary<int, InputFrame> frames, float deltaTime)
        {
            foreach (int id in client.Players().Keys)
            {
                m_isShooting[id] = frames[id].Shooting.Value;
                m_shootingDirection[id] = frames[id].ShootingDirection.Value;

                common.logic.PlayerShooting.Execute(m_playersGameObjects[id], m_playerShootingSettings, m_shootingDirection[id], deltaTime);
            }
        }

        public override void UpdateWorld(WorldState client)
        {
            foreach (int id in client.Players().Keys)
            {
                PlayerState player = client.Players()[id];
                player.States.Set((int)PlayerStateEnum.IS_SHOOTING, m_isShooting[id]);
                player.ShootingDirection.Value = m_shootingDirection[id];

                if (m_isShooting[id])
                {
                    m_playerHasShotSinceLastSnapshot[player.GUID.Value] = true;
                }
            }
        }

        private Dictionary<int, bool> m_playerHasShotSinceLastSnapshot;

        public override void PrepareWorldStateBeforeSnapshot(WorldState state) 
        {
            foreach (int id in state.Players().Keys)
            {
                PlayerState player = state.Players()[id];
                player.States.Set((int)PlayerStateEnum.IS_SHOOTING, m_playerHasShotSinceLastSnapshot[player.GUID.Value]);
                m_playerHasShotSinceLastSnapshot[player.GUID.Value] = false;
            }
        }
    }
}
