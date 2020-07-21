using UnityEngine;
using UnityEditor;

namespace UBV
{
    public interface IClientStateUpdater
    {
        void ClientStep(ref ClientState state, InputFrame input, float deltaTime);
        void SaveClientState(ref ClientState state);
    }
}