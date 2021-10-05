using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ubv.server.testing.PathTesting))]
public class PathTestingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ubv.server.testing.PathTesting myTarget = (ubv.server.testing.PathTesting)target;

        if(GUILayout.Button("Test Random Path"))
        {
            myTarget.TestRandomPath();
        }

        if (GUILayout.Button("Free node"))
        {
            myTarget.FreeNode();
        }

        if (GUILayout.Button("Block node"))
        {
            myTarget.BlockNode();
        }
    }
}