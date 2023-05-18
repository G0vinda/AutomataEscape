using Helper;
using LevelGrid;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class TurnLeftState : RobotState
    {
        public TurnLeftState(LevelGridManager levelGridManager, SpriteChanger spriteChanger) : base(levelGridManager, spriteChanger) {}

        public override bool ProcessState(ref Vector2Int coordinates, ref Direction direction)
        {
            direction = direction.Turn(false);
            SpriteChanger.SetSpriteDirection(direction);
            SoundPlayer.Instance.PlayRobotTurn();
            return false;
        }
    }
}