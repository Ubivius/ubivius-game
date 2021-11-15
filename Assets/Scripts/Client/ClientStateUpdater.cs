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
        public abstract void Init(WorldState state, int localID);

        /// <summary>
        /// Stores the world client state in local storage.
        /// </summary>
        /// <param name="state">State to store and update</param>
        /// <param name="deltaTime"></param>
        public abstract void SaveSimulationInState(ref WorldState state);

        /// <summary>
        /// Updates the local state.
        /// </summary>
        /// <param name="input">Input used to step state simulation</param>
        /// <param name="deltaTime"></param>
        public abstract void Step(common.data.InputFrame input, float deltaTime);

        /// <summary>
        /// Resets the simulation to state
        /// </summary>
        /// <param name="state">State to update to</param>
        public abstract void ResetSimulationToState(WorldState state);

        /// <summary>
        /// Update simulation based on diff between local and remote state
        /// </summary>
        /// <param name="localState">local state</param>
        /// <param name="remoteState">remote state to check against</param>
        public abstract void UpdateSimulationFromState(WorldState localState, WorldState remoteState);

        /// <summary>
        /// Checks if current local state needs to be corrected according to
        /// the remote state (server state)
        /// </summary>
        /// <param name="remoteState">The state as computed by the server</param>
        /// <returns>If the client states needs correction</returns>
        public abstract bool IsPredictionWrong(WorldState localState, WorldState remoteState);
        
        public abstract void DisableSimulation();

        public abstract void EnableSimulation();

        public abstract void FixedStateUpdate(float deltaTime);
    }

}
