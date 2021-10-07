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


        public override void FixedStateUpdate(float deltaTime)
        {
        }

        public override void Init(WorldState state, int localID)
        {
        }

        public override bool NeedsCorrection(WorldState localState, WorldState remoteState)
        {
            m_OpeningDoor = remoteState.OpeningDoors().Value.Except(localState.OpeningDoors().Value).ToList();
            if (m_OpeningDoor.Count > 0)
            {
                return true;
            }
            return false;
        }

        public override void Step(InputFrame input, float deltaTime)
        {
        }

        public override void UpdateStateFromWorld(ref WorldState state)
        {
        }

        public override void UpdateWorldFromState(WorldState state)
        {
            foreach (ubv.common.serialization.types.Vector2Int door in m_OpeningDoor)
            {
                Vector3Int pos = new Vector3Int(door.Value.x, door.Value.y, 0);
                m_tileDoor.SetTile(pos, null);
                m_tileDoor.RefreshTile(pos);
            }
        }
    }
}
