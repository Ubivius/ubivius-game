using UnityEngine;
using UnityEditor;

namespace ubv
{
    namespace editor
    {
        [CustomEditor(typeof(client.ClientSync))]
        public class ClientSyncHelperEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

#if NETWORK_SIMULATE
                client.ClientSync targetClientSync = (client.ClientSync)target;
                if( GUILayout.Button("Connect to server : Auto ID"))
                {
                    targetClientSync.ConnectButtonEvent.Invoke();
                }
#endif // NETWORK_SIMULATE
            }
        }
    }
}
