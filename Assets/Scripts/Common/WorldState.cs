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

        public class EnemyHashMap : HashMap<common.data.EnemyState>
        {
            public EnemyHashMap(Dictionary<int, common.data.EnemyState> enemies) : base(enemies)
            { }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.HASHMAP_INT_ENEMYSTATEDATA;
            }
        }

        public class Vector2IntList : serialization.types.List<serialization.types.Int32>
        {
            public Vector2IntList(System.Collections.Generic.List<serialization.types.Int32> doorList) : base(doorList)
            {

            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.LIST_VECTOR2INT;
            }
        }

        private PlayerHashMap m_playerStates;
        private EnemyHashMap m_enemyStatesDatas;

        private Vector2IntList m_openingDoorList;

        public WorldState() : base()
        {
            m_playerStates = new PlayerHashMap(new Dictionary<int, common.data.PlayerState>());
            m_enemyStatesDatas = new EnemyHashMap(new Dictionary<int, common.data.EnemyState>());
            m_openingDoorList = new Vector2IntList(new System.Collections.Generic.List<serialization.types.Int32>());
            InitSerializableMembers(m_playerStates, m_enemyStatesDatas, m_openingDoorList);
        }

        public WorldState(System.Collections.Generic.List<common.data.PlayerState> playerStates) : base()
        {
            Dictionary<int, common.data.PlayerState> dictStates = new Dictionary<int, data.PlayerState>();
            foreach(data.PlayerState state in playerStates)
            {
                dictStates.Add(state.GUID.Value, state);
            }
            m_playerStates = new PlayerHashMap(dictStates);
            m_enemyStatesDatas = new EnemyHashMap(new Dictionary<int, common.data.EnemyState>());
            m_openingDoorList = new Vector2IntList(new System.Collections.Generic.List<serialization.types.Int32>());
            InitSerializableMembers(m_playerStates, m_enemyStatesDatas, m_openingDoorList);
        }

        public WorldState(ref WorldState state) : base()
        {
            m_playerStates = new PlayerHashMap(new Dictionary<int, common.data.PlayerState>());
            m_enemyStatesDatas = new EnemyHashMap(new Dictionary<int, common.data.EnemyState>());

            SetPlayers(state.m_playerStates.Value);
            SetEnemies(state.m_enemyStatesDatas.Value);

            m_openingDoorList = new Vector2IntList(new System.Collections.Generic.List<serialization.types.Int32>());
            SetOpeningDoor(state.m_openingDoorList.Value);

            InitSerializableMembers(m_playerStates, m_enemyStatesDatas, m_openingDoorList);
        }
            
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_STATE;
        }

        public void AddPlayer(data.PlayerState playerState)
        {
            m_playerStates.Value[playerState.GUID.Value] = playerState;
        }

        public void AddEnemy(data.EnemyState enemyStateData)
        {
            m_enemyStatesDatas.Value[enemyStateData.GUID.Value] = enemyStateData;
        }

        public void SetPlayers(Dictionary<int, common.data.PlayerState> playerStates)
        {
            m_playerStates.Value.Clear();
            foreach (common.data.PlayerState player in playerStates.Values)
            {
                m_playerStates.Value[player.GUID.Value] = new common.data.PlayerState(player);
            }
        }
        public void SetEnemies(Dictionary<int, common.data.EnemyState> enemyStatesData)
        {
            m_enemyStatesDatas.Value.Clear();
            foreach (common.data.EnemyState enemyStateData in enemyStatesData.Values)
            {
                m_enemyStatesDatas.Value[enemyStateData.GUID.Value] = new common.data.EnemyState();
            }
        }

        public Dictionary<int, common.data.PlayerState> Players()
        {
            return m_playerStates.Value;
        }

        public Dictionary<int, common.data.EnemyState> Enemies()
        {
            return m_enemyStatesDatas.Value;
        }

        public Vector2IntList OpeningDoors()
        {
            return m_openingDoorList;
        }

        public void SetOpeningDoor(System.Collections.Generic.List<serialization.types.Int32> doors)
        {
            m_openingDoorList.Value.Clear();
            foreach (serialization.types.Int32 door in doors)
            {
                m_openingDoorList.Value.Add(door);
            }
        }
    }
}
