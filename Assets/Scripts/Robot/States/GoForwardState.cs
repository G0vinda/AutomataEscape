using Helper;
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

        public override bool ProcessState(ref Vector2Int coordinates, ref Direction direction)
        {
            if (LevelGridManager.CheckIfWayIsBlocked(coordinates, direction))
                return false;

            coordinates += direction.ToVector2Int();
            _robotTransform.position = LevelGridManager.Grid[coordinates].transform.position;
            SpriteChanger.SetSpriteSortingOrder(LevelGridManager.GetSpriteSortingOrderFromCoordinates(coordinates));
            
            return LevelGridManager.CheckIfTileIsGoal(coordinates);
        }
    }
}