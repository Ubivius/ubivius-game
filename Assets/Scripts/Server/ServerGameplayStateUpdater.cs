using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;

public abstract class ServerGameplayStateUpdater : MonoBehaviour
{
    public abstract void Setup();

    public abstract void InitClients(WorldState world);

    public abstract void FixedUpdateFromClient(WorldState client, InputFrame input, float deltaTime);

    public abstract void UpdateClient(ref WorldState client);
}
