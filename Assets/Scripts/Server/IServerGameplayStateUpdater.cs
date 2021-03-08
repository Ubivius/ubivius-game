using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;

public interface IServerGameplayStateUpdater
{
    void Setup();

    void InitClient(ClientState client);

    void InitPlayer(PlayerState player);

    void FixedUpdate(ClientState client, InputFrame input, float deltaTime);

    void UpdateClient(ClientState client);
}
