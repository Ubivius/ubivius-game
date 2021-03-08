using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;

namespace ubv.server.logic
{
    public class PlayerMovementUpdater : IServerGameplayStateUpdater
    {
        private Dictionary<int, Rigidbody2D> m_bodies;
        [SerializeField] private StandardMovementSettings m_movementSettings;
        [SerializeField] private GameObject m_playerPrefab;

        public void Setup()
        {
            m_bodies = new Dictionary<int, Rigidbody2D>();
        }

        public void InitClient(ClientState state)
        {
            int id = state.PlayerGUID;
            Rigidbody2D body = GameObject.Instantiate(m_playerPrefab).GetComponent<Rigidbody2D>();
            body.position = m_bodies.Count * Vector2.left * 3;
            body.name = "Server player " + id.ToString();
            m_bodies.Add(id, body);
        }

        public void InitPlayer(PlayerState player)
        {
            player.Position.Value = m_bodies[player.GUID.Value].position;
        }

        public void FixedUpdate(ClientState client, InputFrame frame, float deltaTime)
        {
            Rigidbody2D body = m_bodies[client.PlayerGUID];
            common.logic.PlayerMovement.Execute(ref body, m_movementSettings, frame, Time.fixedDeltaTime);
        }

        public void UpdateClient(ClientState client)
        {
            Rigidbody2D body = m_bodies[client.PlayerGUID];
            PlayerState player = client.GetPlayer();
            player.Position.Value = body.position;
            player.Rotation.Value = body.rotation;
        }
    }
}
