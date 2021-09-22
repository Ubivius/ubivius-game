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
        }

        public override void InitClient(ClientState state)
        {

        }

        public override void InitPlayer(PlayerState player)
        {

        }

        public override void FixedUpdateFromClient(ClientState client, InputFrame frame, float deltaTime)
        {
        }

        public override void UpdateClient(ref ClientState client)
        {//state change et deplacement
        }
    }
}
