using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;

public abstract class ServerGameplayStateUpdater : MonoBehaviour
{
    public abstract void Setup();

    public abstract void InitWorld(WorldState world);
    
    public abstract void FixedUpdateFromClient(WorldState world, Dictionary<int, InputFrame> inputs, float deltaTime);

    public abstract void UpdateWorld(WorldState client);
}
