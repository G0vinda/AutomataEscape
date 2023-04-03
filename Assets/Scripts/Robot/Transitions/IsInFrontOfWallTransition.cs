using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.Transitions
{
    public class IsInFrontOfWallTransition : RobotTransition
    {
        public IsInFrontOfWallTransition(int destinationId) : base(destinationId) {}
        
        public override int Priority => 3;

        public override bool CheckCondition(Vector2Int coordinates, Direction direction, LevelGridManager levelGridManager)
        {
            return levelGridManager.CheckIfWayIsBlocked(coordinates, direction);
        }
    }
}