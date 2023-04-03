using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class DropState : KeyHandleState
    {
        private LevelGridManager.KeyType _grabbedKeyType;
        
        public DropState(LevelGridManager levelGridManager, SpriteChanger spriteChanger, ref LevelGridManager.KeyType grabbedKeyType) 
            : base(levelGridManager, spriteChanger, grabbedKeyType) {}

        public override bool ProcessState(ref Vector2Int coordinates, ref Direction direction)
        {
            if (GrabbedKeyType == LevelGridManager.KeyType.None || CheckIfOnKey(coordinates))
                return false;
            
            GameManager.Instance.DropKeyOnCoordinates(coordinates, GrabbedKeyType);
            GrabbedKeyType = LevelGridManager.KeyType.None;
            SpriteChanger.SetCarryKeyType(LevelGridManager.KeyType.None);
            return false;
        }
    }
}