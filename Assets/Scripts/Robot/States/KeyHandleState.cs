using LevelGrid;
using UI;
using UnityEngine;

namespace Robot.States
{
    public abstract class KeyHandleState : RobotState
    {
        protected static LevelGridManager.KeyType GrabbedKeyType;

        public static void ResetGrabbedKeyType()
        {
            GrabbedKeyType = LevelGridManager.KeyType.None;
        }

        protected KeyHandleState(LevelGridManager levelGridManager, SpriteChanger spriteChanger) : base(levelGridManager, spriteChanger)
        {
            GrabbedKeyType = LevelGridManager.KeyType.None;
        }

        protected LevelGridManager.KeyType CheckForKey(Vector2Int coordinates)
        {
            return GameManager.Instance.GetKeyTypeOnCoordinates(coordinates);
        }
    }
}