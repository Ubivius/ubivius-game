using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UBV
{

    /// <summary>
    /// Class reprensenting local client state, which will be simulated locally and synced with an authoritative server.
    /// Add here everything that needs to be shared with the server (and the other players).
    /// </summary>
    public class ClientState
    {
        // Add here the stuff you need to share
        public Vector2 Position;
        public Quaternion Rotation;

        #region UTILITY FUNCTIONS
        private static List<IClientStateUpdater> m_updaters = new List<IClientStateUpdater>();
      
        static public void RegisterUpdater(IClientStateUpdater updater)
        {
            m_updaters.Add(updater);
        }

        static public void Step(ref ClientState state, InputFrame input, float deltaTime)
        {
            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].ClientStep(ref state, input, deltaTime);
            }
        }
        #endregion
    }
}