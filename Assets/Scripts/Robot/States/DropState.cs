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

        public override Status ProcessState(ref Vector2Int coordinates, ref Direction direction, out Tween animation)
        {
            animation = null;
            
            if (GrabbedKeyType == LevelGridManager.KeyType.None || CheckIfOnKey(coordinates))
                return Status.Running;
            
            GameManager.Instance.DropKeyOnCoordinates(coordinates, GrabbedKeyType);
            GrabbedKeyType = LevelGridManager.KeyType.None;
            SpriteChanger.SetCarryKeyType(LevelGridManager.KeyType.None);
            SpriteChanger.UpdateSprite();
            SoundPlayer.Instance.PlayRobotDrop();
            return Status.Running;
        }
    }
}