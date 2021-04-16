using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ubv.server.testing.HealthBarTesting))]
public class HealthBarTestingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ubv.server.testing.HealthBarTesting myTarget = (ubv.server.testing.HealthBarTesting)target;

        if (GUILayout.Button("Heal"))
        {
            myTarget.Heal();
        }

        if (GUILayout.Button("Damage"))
        {
            myTarget.Damage();
        }
    }
}