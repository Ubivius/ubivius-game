using UnityEngine;
using UnityEditor;

public interface IClientStateUpdater
{
    void ClientStep(ClientState state, float deltaTime);
}