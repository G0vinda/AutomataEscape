using DG.Tweening;
using Helper;
using LevelGrid;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class TurnRightState : RobotState
    {
        public TurnRightState(LevelGridManager levelGridManager, SpriteChanger spriteChanger) : base(levelGridManager, spriteChanger) {}

        public override bool ProcessState(ref Vector2Int coordinates, ref Direction direction, out Tween animation)
        {
            animation = null;
            
            direction = direction.Turn(true);
            SpriteChanger.SetSpriteDirection(direction);
            SoundPlayer.Instance.PlayRobotTurn();
            return false;
        }
    }
}