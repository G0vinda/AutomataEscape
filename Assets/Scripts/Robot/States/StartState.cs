using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class StartState : RobotState
    {
        public StartState() : base(null, null) {}

        public override bool ProcessState(ref Vector2Int coordinates, ref Direction direction)
        {
            return false;
        }
    }
}