using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathTesting))]
public class PathTestingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PathTesting myTarget = (PathTesting)target;

        if(GUILayout.Button("Test Random Path"))
        {
            myTarget.TestRandomPath();
        }
    }
}