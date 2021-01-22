using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace ubv
{
    namespace server
    {
        namespace logic
        {
            /// <summary>
            /// Manages server specific update logic
            /// </summary>
            public class ServerUpdate : MonoBehaviour
            {
                private ServerState m_currentState;

                [SerializeField] private GameObject m_playerPrefab;
                [SerializeField] private common.StandardMovementSettings m_movementSettings;
                [SerializeField] private string m_physicsScene;
                [SerializeField] private int m_snapshotDelay;

                [SerializeField] private udp.server.UDPServer m_server;
                
                // Use this for initialization
                void Start()
                {
                    m_currentState = new GameCreationState(m_server, m_playerPrefab, m_movementSettings, m_snapshotDelay, m_physicsScene);
                }

                // Update is called once per frame
                void Update()
                {
                    m_currentState = m_currentState.Update();
                }

                // Updates all players rewinds if necessary
                private void FixedUpdate()
                {
                    m_currentState = m_currentState.FixedUpdate();
                }
                
                // DRAFT SUR AUTH:
                /*
                 * Client envoie un message d'auth en TCP, qui contient
                 *  un "hello" + des credentials / un moyen de prouver 
                 *  qui'il est legit
                 * Serveur renvoie un message d'acknowledgement de
                 * connexion, qui contient le playerID qu'il a attribué 
                 * au joueur du client
                 * 
                 * En théorie, on connect et on disconnect vont être gérées par le TCP 
                 * et pas par l'UDP
                 * et OnConnect dans l'UDP/ici gérerait un nouveau joueur dont on connait
                 * déjà l'ID (qui a été donné  par le TCP)
                */
            }
        }
    }
}
