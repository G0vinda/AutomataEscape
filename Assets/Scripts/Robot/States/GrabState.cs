﻿using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class GrabState : KeyHandleState
    {
        public GrabState(LevelGridManager levelGridManager, SpriteChanger spriteChanger, ref LevelGridManager.KeyType grabbedKeyType)
            : base(levelGridManager, spriteChanger, grabbedKeyType) {}

        public override bool ProcessState(ref Vector2Int coordinates, ref Direction direction)
        {
            if (!CheckIfOnKey(coordinates) || GrabbedKeyType != LevelGridManager.KeyType.None)
                return false;
            
            GrabbedKeyType = GameManager.Instance.GrabKeyOnCoordinates(coordinates);
            SpriteChanger.SetCarryKeyType(GrabbedKeyType);
            return false;
        }
    }
}