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
        [SerializeField] private GameMaster m_gameMaster;

        private Dictionary<int, PlayerState> m_playerStates;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, common.gameplay.PlayerController> m_playerControllers;

        public override void Setup()
        {
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_playerControllers = new Dictionary<int, common.gameplay.PlayerController>();
            m_playerStates = new Dictionary<int, PlayerState>();
        }

        public override void InitClients(WorldState world)
        {
            foreach (int id in world.Players().Keys)
            {
                GameObject playerGameObject = GameObject.Instantiate(m_playerSettings.PlayerPrefab);
                Rigidbody2D body = playerGameObject.GetComponent<Rigidbody2D>();
                common.gameplay.PlayerController playerCtrl = playerGameObject.GetComponent<common.gameplay.PlayerController>();
                //body.position = m_bodies.Count * Vector2.left * 3;
                body.position = m_gameMaster.GetPlayerSpawnPos();
                body.name = "Server player " + id.ToString();
                m_bodies.Add(id, body);

                PlayerState player = new PlayerState(id);
                player.Position.Value = m_bodies[player.GUID.Value].position;
                m_playerStates.Add(id, player);
                m_playerControllers.Add(id, playerCtrl);
                world.AddPlayer(player);
            }
        }


        public override void FixedUpdateFromClient(WorldState client, InputFrame frame, float deltaTime)
        {
            foreach(int id in client.Players().Keys) 
            {
                Rigidbody2D body = m_bodies[id];
                common.logic.PlayerMovement.Execute(ref body, m_playerControllers[id].GetStats(), frame, Time.fixedDeltaTime);
            }
        }

        public override void UpdateClient(ref WorldState client)
        {
            foreach (int id in client.Players().Keys)
            {
                Rigidbody2D body = m_bodies[id];
                PlayerState player = m_playerStates[id];
                player.Position.Value = body.position;
                player.Rotation.Value = body.rotation;
            }
        }
    }
}
