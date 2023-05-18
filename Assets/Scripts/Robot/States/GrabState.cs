using LevelGrid;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class GrabState : KeyHandleState
    {
        public GrabState(LevelGridManager levelGridManager, SpriteChanger spriteChanger) : base(levelGridManager, spriteChanger) {}

        public override bool ProcessState(ref Vector2Int coordinates, ref Direction direction)
        {
            if (!CheckIfOnKey(coordinates) || GrabbedKeyType != LevelGridManager.KeyType.None)
                return false;
            
            GrabbedKeyType = GameManager.Instance.GrabKeyOnCoordinates(coordinates);
            SpriteChanger.SetCarryKeyType(GrabbedKeyType);
            SoundPlayer.Instance.PlayRobotGrab();
            return false;
        }
    }
}