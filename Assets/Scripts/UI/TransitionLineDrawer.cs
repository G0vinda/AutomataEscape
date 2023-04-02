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
                { StateChartManager.TransitionCondition.IsInFrontOfWall, 0 },
                { StateChartManager.TransitionCondition.StandsOnKey, 0 },
                { StateChartManager.TransitionCondition.StandsOnOrange, 0 },
                { StateChartManager.TransitionCondition.StandsOnPurple, 0 },
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
                { StateChartManager.TransitionCondition.StandsOnOrange, new Color32[] {
                    new (0xE5, 0x8D, 0x1B, 0xFF),
                    new (0xE5, 0x8D, 0x1B, 0xFF),
                    new (0xE5, 0x8D, 0x1B, 0xFF),
                    new (0xE5, 0x8D, 0x1B, 0xFF),
                    new (0xE5, 0x8D, 0x1B, 0xFF),
                    new (0xE5, 0x8D, 0x1B, 0xFF),
                    new (0xE5, 0x8D, 0x1B, 0xFF),
                    new (0xE5, 0x8D, 0x1B, 0xFF),
                    new (0xE5, 0x8D, 0x1B, 0xFF),
                    new (0xE5, 0x8D, 0x1B, 0xFF)
                }},
                { StateChartManager.TransitionCondition.StandsOnPurple, new Color32[] {
                    new (0x56, 0x31, 0xBC, 0xFF),
                    new (0x56, 0x31, 0xBC, 0xFF),
                    new (0x56, 0x31, 0xBC, 0xFF),
                    new (0x56, 0x31, 0xBC, 0xFF),
                    new (0x56, 0x31, 0xBC, 0xFF),
                    new (0x56, 0x31, 0xBC, 0xFF),
                    new (0x56, 0x31, 0xBC, 0xFF),
                    new (0x56, 0x31, 0xBC, 0xFF),
                    new (0x56, 0x31, 0xBC, 0xFF),
                    new (0x56, 0x31, 0xBC, 0xFF)
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

            if (inputIsHorizontal)
            {
                if (hoveredSubCell.PlacedHorizontalLine != null)
                    return false;

                hoveredSubCell.PlacedHorizontalLine = CurrentTransitionLine;
            }
            else
            {
                if (hoveredSubCell.PlacedVerticalLine != null)
                    return false;

                hoveredSubCell.PlacedVerticalLine = CurrentTransitionLine;
            }

            var drawStartPosition = StateChartUIGrid.GetStateBorderPosition(sourceState.transform.position, inputPosition, inputDirection);
            var colorIndex = _numberOfLinesByCondition[CurrentTransitionCondition]++;
            var lineColor = _colorSetsByCondition[CurrentTransitionCondition][colorIndex % 10];
            CurrentTransitionLine = sourceState.DrawFirstTransitionLineElement(drawStartPosition, inputDirection, lineColor, CurrentTransitionCondition);
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
                DestinationStateElement.HighlightAsTransitionDestination();
                var stateCell = DestinationStateElement.GetComponent<StateUIElement>().ConnectedCell;
                (_plugPosition, _plugDirection) =
                    StateChartUIGrid.GetPlugAttributesForAdjacentState(_currentSubCellPosition, stateCell);
                return true;
            }


            if (DestinationStateElement != null)
            {
                DestinationStateElement.RemoveHighlight();
                DestinationStateElement = null;
                _plugPosition = Vector2.negativeInfinity;
            }

            if (StateChartUIGrid.CheckIfSubCellIsAdjacentToSubCell(_currentSubCellPosition, inputPosition, out var newDirection))
            {
                if (newDirection.IsOpposite(_previousDrawDirection))
                {
                    CurrentTransitionLine.RemoveLastElement();
                    
                    var previousInputWasHorizontal = _previousDrawDirection == Direction.Left ||
                                                     _previousDrawDirection == Direction.Right;
                    ref var hoveredSubCell = ref StateChartUIGrid.GetSubCellOnPosition(inputPosition);
                    if (previousInputWasHorizontal)
                    {
                        hoveredSubCell.PlacedHorizontalLine = null;
                    }
                    else
                    {
                        hoveredSubCell.PlacedVerticalLine = null;
                    }

                    if (!CurrentTransitionLine.TryGetLastElementDirection(out _previousDrawDirection))
                    {
                        return false;
                    }
                }
                else
                {
                    var inputIsHorizontal = newDirection == Direction.Left || newDirection == Direction.Right;
                    var previousInputWasHorizontal = _previousDrawDirection == Direction.Left ||
                                                     _previousDrawDirection == Direction.Right;
                    ref var hoveredSubCell = ref StateChartUIGrid.GetSubCellOnPosition(inputPosition);
                    ref var previousSubCell = ref StateChartUIGrid.GetSubCellOnPosition(_currentSubCellPosition);
                    
                    if (inputIsHorizontal)
                    {
                        if (hoveredSubCell.PlacedHorizontalLine != null)
                            return true;

                        hoveredSubCell.PlacedHorizontalLine = CurrentTransitionLine;
                        if (!previousInputWasHorizontal)
                            previousSubCell.PlacedHorizontalLine = CurrentTransitionLine;
                    }
                    else
                    {
                        if (hoveredSubCell.PlacedVerticalLine != null)
                            return true;

                        hoveredSubCell.PlacedVerticalLine = CurrentTransitionLine;
                        if (previousInputWasHorizontal)
                            previousSubCell.PlacedVerticalLine = CurrentTransitionLine;
                    }
                    CurrentTransitionLine.DrawLineElement(newDirection);
                    _previousDrawDirection = newDirection;
                }
                
                _currentSubCellPosition = StateChartUIGrid.GetNextSubCellPositionInDirection(_currentSubCellPosition, newDirection);
                return true;
            }
            
            return false;
        }

        public static void CancelDraw()
        {
            if(DestinationStateElement != null)
                DestinationStateElement.RemoveHighlight();
        }

        public static void FinishLine()
        {
            CurrentTransitionLine.CreatePlug(_plugPosition, _plugDirection.ToZRotation());
            DestinationStateElement.RemoveHighlight();
        }
    }
}
