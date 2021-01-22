using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ubv
{
    namespace client
    {
        /// <summary>
        /// In charge of adding remote players to the game world.
        /// Instantiates them.
        /// </summary>
        public class RemotePlayersController : MonoBehaviour, client.IClientStateUpdater
        {
            [SerializeField] private GameObject m_playerPrefab; //  TODO : instantiate other players
            private List<common.data.PlayerState> m_players;
            
            private void Awake()
            {
                ClientState.RegisterUpdater(this);
            }

            // Start is called before the first frame update
            void Start()
            {

            }
            
            private void FixedUpdate()
            {
                // ...
            }
            
            public void SetStateAndStep(ref client.ClientState state, common.data.InputFrame input, float deltaTime)
            {
                SetState(ref state);
            }

            public void UpdateFromState(client.ClientState state)
            {
                m_players.Clear();
                foreach(common.data.PlayerState player in state.Players())
                {
                    m_players.Add(new common.data.PlayerState(player));
                }
            }

            public bool NeedsCorrection(client.ClientState remoteState)
            {
                return (m_players.Count != remoteState.Players().Count);
            }

            private void SetState(ref client.ClientState state)
            {
                state.SetPlayers(m_players);
            }
        }
    }
}
