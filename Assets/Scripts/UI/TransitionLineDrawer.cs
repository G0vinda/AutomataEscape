using System;
using System.Collections;
using System.Collections.Generic;
using Helper;
using UnityEngine;

namespace UI
{
    public static class TransitionLineDrawer
    {
        public static StateChartUIGrid StateChartUIGrid { get; set; }
        public static StateChartManager.TransitionCondition CurrentTransitionCondition { get; set; }

        private static Dictionary<StateChartManager.TransitionCondition, int> _numberOfLinesByCondition =
            new Dictionary<StateChartManager.TransitionCondition, int>()
            {
                { StateChartManager.TransitionCondition.Default, 0 },
                { StateChartManager.TransitionCondition.StandsOnKey, 0 },
                { StateChartManager.TransitionCondition.IsInFrontOfWall, 0 }
            };

        private static Dictionary<StateChartManager.TransitionCondition, Color32[]> _colorSetsByCondition =
            new Dictionary<StateChartManager.TransitionCondition, Color32[]>()
            {
                { StateChartManager.TransitionCondition.Default, new Color32[] {
                    new (0x50, 0x50, 0x50, 0xFF),
                    new (0x44, 0x44, 0x44, 0xFF),
                    new (0x68, 0x68, 0x68, 0xFF),
                    new (0x49, 0x49, 0x49, 0xFF),
                    new (0x5C, 0x5C, 0x5C, 0xFF),
                    new (0x3E, 0x3E, 0x3E, 0xFF),
                    new (0x75, 0x75, 0x75, 0xFF),
                    new (0x38, 0x38, 0x38, 0xFF),
                    new (0x2D, 0x2D, 0x2D, 0xFF),
                    new (0x33, 0x33, 0x33, 0xFF)
                }},
                { StateChartManager.TransitionCondition.IsInFrontOfWall, new Color32[] {
                    new (0xDF, 0x22, 0x22, 0xFF),
                    new (0xDF, 0x22, 0x22, 0xFF),
                    new (0xDF, 0x22, 0x22, 0xFF),
                    new (0xDF, 0x22, 0x22, 0xFF),
                    new (0xDF, 0x22, 0x22, 0xFF),
                    new (0xDF, 0x22, 0x22, 0xFF),
                    new (0xDF, 0x22, 0x22, 0xFF),
                    new (0xDF, 0x22, 0x22, 0xFF),
                    new (0xDF, 0x22, 0x22, 0xFF),
                    new (0xDF, 0x22, 0x22, 0xFF)
                }},
                { StateChartManager.TransitionCondition.StandsOnKey, new Color32[] {
                    new (0x10, 0x22, 0xDF, 0xFF),
                    new (0x10, 0x22, 0xDF, 0xFF),
                    new (0x10, 0x22, 0xDF, 0xFF),
                    new (0x10, 0x22, 0xDF, 0xFF),
                    new (0x10, 0x22, 0xDF, 0xFF),
                    new (0x10, 0x22, 0xDF, 0xFF),
                    new (0x10, 0x22, 0xDF, 0xFF),
                    new (0x10, 0x22, 0xDF, 0xFF),
                    new (0x10, 0x22, 0xDF, 0xFF),
                    new (0x10, 0x22, 0xDF, 0xFF)
                }},
                
            };
        
        private static TransitionLine _currentTransitionLine;
        private static Vector2 _currentSubCellPosition;
        private static Direction _previousDrawDirection;
        
        public static bool StartDrawingIfSubCellIsFree(Vector2 inputPosition, Direction inputDirection, StateUIElement sourceState)
        {
            var inputIsHorizontal = inputDirection == Direction.Left || inputDirection == Direction.Right;
            var hoveredSubCell = StateChartUIGrid.GetSubCellOnPosition(inputPosition);

            if (inputIsHorizontal && hoveredSubCell.BlockedHorizontally ||
                !inputIsHorizontal && hoveredSubCell.BlockedVertically)
                return false;
            
            var drawStartPosition = StateChartUIGrid.GetTransitionDrawStartPosition(sourceState.transform.position, inputPosition, inputDirection);
            var colorIndex = _numberOfLinesByCondition[CurrentTransitionCondition]++;
            var lineColor = _colorSetsByCondition[CurrentTransitionCondition][colorIndex];
            _currentTransitionLine = sourceState.DrawFirstTransitionLine(drawStartPosition, inputDirection, lineColor);
            _currentSubCellPosition =
                StateChartUIGrid.GetNextSubCellPositionInDirection(drawStartPosition, inputDirection, true);
            _previousDrawDirection = inputDirection;
            
            return true;
        }

        public static bool DrawOnInput(Vector2 inputPosition)
        {
            if(StateChartUIGrid.IsPositionInsideSubCell(_currentSubCellPosition, inputPosition))
                return true;

            if (StateChartUIGrid.CheckIfSubCellIsAdjacentToSubCell(_currentSubCellPosition, inputPosition, out var newDirection))
            {
                if (!newDirection.IsOpposite(_previousDrawDirection))
                {
                    _currentTransitionLine.DrawLineElement(newDirection);
                    _previousDrawDirection = newDirection;
                }
                else
                {
                    _currentTransitionLine.RemoveLastElement();
                    if (!_currentTransitionLine.TryGetLastElementDirection(out _previousDrawDirection))
                        return false;
                }

                _currentSubCellPosition = StateChartUIGrid.GetNextSubCellPositionInDirection(_currentSubCellPosition, newDirection);
                return true;
            }

            return false;
        }
    }
}
