using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;

namespace ubv.server.logic
{
    public class PlayerMovementUpdater : ServerGameplayStateUpdater
    {
        [SerializeField] private PlayerSettings m_playerSettings;
        [SerializeField] private PlayerShootingSettings m_playerShootingSettings;
        [SerializeField] private GameMaster m_gameMaster;
        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, PlayerState> m_playerStates;
        private Dictionary<int, common.gameplay.PlayerController> m_playerControllers;
        private Dictionary<int, bool> m_isSprinting;

        public override void Setup()
        {
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_playerControllers = new Dictionary<int, common.gameplay.PlayerController>();
            m_playerStates = new Dictionary<int, PlayerState>();
            m_isSprinting = new Dictionary<int, bool>();
        }

        public override void InitWorld(WorldState state)
        {
            foreach(int id in state.Players().Keys)
            {
                GameObject playerGameObject = GameObject.Instantiate(m_playerSettings.PlayerPrefab);
                Rigidbody2D body = playerGameObject.GetComponent<Rigidbody2D>();
                common.gameplay.PlayerController playerCtrl = playerGameObject.GetComponent<common.gameplay.PlayerController>();

                body.position = m_gameMaster.GetPlayerSpawnPos();
                body.name = "Server player " + id.ToString();
                m_bodies.Add(id, body);
                m_isSprinting.Add(id, false);

                PlayerState playerState = state.Players()[id];

                playerState.Position.Value = m_bodies[id].position;

                m_playerControllers.Add(id, playerCtrl);
                m_playerStates.Add(id, playerState);
            }

            foreach (PlayerState player in m_playerStates.Values)
            {
                state.AddPlayer(player);
            }
        }

        public override void FixedUpdateFromClient(WorldState client, Dictionary<int, InputFrame> frames, float deltaTime)
        {
            foreach (int id in client.Players().Keys)
            {
                Rigidbody2D body = m_bodies[id];
                common.logic.PlayerMovement.Execute(ref body, m_playerControllers[id].GetStats(), frames[id], Time.fixedDeltaTime);
                common.logic.PlayerShooting.Execute(m_playerSettings.PlayerPrefab.FirePoint, m_playerShootingSettings.BulletPrefab, m_playerShootingSettings.BulletForce);
                m_isSprinting[id] = frames[id].Sprinting.Value;
            }
        }

        public override void UpdateWorld(WorldState client)
        {
            foreach (int id in client.Players().Keys)
            {
                Rigidbody2D body = m_bodies[id];
                PlayerState player = m_playerStates[id];
                player.Position.Value = body.position;
                player.Rotation.Value = body.rotation;
                player.Velocity.Value = body.velocity;
                player.States.Set(0, m_isSprinting[id]);
            }

        }
    }
}
