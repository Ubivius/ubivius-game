using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using UnityEngine.Events;
using static ubv.microservices.CharacterDataService;
using static ubv.common.data.CharacterListMessage;
using ubv.utils;

namespace ubv.client.logic
{
    public class ClientListUpdateEvent : UnityEvent<List<CharacterData>>  { }

    public class ClientSyncLobby : ClientSyncState, tcp.client.ITCPClientReceiver
    {
        [SerializeField] private string m_clientGameSearchScene;
        [SerializeField] private string m_clientPlayScene;

        private Flag m_serverSignal;

        private int? m_simulationBuffer;
        private Dictionary<int, CharacterData> m_clientCharacters;

        private ServerInitMessage m_awaitedInitMessage;
        private List<int> m_playerIDs;

        public ClientListUpdateEvent ClientListUpdate { get; private set; }
        public UnityAction OnStartLoadWorld;
        public UnityAction OnGameStart;

        private string m_cachedActiveCharacterID;

        public float LoadPercentage { get; private set; }
        
        protected override void StateAwake()
        {
            ClientListUpdate = new ClientListUpdateEvent();
            ClientSyncState.m_lobbyState = this;
            m_serverSignal = new Flag();
            LoadPercentage = 0;
            m_cachedActiveCharacterID = null;
            m_clientCharacters = new Dictionary<int, CharacterData>();
        }

        private void OnWorldBuilt()
        {
            ClientWorldLoadedMessage worldLoaded = new ClientWorldLoadedMessage();
            m_TCPClient.Send(worldLoaded.GetBytes());
        }
        
        public void SendReadyToServer()
        {
            ClientReadyMessage clientReadyMessage = new ClientReadyMessage();
            m_TCPClient.Send(clientReadyMessage.GetBytes());
        }
        
        public void ReceivePacket(tcp.TCPToolkit.Packet packet)
        {
            ServerStartsMessage ready = common.serialization.IConvertible.CreateFromBytes<ServerStartsMessage>(packet.Data.ArraySegment());
            if (ready != null)
            {
#if DEBUG_LOG
                Debug.Log("Received server start message.");
#endif // DEBUG_LOG
                m_serverSignal.Raise();
                return;
            }
            
            // loads other players in lobby, receives message from server indicating a new player joined / left
            CharacterListMessage clientList = common.serialization.IConvertible.CreateFromBytes<CharacterListMessage>(packet.Data.ArraySegment());
            if (clientList != null)
            {
                Debug.Log("Received " + clientList.PlayerCharacters.Value.Count + " characters from server");
                m_clientCharacters.Clear(); // clear old list
                foreach (common.serialization.types.String id in clientList.PlayerCharacters.Value.Values)
                {
                    Debug.Log("Fetching character " + id.Value + " from microservice");
                    // fetch character data from microservice
                    string strID = id.Value;
                    m_characterService.GetCharacter(strID, (CharacterData character) =>
                    {
                        lock (m_lock)
                        {
                            Debug.Log("Got character from " + character.PlayerID + " : " + character.Name);
                            m_clientCharacters[character.PlayerID.GetHashCode()] = character;
                            ClientListUpdate.Invoke(new List<CharacterData>(m_clientCharacters.Values));
                        }
                    });
                }
                return;
            }
            
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            ServerInitMessage start = common.serialization.IConvertible.CreateFromBytes<ServerInitMessage>(packet.Data.ArraySegment());
            watch.Stop();
            if (start != null)
            {
#if DEBUG_LOG
                Debug.Log("Time elapsed for world deserialization : " + watch.ElapsedMilliseconds + " ms");
#endif // DEBUG_LOG
                m_awaitedInitMessage = start;
                return;
            }
        }

        protected override void StateUpdate()
        {
            if(m_awaitedInitMessage != null)
            {
                m_simulationBuffer = m_awaitedInitMessage.SimulationBuffer.Value;
                m_playerIDs = new List<int>();

                foreach(int id in m_awaitedInitMessage.PlayerCharacters.Value.Keys)
                {
                    m_playerIDs.Add(id);
                }

#if DEBUG_LOG
                Debug.Log("Client received confirmation that server is about to start game with " + m_playerIDs.Count + " players and " + m_simulationBuffer + " simulation buffer ticks");
#endif // DEBUG_LOG

#if DEBUG_LOG
                Debug.Log("Starting to load world.");
#endif // DEBUG_LOG

                StartCoroutine(LoadWorldCoroutine(m_awaitedInitMessage.CellInfo2DArray.Value));
                m_awaitedInitMessage = null;
            }

            if (m_serverSignal.Read())
            {
                GoToGame();
            }
        }

        private void GoToGame()
        {
            m_TCPClient.Unsubscribe(this);
            OnGameStart?.Invoke();
            ClientSyncState.m_playState.Init(m_simulationBuffer.Value, m_playerIDs, new ClientGameInfo(m_clientCharacters.Values));
            m_currentState = ClientSyncState.m_playState;
        }

        private IEnumerator LeaveLobbyCoroutine(bool leaveServer)
        {
            m_TCPClient.Unsubscribe(this);
            AsyncOperation loadGame = SceneManager.LoadSceneAsync(m_clientGameSearchScene);
            while (!loadGame.isDone)
            {
                yield return null;
            }
            m_initState.Init(leaveServer);
            m_currentState = m_initState;
        }

        public void LeaveLobby(bool leaveServer)
        {
            StartCoroutine(LeaveLobbyCoroutine(leaveServer));
        }

        public void Init(string activeCharacterID)
        {
            m_cachedActiveCharacterID = activeCharacterID;
            m_awaitedInitMessage = null;
            m_clientCharacters?.Clear();
            m_TCPClient.Subscribe(this);
            OnSuccessfulTCPConnect();
        }
        
        private IEnumerator LoadWorldCoroutine(common.world.cellType.CellInfo[,] cellInfos)
        {
            OnStartLoadWorld?.Invoke();
            AsyncOperation loadGame = SceneManager.LoadSceneAsync(m_clientPlayScene);
            while (!loadGame.isDone)
            {
                LoadPercentage = loadGame.progress * 0.25f;
                yield return null;
            }

            world.WorldRebuilder worldRebuilder = LoadingData.WorldRebuilder;
            worldRebuilder.OnWorldBuilt(OnWorldBuilt);
            worldRebuilder.BuildWorldFromCellInfo(cellInfos);
            while (!worldRebuilder.IsRebuilt)
            {
                LoadPercentage = 0.25f + (worldRebuilder.GetWorldBuildProgress() * 0.75f);
                yield return null;
            }
        }

        public void OnDisconnect()
        {
            ClientListUpdate.Invoke(new List<CharacterData>());
#if DEBUG_LOG
            Debug.Log("Lobby : lost connection to game server.");
#endif // DEBUG_LOG
        }

        public void OnSuccessfulTCPConnect()
        {
            if (m_cachedActiveCharacterID != null)
            {
#if DEBUG_LOG
                Debug.Log("Connected to game server.");
#endif // DEBUG_LOG
                m_TCPClient.Send(new OnLobbyEnteredMessage(m_cachedActiveCharacterID).GetBytes());
            }
            else
            {
                Debug.LogError("No active character on connect. Aborting.");
            }
        }
    }   
}
