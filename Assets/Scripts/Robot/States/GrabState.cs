using DG.Tweening;
using LevelGrid;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class GrabState : KeyHandleState
    {
        public GrabState(LevelGridManager levelGridManager, SpriteChanger spriteChanger) : base(levelGridManager, spriteChanger) {}

        public override Status ProcessState(ref Vector2Int coordinates, ref Direction direction, out Tween animation)
        {
            animation = null;
            var keyTypeOnGround = CheckForKey(coordinates);
            if (keyTypeOnGround == LevelGridManager.KeyType.None || GrabbedKeyType != LevelGridManager.KeyType.None)
                return Status.Running;

            GrabbedKeyType = keyTypeOnGround;
            var pickUpCoordinates = coordinates;
            SpriteChanger.GrabKey(keyTypeOnGround,
                () =>
                {
                    Debug.Log("GrabAction got called!");
                    GameManager.Instance.GrabKeyOnCoordinates(pickUpCoordinates);
                    SoundPlayer.Instance.PlayRobotGrab();
                });
            return Status.Running;
        }
    }
}