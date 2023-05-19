using DG.Tweening;
using LevelGrid;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class DropState : KeyHandleState
    {
        public DropState(LevelGridManager levelGridManager, SpriteChanger spriteChanger) : base(levelGridManager, spriteChanger) {}

        public override bool ProcessState(ref Vector2Int coordinates, ref Direction direction, out Tween animation)
        {
            animation = null;
            
            if (GrabbedKeyType == LevelGridManager.KeyType.None || CheckIfOnKey(coordinates))
                return false;
            
            GameManager.Instance.DropKeyOnCoordinates(coordinates, GrabbedKeyType);
            GrabbedKeyType = LevelGridManager.KeyType.None;
            SpriteChanger.SetCarryKeyType(LevelGridManager.KeyType.None);
            SoundPlayer.Instance.PlayRobotDrop();
            return false;
        }
    }
}