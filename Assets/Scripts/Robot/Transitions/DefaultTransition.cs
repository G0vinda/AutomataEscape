using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.Transitions
{
    public class DefaultTransition : RobotTransition
    {
        public DefaultTransition(int destinationId) : base(destinationId) {}

        public override int Priority => 0;

        public override bool CheckCondition(Vector2Int coordinates, Direction direction, LevelGridManager levelGridManager)
        {
            return true;
        }
    }
}