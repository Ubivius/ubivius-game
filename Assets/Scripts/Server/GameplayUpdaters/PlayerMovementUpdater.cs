﻿using UnityEngine;
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
        [SerializeField] private Camera cam;

        private Dictionary<int, PlayerPrefab> m_playersGameObjects;
        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, PlayerState> m_playerStates;
        private Dictionary<int, common.gameplay.PlayerController> m_playerControllers;
        private Dictionary<int, bool> m_isSprinting;

        private Dictionary<int, bool> m_isShooting;
        private Dictionary<int, Vector2> m_shootingDirection;

        public override void Setup()
        {
            m_playersGameObjects = new Dictionary<int, PlayerPrefab>();
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_playerControllers = new Dictionary<int, common.gameplay.PlayerController>();
            m_playerStates = new Dictionary<int, PlayerState>();
            m_isSprinting = new Dictionary<int, bool>();

            m_isShooting = new Dictionary<int, bool>();
            m_shootingDirection = new Dictionary<int, Vector2>();
        }

        public override void InitWorld(WorldState state)
        {
            PlayerPrefab playerGameObject = GameObject.Instantiate(m_playerSettings.PlayerPrefab);

            foreach (int id in state.Players().Keys)
            {
                m_playersGameObjects.Add(id, playerGameObject);
                Rigidbody2D body = playerGameObject.GetComponent<Rigidbody2D>();
                common.gameplay.PlayerController playerCtrl = playerGameObject.GetComponent<common.gameplay.PlayerController>();

                body.position = m_gameMaster.GetPlayerSpawnPos();
                body.name = "Server player " + id.ToString();
                m_bodies.Add(id, body);
                m_isSprinting.Add(id, false);

                PlayerState playerState = state.Players()[id];
				
                m_isShooting[id] = false;
                m_shootingDirection[id] = new Vector2(0, 0);

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
                m_isShooting[id] = frames[id].Shooting.Value;
                m_shootingDirection[id] = frames[id].ShootingDirection.Value;

                Rigidbody2D body = m_bodies[id];
                common.logic.PlayerMovement.Execute(ref body, m_playerControllers[id].GetStats(), frames[id], Time.fixedDeltaTime);
                common.logic.PlayerShooting.Execute(m_playersGameObjects[id], m_playerShootingSettings, frames[id], deltaTime);
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
