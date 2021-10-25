using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.client.logic;
using ubv.client.world;
using ubv.common;
using ubv.common.data;
using ubv.common.world.cellType;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Client
{
    public class DoorUpdater : ClientStateUpdater
    {
        [SerializeField] private Tilemap m_tileDoor;
        [SerializeField] private WorldRebuilder m_world;
        private List<ubv.common.serialization.types.Int32> m_OpeningDoor;
        private List<ubv.common.serialization.types.Int32> m_OpeningDoorDiff;


        public override void FixedStateUpdate(float deltaTime)
        {
        }

        public override void Init(WorldState state, int localID)
        {
            m_OpeningDoor = new List<ubv.common.serialization.types.Int32>();
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
            foreach (ubv.common.serialization.types.Int32 doorSection in m_OpeningDoorDiff)
            {
                switch (doorSection.Value)
                {
                    case (int)DoorType.Section_North:
                        RemoveDoor(m_world.DoorNorth);
                        break;
                    case (int)DoorType.Section_East:
                        RemoveDoor(m_world.DoorEast);
                        break;
                    case (int)DoorType.Section_South:
                        RemoveDoor(m_world.DoorSouth);
                        break;
                    case (int)DoorType.Section_West:
                        RemoveDoor(m_world.DoorWest);
                        break;
                    case (int)DoorType.Section0_NorthEast:
                        RemoveDoor(m_world.DoorSection0NorthEast);
                        break;
                    case (int)DoorType.Section0_SouthEast:
                        RemoveDoor(m_world.DoorSection0SouthEast);
                        break;
                    case (int)DoorType.Section0_SouthWest:
                        RemoveDoor(m_world.DoorSection0SouthWest);
                        break;
                    case (int)DoorType.Section0_NorthWest:
                        RemoveDoor(m_world.DoorSection0NorthWest);
                        break;
                    case (int)DoorType.FinalDoor:
                        RemoveDoor(m_world.FinalDoor);
                        break;
                    default:
                        break;
                }
            }
        }

        private List<ubv.common.serialization.types.Int32> DiffInOpeningDoor(
            List<ubv.common.serialization.types.Int32> remote,
            List<ubv.common.serialization.types.Int32> local)
        {
            List<ubv.common.serialization.types.Int32> tmp = new List<ubv.common.serialization.types.Int32>();
            bool isPresent = false;
            foreach (var remoteDoor in remote)
                {
                isPresent = false;
                foreach (var localDoor in local)
                {
                    if (remoteDoor.Value == localDoor.Value)
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

        private void RemoveDoor(List<Vector2Int> doorList)
        {
            foreach (Vector2Int doorPos in doorList)
            {
                Vector3Int pos = new Vector3Int(doorPos.x, doorPos.y, 0);
                m_tileDoor.SetTile(pos, null);
                m_tileDoor.RefreshTile(pos);
            }
        }
    }
}
