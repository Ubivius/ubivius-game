using UnityEngine;
using UnityEditor;
using ubv.common;
using System.Collections.Generic;
using ubv.common.data;

namespace ubv.client.logic
{
    /// <summary>
    /// Updates the client state. You must implement this interface if
    /// your class contains client data that must be updated and 
    /// synced to the server.
    /// </summary>
    public abstract class ClientStateUpdater : MonoBehaviour
    {
        public abstract void Init(List<PlayerState> playerStates, int localID);

        /// <summary>
        /// Stores the world client state in local storage.
        /// </summary>
        /// <param name="state">State to store and update</param>
        /// <param name="deltaTime"></param>
        public abstract void UpdateStateFromWorld(ref ClientState state);

        /// <summary>
        /// Updates the local state.
        /// </summary>
        /// <param name="input">Input used to step state simulation</param>
        /// <param name="deltaTime"></param>
        public abstract void Step(common.data.InputFrame input, float deltaTime);

        /// <summary>
        /// Sets the current local client state to state
        /// </summary>
        /// <param name="state">State to update to</param>
        public abstract void UpdateWorldFromState(ClientState state);

        /// <summary>
        /// Checks if current local state needs to be corrected according to
        /// the remote state (server state)
        /// </summary>
        /// <param name="remoteState">The state as computed by the server</param>
        /// <returns>If the client states needs correction</returns>
        public abstract bool NeedsCorrection(ClientState localState, ClientState remoteState);

        public abstract void FixedStateUpdate(float deltaTime);
    }

}
