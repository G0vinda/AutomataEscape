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

        private static Dictionary<StateChartManager.TransitionCondition, (Color32 startColor, Color32 endColor)> _lineColors =
            new Dictionary<StateChartManager.TransitionCondition, (Color32, Color32)>()
            {
                { 
                    StateChartManager.TransitionCondition.Default, (new Color32(0x2E, 0x2E, 0x2E, 0xFF), new Color32(0xE0, 0xE0, 0xE0, 0xFF) )
                },
                { 
                    StateChartManager.TransitionCondition.IsInFrontOfWall, (new Color32(0x92, 0xF8, 0xFF, 0xFF), new Color32(0x10, 0x4B, 0x50, 0xFF))
                },
                { 
                    StateChartManager.TransitionCondition.StandsOnKey, (new Color32(0x3C, 0xFA, 0x9E, 0xFF), new Color32(0x10, 0x4F, 0x24, 0xFF))
                },
                { 
                    StateChartManager.TransitionCondition.StandsOnOrange, (new Color32(0xF8, 0x90, 0x41, 0xFF), new Color32(0x59, 0x26, 0x00, 0xFF))
                },
                { 
                    StateChartManager.TransitionCondition.StandsOnPurple, (new Color32(0xB3, 0x4A, 0xA2, 0xFF), new Color32(0x24, 0x1A, 0x5B, 0xFF))
                },
                
            };

        private static readonly float ColorStep = 1f / 9;
        
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
            var colorIndex = _numberOfLinesByCondition[CurrentTransitionCondition];
            var lineColor = GetLineColor(CurrentTransitionCondition, colorIndex);
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
                    ref var previousSubCell = ref StateChartUIGrid.GetSubCellOnPosition(_currentSubCellPosition);
                    if (previousInputWasHorizontal)
                    {
                        previousSubCell.PlacedHorizontalLine = null;
                    }
                    else
                    {
                        previousSubCell.PlacedVerticalLine = null;
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
            _numberOfLinesByCondition[CurrentTransitionCondition]++;
            CurrentTransitionLine.CreatePlug(_plugPosition, _plugDirection.ToZRotation());
            DestinationStateElement.RemoveHighlight();
        }

        public static void TransitionLineRemoved(StateChartManager.TransitionCondition condition)
        {
            _numberOfLinesByCondition[condition]--;
        }

        public static Color GetLineColor(StateChartManager.TransitionCondition condition, int colorIndex)
        {
            var (color1, color2) = _lineColors[condition];
            return Color32.Lerp(color1, color2, ColorStep * colorIndex);
        }
    }
}
