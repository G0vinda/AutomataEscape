using System.Collections.Generic;
using Helper;
using UnityEngine;

namespace UI
{
    public static class TransitionLineDrawer
    {
        public static StateChartUIGrid StateChartUIGrid { get; set; }
        public static StateChartManager.TransitionCondition CurrentTransitionCondition { get; set; }
        public static TransitionLine CurrentTransitionLine { get; private set; }
        public static StateUIPlaceElement DestinationStateElement { get; set; }

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
        
        private static Vector2 _currentSubCellPosition;
        private static Direction _previousDrawDirection;
        private static Vector2 _plugPosition;
        private static Direction _plugDirection;
        
        public static bool StartDrawingIfSubCellIsFree(Vector2 inputPosition, Direction inputDirection, StateUIElement sourceState)
        {
            var inputIsHorizontal = inputDirection == Direction.Left || inputDirection == Direction.Right;
            var hoveredSubCell = StateChartUIGrid.GetSubCellOnPosition(inputPosition);

            if (inputIsHorizontal && hoveredSubCell.BlockedHorizontally ||
                !inputIsHorizontal && hoveredSubCell.BlockedVertically)
                return false;
            
            var drawStartPosition = StateChartUIGrid.GetStateBorderPosition(sourceState.transform.position, inputPosition, inputDirection);
            var colorIndex = _numberOfLinesByCondition[CurrentTransitionCondition]++;
            var lineColor = _colorSetsByCondition[CurrentTransitionCondition][colorIndex % 10];
            CurrentTransitionLine = sourceState.DrawFirstTransitionLine(drawStartPosition, inputDirection, lineColor, CurrentTransitionCondition);
            _currentSubCellPosition =
                StateChartUIGrid.GetNextSubCellPositionInDirection(drawStartPosition, inputDirection, true);
            _previousDrawDirection = inputDirection;
            
            return true;
        }

        public static bool DrawOnInput(Vector2 inputPosition)
        {
            if (!StateChartUIGrid.IsPositionInsideGrid(inputPosition))
                return true;
            
            if(StateChartUIGrid.IsPositionInsideSubCell(_currentSubCellPosition, inputPosition))
                return true;

            if (StateChartUIGrid.CheckIfStateIsAdjacentToSubCell(_currentSubCellPosition, inputPosition,
                out var hoveredStateElement))
            {
                if (DestinationStateElement == hoveredStateElement) 
                    return true;
                
                Debug.Log("State was detected");
                DestinationStateElement = hoveredStateElement;
                var stateCell = DestinationStateElement.GetComponent<StateUIElement>().ConnectedCell;
                (_plugPosition, _plugDirection) =
                    StateChartUIGrid.GetPlugAttributesForAdjacentState(_currentSubCellPosition, stateCell);
                return true;
            }
            DestinationStateElement = null;
            _plugPosition = Vector2.negativeInfinity;

            if (StateChartUIGrid.CheckIfSubCellIsAdjacentToSubCell(_currentSubCellPosition, inputPosition, out var newDirection))
            {
                if (newDirection.IsOpposite(_previousDrawDirection))
                {
                    CurrentTransitionLine.RemoveLastElement();
                    if (!CurrentTransitionLine.TryGetLastElementDirection(out _previousDrawDirection))
                    {
                        return false;
                    }
                }
                else
                {
                    CurrentTransitionLine.DrawLineElement(newDirection);
                    _previousDrawDirection = newDirection;
                }
                
                _currentSubCellPosition = StateChartUIGrid.GetNextSubCellPositionInDirection(_currentSubCellPosition, newDirection);
                return true;
            }
            
            return false;
        }

        public static void FinishLine()
        {
            CurrentTransitionLine.CreatePlug(_plugPosition, _plugDirection.ToZRotation());
        }
    }
}
