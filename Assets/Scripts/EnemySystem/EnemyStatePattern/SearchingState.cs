using UnityEditor;
using UnityEngine;
using ubv.common.serialization;

namespace ubv.server.logic.ai
{
    public class SearchingState : EnemyBehaviorState
    {
        public SearchingState(PlayerMovementUpdater playerMovement,
            EnemyMovementUpdater enemyMovement,
            PathfindingGridManager pathfinding)
            : base(enemyMovement, playerMovement, pathfinding)
        {

        }
    }
}
