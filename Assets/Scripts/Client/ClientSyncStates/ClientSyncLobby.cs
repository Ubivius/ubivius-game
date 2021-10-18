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
using ubv.microservices;
using System.Threading;

namespace ubv.client.logic
{
    public class ClientListUpdateEvent : UnityEvent<List<CharacterData>>  { }

    public class ClientSyncLobby : ClientSyncState, tcp.client.ITCPClientReceiver
    {
        private enum SubState
        {
            SUBSTATE_WAITING_FOR_EVERYONE_READY,
            SUBSTATE_WAITING_FOR_WORLD,
            SUBSTATE_GOING_TO_GAME
        }
        
        [SerializeField] private string m_clientPlayScene;

        private Flag m_serverSignal;
        
        private Dictionary<int, CharacterData> m_clientCharacters;

        private ServerInitMessage m_awaitedInitMessage;
        private List<int> m_playerIDs;

        public ClientListUpdateEvent ClientListUpdate { get; private set; }

        private string m_activeCharacterID;

        private SubState m_currentSubState;

        public float LoadPercentage { get; private set; }
        
        protected override void StateLoad()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_EVERYONE_READY;
            ClientListUpdate = new ClientListUpdateEvent();
            LoadPercentage = 0;
            m_clientCharacters = new Dictionary<int, CharacterData>();
            Init();
        }
        
        public void SendReadyToServer()
        {
            ClientReadyMessage clientReadyMessage = new ClientReadyMessage();
            m_TCPClient.Send(clientReadyMessage.GetBytes());
        }
        
        public void ReceivePacket(tcp.TCPToolkit.Packet packet)
        {
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
                    m_characterService.Request(new GetSingleCharacterRequest(strID, (CharacterData[] characters) =>
                    {
                        lock (m_lock)
                        {
                            Debug.Log("Got character from " + characters[0].PlayerID + " : " + characters[0].Name);
                            m_clientCharacters[characters[0].PlayerID.GetHashCode()] = characters[0];
                            ClientListUpdate.Invoke(new List<CharacterData>(m_clientCharacters.Values));
                        }
                    }));
                }
                return;
            }
            
            Thread deserializeWorldThread = new Thread(() => 
            {
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
                }
            });
            deserializeWorldThread.Start();
        }

        public override void StateUpdate()
        {
            if(m_awaitedInitMessage != null)
            {
                m_playerIDs = new List<int>();

                foreach(int id in m_awaitedInitMessage.PlayerCharacters.Value.Keys)
                {
                    m_playerIDs.Add(id);
                }

#if DEBUG_LOG
                Debug.Log("Client received confirmation that server is about to start game with " + m_playerIDs.Count + " players");
#endif // DEBUG_LOG

#if DEBUG_LOG
                Debug.Log("Starting to load world.");
#endif // DEBUG_LOG
                LoadingData.ServerInit = m_awaitedInitMessage;
                m_awaitedInitMessage = null;
                GoToGame();
            }
        }

        private void GoToGame()
        {
            LoadingData.PlayerIDs = m_playerIDs;
            LoadingData.GameInfo = new ClientGameInfo(m_clientCharacters.Values);
            ClientStateManager.Instance.PushState(m_clientPlayScene);
        }

        private void Init()
        {
            m_activeCharacterID = LoadingData.ActiveCharacter.ID;
            m_awaitedInitMessage = null;
            m_clientCharacters?.Clear();
            m_TCPClient.Subscribe(this);
            m_TCPClient.Send(new OnLobbyEnteredMessage(m_activeCharacterID).GetBytes());
        }
        
        public void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Lobby : lost connection to game server. Leaving lobby.");
#endif // DEBUG_LOG
            ClientStateManager.Instance.PopState();
        }
        
        protected override void StateUnload()
        {
            m_TCPClient.Unsubscribe(this);
        }

        protected override void StatePause()
        {
            m_TCPClient.Unsubscribe(this);
        }

        protected override void StateResume()
        {
            m_TCPClient.Subscribe(this);
        }

        public void OnSuccessfulTCPConnect() { }
    }   
}
