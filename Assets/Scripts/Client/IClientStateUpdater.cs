using UnityEngine;
using UnityEditor;

namespace ubv
{
    namespace client
    {
        public interface IClientStateUpdater
        {
            void ClientStoreAndStep(ref ClientState state, common.data.InputFrame input, float deltaTime);
            void UpdateFromState(ClientState state);
            bool NeedsCorrection(ClientState localState, ClientState remoteState);
        }
    }
}