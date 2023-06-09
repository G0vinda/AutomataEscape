using DG.Tweening;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class StartState : RobotState
    { 
        public StartState() : base(null, null) {}

        public override Status ProcessState(ref Vector2Int coordinates, ref Direction direction, out Tween animation)
        {
            animation = null;
            return Status.Running;
        }
    }
}