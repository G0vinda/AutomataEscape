﻿using LevelGrid;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.Transitions
{
    public class StandsOnKeyTransition : RobotTransition
    {
        public StandsOnKeyTransition(int destinationId) : base(destinationId) {}

        public override int Priority => 3;

        public override bool CheckCondition(Vector2Int coordinates, Direction direction, LevelGridManager levelGridManager)
        {
            return GameManager.Instance.GetKeyTypeOnCoordinates(coordinates) != LevelGridManager.KeyType.None;
        }
    }
}