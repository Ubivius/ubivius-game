using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.data;
using ubv.common.serialization;
using ubv.common;
using UnityEngine;

namespace ubv.server.logic
{
    class GameMaster : ServerGameplayStateUpdater
    {
        [SerializeField] private ubv.common.world.WorldGenerator m_world;

        public override void Setup()
        {
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

        public override void UpdateClient(ClientState client)
        {
        }
    }
}
