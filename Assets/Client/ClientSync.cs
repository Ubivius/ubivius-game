using UnityEngine;
using UnityEditor;

public class ClientSync : MonoBehaviour
{
    // has an input buffer to recreate inputs after server correction

    private void Awake()
    {
        
    }

    private void FixedUpdate()
    {
        ClientState.Step(Time.fixedDeltaTime);
    }
}