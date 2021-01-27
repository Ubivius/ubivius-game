using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.common.world
{
    [RequireComponent(typeof(Grid))]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2Int m_bondaries;

        [SerializeField] private int m_numberRandomRoom;

        [SerializeField] private List<GameObject> m_roomPoolRandom;
        [SerializeField] private List<GameObject> m_roomPoolSection0;
        [SerializeField] private List<GameObject> m_roomPoolSection1;

        private Grid m_grid;

        private void Awake()
        {
            m_grid = GetComponent<Grid>();

            Instantiate(m_roomPoolSection0[0], new Vector3(0, 0, 0), Quaternion.identity, m_grid.transform);


        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

