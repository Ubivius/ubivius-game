using UnityEngine;
using UnityEditor;

namespace ubv
{
    public interface IClientStateUpdater
    {
        void ClientStep(ref ClientState state, InputFrame input, float deltaTime);
        void SaveClientState(ref ClientState state);
        bool NeedsCorrection(ref ClientState localState, ref ClientState remoteState);
    }
}