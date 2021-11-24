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

        private Dictionary<int, PlayerPrefab> m_playersGameObjects;
        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, PlayerState> m_playerStates;
        private Dictionary<int, common.gameplay.PlayerController> m_playerControllers;
        private Dictionary<int, bool> m_isSprinting;

        public override void Setup()
        {
            m_playersGameObjects = new Dictionary<int, PlayerPrefab>();
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_playerControllers = new Dictionary<int, common.gameplay.PlayerController>();
            m_playerStates = new Dictionary<int, PlayerState>();
            m_isSprinting = new Dictionary<int, bool>();
        }

        public override void InitWorld(WorldState state)
        {
            foreach (int id in state.Players().Keys)
            {
                PlayerPrefab playerGameObject = GameObject.Instantiate(m_playerSettings.PlayerPrefab);
                m_playersGameObjects.Add(id, playerGameObject);
                Rigidbody2D body = playerGameObject.GetComponent<Rigidbody2D>();
                common.gameplay.PlayerController playerCtrl = playerGameObject.GetComponent<common.gameplay.PlayerController>();

                body.position = m_gameMaster.GetPlayerSpawnPos();
                body.name = "Server player " + id.ToString();
                m_bodies.Add(id, body);
                m_isSprinting.Add(id, false);

                PlayerState playerState = state.Players()[id];

                playerState.Position.Value = m_bodies[id].position;
                playerState.CurrentHP.Value = playerCtrl.GetCurrentHP();

                m_playerControllers.Add(id, playerCtrl);
                m_playerStates.Add(id, state.Players()[id]);
            }
        }

        public override void FixedUpdateFromClient(WorldState client, Dictionary<int, InputFrame> frames, float deltaTime)
        {
            foreach (int id in client.Players().Keys)
            {
                Rigidbody2D body = m_bodies[id];
                if (m_playerControllers[id].IsAlive())
                {
                    Vector2 velocity = common.logic.PlayerMovement.GetVelocity(frames[id].Movement.Value,
                        frames[id].Sprinting.Value, m_playerControllers[id].GetStats());
                    common.logic.PlayerMovement.Execute(ref body, velocity);
                    m_isSprinting[id] = frames[id].Sprinting.Value;

                    client.Players()[id].CurrentHP.Value = m_playerControllers[id].GetCurrentHP();
                }
                else
                {
                    body.velocity = Vector2.zero;
                }
            }
        }

        public override void UpdateWorld(WorldState client)
        {
            foreach (int id in client.Players().Keys)
            {
                Rigidbody2D body = m_bodies[id];
                PlayerState player = m_playerStates[id];
                player.Position.Value = body.position;
                player.Velocity.Value = body.velocity;
                player.CurrentHP.Value = m_playerControllers[id].GetCurrentHP();
                player.States.Set((int)PlayerStateEnum.IS_SPRINTING, m_isSprinting[id]);
            }
        }

        public Dictionary<int, PlayerPrefab> GetPlayersGameObject()
        {
            return m_playersGameObjects;
        }

        public void SetPlayerPosition(PlayerState player, Vector2Int pos)
        {
            m_bodies[player.GUID.Value].position = pos;
            player.Position.Value = m_bodies[player.GUID.Value].position;
        }

        public bool IsPlayerAlive(int playerID)
        {
            return m_playerControllers[playerID].IsAlive();
        }

        public void Heal(int playerID)
        {
            Debug.Log("Dans le heal de playerMovementUpdater");
            m_playerControllers[playerID].Heal(m_playerControllers[playerID].GetMaxHealth()/2);
        }
    }
}
