using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;
using UnityEngine.Events;

namespace ubv.server.logic
{
    public class EnemyBehaviorUpdater : ServerGameplayStateUpdater
    {
        [SerializeField] private EnemySettings m_enemySettings;
        [SerializeField] private GameMaster m_gameMaster;
        [SerializeField] private EnemyPathfindingManager m_pathfindingmanager;


        public override void Setup()
        {
            //instantier un seul ennemy pur le moment
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_playerControllers = new Dictionary<int, common.gameplay.PlayerController>();
        }

        public override void InitClient(ClientState state)
        {
            int id = state.EnemyGUID;
            GameObject playerGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
            Rigidbody2D body = playerGameObject.GetComponent<Rigidbody2D>();
            common.gameplay.PlayerController playerCtrl = playerGameObject.GetComponent<common.gameplay.PlayerController>();
            body.position = m_gameMaster.GetPlayerSpawnPos();
            body.name = "Server enemy " + id.ToString();
            m_bodies.Add(id, body);

            m_playerControllers.Add(state.PlayerGUID, playerCtrl);

        }

        public override void InitPlayer(PlayerState player)
        {
            enemy.Position.Value = m_bodies[player.GUID.Value].position;
        }

        public override void FixedUpdateFromClient(ClientState client, InputFrame frame, float deltaTime)
        {
        }

        public override void UpdateClient(ref ClientState client)
        {//state change et deplacement
        }
    }
}
