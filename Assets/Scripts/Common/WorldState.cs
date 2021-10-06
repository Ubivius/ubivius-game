using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ubv.common.serialization.types;
using ubv.common.serialization;

namespace ubv.common
{
    /// <summary>
    /// Class reprensenting local client state, which will be simulated locally and synced with an authoritative server.
    /// Add here everything that needs to be shared with the server (and the other players).
    /// </summary>
    public class WorldState : Serializable
    {
        public class PlayerHashMap : HashMap<common.data.PlayerState>
        {
            public PlayerHashMap(Dictionary<int, common.data.PlayerState> players) : base(players)
            { }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.HASHMAP_INT_PLAYERSTATE;
            }
        }

        public class EnemyHashMap : HashMap<common.data.EnemyStateData>
        {
            public EnemyHashMap(Dictionary<int, common.data.EnemyStateData> enemies) : base(enemies)
            { }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.HASHMAP_INT_ENEMYSTATEDATA;
            }
        }

        private PlayerHashMap m_playerStates;
        private EnemyHashMap m_enemyStateData;

        public int EnemyGUID;

        public WorldState() : base()
        {
            //Add EnemyHashMap for every initseriazablemember
            m_playerStates = new PlayerHashMap(new Dictionary<int, common.data.PlayerState>());
            InitSerializableMembers(m_playerStates);
        }

        public WorldState(System.Collections.Generic.List<common.data.PlayerState> playerStates) : base()
        {
            Dictionary<int, common.data.PlayerState> dictStates = new Dictionary<int, data.PlayerState>();
            foreach(data.PlayerState state in playerStates)
            {
                dictStates.Add(state.GUID.Value, state);
            }
            m_playerStates = new PlayerHashMap(dictStates);
            InitSerializableMembers(m_playerStates);
        }

        public WorldState(ref WorldState state) : base()
        {
            m_playerStates = new PlayerHashMap(new Dictionary<int, common.data.PlayerState>());
            SetPlayers(state.m_playerStates.Value);
            InitSerializableMembers(m_playerStates);
        }
            
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_STATE;
        }
            
        public void AddPlayer(data.PlayerState playerState)
        {
            m_playerStates.Value[playerState.GUID.Value] = playerState;
        }

        public void SetPlayers(Dictionary<int, common.data.PlayerState> playerStates)
        {
            m_playerStates.Value.Clear();
            foreach (common.data.PlayerState player in playerStates.Values)
            {
                m_playerStates.Value[player.GUID.Value] = new common.data.PlayerState(player);
            }
        }

        public Dictionary<int, common.data.PlayerState> Players()
        {
            return m_playerStates.Value;
        }

        public data.EnemyStateData GetEnemy()
        {
            return m_enemyStateData.Value[EnemyGUID]; //liste 'ennemy a la placce
        }

        public void AddEnemy(data.EnemyStateData enemyStateData)
        {
            m_enemyStateData.Value[enemyStateData.GUID.Value] = enemyStateData;
        }

        public void SetEnemies(Dictionary<int, common.data.EnemyStateData> enemyStateData)
        {
            m_enemyStateData.Value.Clear();
            foreach (common.data.EnemyStateData enemy in enemyStateData.Values)
            {
                m_enemyStateData.Value[enemy.GUID.Value] = new common.data.EnemyStateData(enemy);
            }
        }

        public Dictionary<int, common.data.EnemyStateData> Enemies()
        {
            return m_enemyStateData.Value;
        }
    }
}
