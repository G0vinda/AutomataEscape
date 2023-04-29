﻿using LevelGrid;
using UI;
using UnityEngine;

namespace Robot.States
{
    public abstract class KeyHandleState : RobotState
    {
        protected static LevelGridManager.KeyType GrabbedKeyType;

        protected KeyHandleState(LevelGridManager levelGridManager, SpriteChanger spriteChanger) : base(levelGridManager, spriteChanger)
        {
            GrabbedKeyType = LevelGridManager.KeyType.None;
        }
        
        protected bool CheckIfOnKey(Vector2Int coordinates)
        {
            return GameManager.Instance.IsKeyOnCoordinates(coordinates);
        }
    }
}