using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;

namespace ubv.server.logic
{
    public class ButtonSataUpdater : ServerGameplayStateUpdater
    {
        [SerializeField] private PlayerSettings m_playerSettings;
        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, common.gameplay.PlayerController> m_playerControllers;

        public override void Setup()
        {
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_playerControllers = new Dictionary<int, common.gameplay.PlayerController>();
        }

        public override void InitClient(ClientState state)
        {
            int id = state.PlayerGUID;
            GameObject playerGameObject = GameObject.Instantiate(m_playerSettings.PlayerPrefab);
            Rigidbody2D body = playerGameObject.GetComponent<Rigidbody2D>();
            common.gameplay.PlayerController playerCtrl = playerGameObject.GetComponent<common.gameplay.PlayerController>();
            //body.position = m_bodies.Count * Vector2.left * 3;
            body.name = "Server player " + id.ToString();
            m_bodies.Add(id, body);

            m_playerControllers.Add(state.PlayerGUID, playerCtrl);
        }

        public override void InitPlayer(PlayerState player)
        {
        }

        public override void FixedUpdateFromClient(ClientState client, InputFrame frame, float deltaTime)
        {
            Rigidbody2D body = m_bodies[client.PlayerGUID];
            common.logic.PlayerMovement.Execute(ref body, m_playerControllers[client.PlayerGUID].GetStats(), frame, Time.fixedDeltaTime);
        }

        public override void UpdateClient(ClientState client)
        {
            Rigidbody2D body = m_bodies[client.PlayerGUID];
            PlayerState player = client.GetPlayer();
            player.Position.Value = body.position;
            player.Rotation.Value = body.rotation;
        }

        public void SetPlayerPosition(PlayerState player, Vector2Int pos)
        {
            m_bodies[player.GUID.Value].position = pos;
            player.Position.Value = m_bodies[player.GUID.Value].position;
        }
    }
}
