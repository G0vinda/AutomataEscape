﻿using LevelGrid;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.Transitions
{
    public class StandsOnPurpleTransition : RobotTransition
    {
        public StandsOnPurpleTransition(int destinationId) : base(destinationId) {}

        public override int Priority => 1;
        
        public override bool CheckCondition(Vector2Int coordinates, Direction direction, LevelGridManager levelGridManager)
        {
            return levelGridManager.CheckIfTileIsPurple(coordinates);
        }
    }
}