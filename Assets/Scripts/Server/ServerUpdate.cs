using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace ubv.server.logic
{
    /// <summary>
    /// Manages server specific update logic
    /// </summary>
    public class ServerUpdate : MonoBehaviour
    {
#if NETWORK_SIMULATE
        [HideInInspector] public UnityEngine.Events.UnityEvent ForceStartGameButtonEvent;
#endif // NETWORK_SIMULATE

#if NETWORK_SIMULATE
        public void ForceStart()
        {
            Debug.Log("Forcing game start");
            ForceStartGameButtonEvent.Invoke();
        }
#endif //NETWORK_SIMULATE
    }
}
