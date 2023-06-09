using DG.Tweening;
using Helper;
using LevelGrid;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class TurnLeftState : RobotState
    {
        public TurnLeftState(LevelGridManager levelGridManager, SpriteChanger spriteChanger) : base(levelGridManager, spriteChanger) {}

        public override Status ProcessState(ref Vector2Int coordinates, ref Direction direction, out Tween animation)
        {
            animation = null;
            
            direction = direction.Turn(false);
            SpriteChanger.SetSpriteDirection(direction);
            SoundPlayer.Instance.PlayRobotTurn();
            return Status.Running;
        }
    }
}