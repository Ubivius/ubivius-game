using System.Collections;
using System.Collections.Generic;
using ubv.microservices;
using UnityEngine;

namespace ubv.client.logic
{
    public class ClientNetworkingManager : MonoBehaviour
    {
        static public ClientNetworkingManager Instance { get; private set; } = null;
        
        [Header("Networking")]
        public tcp.client.TCPClient TCPClient;
        public udp.client.UDPClient UDPClient;
        public http.client.HTTPClient HTTPClient;
        public ClientConnectionManager Server;

        [Header("Microservices")]
        public SocialServicesController SocialServices;
        public CharacterDataService CharacterData;
        public DispatcherMicroservice Dispatcher;
        public AchievementService AchievementService;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
