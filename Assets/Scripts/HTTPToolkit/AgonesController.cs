using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ubv.http.agonesServer
{
    public class AgonesController : MonoBehaviour
    {
        [SerializeField]
        private GameObject server;

        void Start()
        {
            server.SetActive(false);

#if (UNITY_EDITOR || UNITY_SERVER)
            server.SetActive(true);
#endif
        }
    }
}
