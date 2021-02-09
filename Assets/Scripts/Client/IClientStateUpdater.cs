using UnityEngine;
using UnityEditor;

namespace ubv
{
    namespace client
    {
        /// <summary>
        /// Updates the client state. You must implement this interface if
        /// your class contains client data that must be updated and 
        /// synced to the server.
        /// </summary>
        public interface IClientStateUpdater
        {
            /// <summary>
            /// Stores the client state in local storage and updates it.
            /// </summary>
            /// <param name="state">State to store and update</param>
            /// <param name="input">Input used to step state simulation</param>
            /// <param name="deltaTime"></param>
            void SetStateAndStep(ref ClientState state, common.data.InputFrame input, float deltaTime);
            
            /// <summary>
            /// Sets the current local client state to state
            /// </summary>
            /// <param name="state">State to update to</param>
            void UpdateFromState(ClientState state);

            /// <summary>
            /// Checks if current local state needs to be corrected according to
            /// the remote state (server state)
            /// </summary>
            /// <param name="remoteState">The state as computed by the server</param>
            /// <returns>If the client states needs correction</returns>
            bool NeedsCorrection(ClientState localState, ClientState remoteState);
        }
    }
}
