using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Class reprensenting local client state, which will be simulated locally and synced with an authoritative server.
/// Add here everything that needs to be shared with the server (and the other players).
/// </summary>
public class ClientState
{
    // Add here the stuff you need to share
    public Vector3 Position;
    public Quaternion Rotation;

    private List<IClientStateUpdater> m_updaters;

    private static ClientState m_defaultClientState = null;

    public ClientState()
    {
        m_updaters = new List<IClientStateUpdater>();
    }
    
    static public void RegisterUpdater(IClientStateUpdater updater)
    {
        m_defaultClientState.m_updaters.Add(updater);
    }
    
    static public void Step(float deltaTime)
    {
        for(int i = 0; i < m_defaultClientState.m_updaters.Count; i++)
        {
            m_defaultClientState.m_updaters[i].ClientStep(m_defaultClientState, deltaTime);
        }
    }
}