using System.Collections;
using UnityEngine;
using ubv.common.serialization;

namespace ubv.server.logic.ai
{
    public class FightingState : EnemyBehaviorState
    {
        public FightingState(PlayerMovementUpdater playerMovement,
            EnemyMovementUpdater enemyMovement,
            PathfindingGridManager pathfinding)
            : base(enemyMovement, playerMovement, pathfinding)
        {

        }
    }
}
