using UnityEngine;
using UnityEditor;

namespace ubv
{
    public interface IClientStateUpdater
    {
        void ClientStep(ref ClientState state, InputFrame input, float deltaTime);
        void SetClientState(ref ClientState state);
        void UpdateFromState(ClientState state);
        bool NeedsCorrection(ClientState localState, ClientState remoteState);
    }
}