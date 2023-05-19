using DG.Tweening;
using Helper;
using LevelGrid;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class GoForwardState : RobotState
    {
        private Transform _robotTransform;
        public GoForwardState(LevelGridManager levelGridManager, SpriteChanger spriteChanger, Transform robotTransform) : base(
            levelGridManager, spriteChanger)
        {
            _robotTransform = robotTransform;
        }

        public override bool ProcessState(ref Vector2Int coordinates, ref Direction direction, out Tween animation)
        {
            animation = null;
            if (LevelGridManager.CheckIfWayIsBlocked(coordinates, direction))
                return false;

            coordinates += direction.ToVector2Int();
            var moveTime = 0.6f;
            animation = _robotTransform.DOMove(LevelGridManager.Grid[coordinates].transform.position, moveTime).SetEase(Ease.InOutSine);
            SpriteChanger.SetSpriteSortingOrder(LevelGridManager.GetSpriteSortingOrderFromCoordinates(coordinates));
            SoundPlayer.Instance.PlayRobotMove();
            
            return LevelGridManager.CheckIfTileIsGoal(coordinates);
        }
    }
}