using UI;
using UnityEngine;

namespace Robot.States
{
    public abstract class KeyHandleState : RobotState
    {
        protected LevelGridManager.KeyType GrabbedKeyType;

        protected KeyHandleState(LevelGridManager levelGridManager, SpriteChanger spriteChanger, LevelGridManager.KeyType grabbedKeyType) : base(levelGridManager, spriteChanger)
        {
            GrabbedKeyType = grabbedKeyType;
        }
        
        protected bool CheckIfOnKey(Vector2Int coordinates)
        {
            return GameManager.Instance.IsKeyOnCoordinates(coordinates);
        }
    }
}