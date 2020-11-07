using UnityEngine;
using UnityEditor;

namespace ubv
{
    public interface IClientStateUpdater
    {
        void ClientStoreAndStep(ref ClientState state, InputFrame input, float deltaTime);
        void UpdateFromState(ClientState state);
        bool NeedsCorrection(ClientState localState, ClientState remoteState);
    }
}