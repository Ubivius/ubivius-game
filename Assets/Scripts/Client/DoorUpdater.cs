using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.client.logic;
using ubv.common;
using ubv.common.data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Client
{
    public class DoorUpdater : ClientStateUpdater
    {
        [SerializeField] private Tilemap m_tileDoor;
        private List<ubv.common.serialization.types.Vector2Int> m_OpeningDoor;
        private List<ubv.common.serialization.types.Vector2Int> m_OpeningDoorDiff;


        public override void FixedStateUpdate(float deltaTime)
        {
        }

        public override void Init(WorldState state, int localID)
        {
            m_OpeningDoor = new List<ubv.common.serialization.types.Vector2Int>();
            state.SetOpeningDoor(m_OpeningDoor);
        }

        public override bool NeedsCorrection(WorldState localState, WorldState remoteState)
        {
            m_OpeningDoor = remoteState.OpeningDoors().Value;
            m_OpeningDoorDiff = DiffInOpeningDoor(m_OpeningDoor, localState.OpeningDoors().Value);
            //m_OpeningDoorDiff = m_OpeningDoor.Except(localState.OpeningDoors().Value).ToList();
            bool result = false;
            if (m_OpeningDoorDiff.Count > 0)
            {
                Debug.Log("CELLULES PORTES A OUVRIR : " + m_OpeningDoorDiff.Count);
                result = true;
            }
            return result;
        }

        public override void Step(InputFrame input, float deltaTime)
        {
        }

        public override void UpdateStateFromWorld(ref WorldState state)
        {
            state.SetOpeningDoor(m_OpeningDoor);
        }

        public override void UpdateWorldFromState(WorldState state)
        {
            foreach (ubv.common.serialization.types.Vector2Int door in m_OpeningDoorDiff)
            {
                Vector3Int pos = new Vector3Int(door.Value.x, door.Value.y, 0);
                m_tileDoor.SetTile(pos, null);
                m_tileDoor.RefreshTile(pos);
            }
        }

        private List<ubv.common.serialization.types.Vector2Int> DiffInOpeningDoor(
            List<ubv.common.serialization.types.Vector2Int> remote,
            List<ubv.common.serialization.types.Vector2Int> local)
        {
            List<ubv.common.serialization.types.Vector2Int> tmp = new List<ubv.common.serialization.types.Vector2Int>();
            bool isPresent = false;
            foreach (var remoteDoor in remote)
                {
                isPresent = false;
                foreach (var localDoor in local)
                {
                    if (remoteDoor.Value.x == localDoor.Value.x && remoteDoor.Value.y == localDoor.Value.y)
                    {
                        isPresent = true;
                        break;
                    }                    
                }
                if (!isPresent)
                {
                    tmp.Add(remoteDoor);
                }
            }
            return tmp;
        }
    }
}
