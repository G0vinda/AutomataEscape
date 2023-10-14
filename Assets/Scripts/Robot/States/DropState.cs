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

            var keyTypeOnGround = CheckForKey(coordinates);
            if (GrabbedKeyType == LevelGridManager.KeyType.None || keyTypeOnGround != LevelGridManager.KeyType.None)
                return Status.Running;
            
            var dropCoordinates = coordinates;
            var dropKeyType = GrabbedKeyType;
            SpriteChanger.DropKey(() =>
            {
                GameManager.Instance.DropKeyOnCoordinates(dropCoordinates, dropKeyType);
                SoundPlayer.Instance.PlayRobotDrop();
            });
            GrabbedKeyType = LevelGridManager.KeyType.None;
            return Status.Running;
        }
    }
}