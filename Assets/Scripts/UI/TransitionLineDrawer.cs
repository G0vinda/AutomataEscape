using System;
using System.Collections;
using Helper;
using UnityEngine;

namespace UI
{
    public static class TransitionLineDrawer
    {
        public static StateChartUIGrid StateChartUIGrid { get; set; }

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
            _currentTransitionLine = sourceState.DrawFirstTransitionLine(drawStartPosition, inputDirection);
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
