using LevelGrid;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.Transitions
{
    public abstract class RobotTransition
    {
        public int DestinationId { get; }
        public abstract int Priority { get; }
        public abstract bool CheckCondition(Vector2Int coordinates, Direction direction, LevelGridManager levelGridManager);

        protected RobotTransition(int destinationId)
        {
            DestinationId = destinationId;
        }
    }
}