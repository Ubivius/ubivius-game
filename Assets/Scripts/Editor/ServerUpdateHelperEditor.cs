using UnityEngine;
using UnityEditor;

namespace ubv
{
    namespace editor
    {
        [CustomEditor(typeof(server.logic.ServerUpdate))]
        public class ServerUpdateHelperEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                server.logic.ServerUpdate serverUpdate = (server.logic.ServerUpdate)target;
                if (GUILayout.Button("Force Start Game"))
                {
                    serverUpdate.ForceStartGameButtonEvent.Invoke();
                }
                
            }
        }
    }
}
