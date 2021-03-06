﻿using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;

public abstract class ServerGameplayStateUpdater : MonoBehaviour
{
    public abstract void Setup();

    public abstract void InitClient(ClientState client);

    public abstract void InitPlayer(PlayerState player);

    public abstract void FixedUpdateFromClient(ClientState client, InputFrame input, float deltaTime);

    public abstract void UpdateClient(ClientState client);
}
