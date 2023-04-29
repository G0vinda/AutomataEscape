using System.Collections.Generic;
using Helper;
using Robot;
using UI.Grid;
using UI.State;
using UnityEngine;

namespace UI.Transition
{
    public static class TransitionLineDrawer
    {
        public static UIGridManager UIGridManager { get; set; }
        public static StateChartManager.TransitionCondition CurrentTransitionCondition { get; set; }
        public static TransitionLine CurrentTransitionLine { get; private set; }
        public static StateUIPlaceElement DestinationStateElement { get; set; }

        private static Dictionary<StateChartManager.TransitionCondition, int> _numberOfLinesByCondition;

        private static Dictionary<StateChartManager.TransitionCondition, (Color32 startColor, Color32 endColor)> _lineColors =
            new ()
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
            if (!UIGridManager.IsPositionInsideGrid(inputPosition))
                return false;

            ref var hoveredSubCell = ref UIGridManager.GetSubCellOnPosition(inputPosition);

            if (inputIsHorizontal && hoveredSubCell.PlacedHorizontalLine != null)
                return false;
            
            if(!inputIsHorizontal && hoveredSubCell.PlacedVerticalLine != null)
                return false;
            
            var drawStartPosition = UIGridManager.GetStateBorderPosition(sourceState.transform.position, inputPosition, inputDirection);
            var colorIndex = _numberOfLinesByCondition[CurrentTransitionCondition];
            var lineColor = GetLineColor(CurrentTransitionCondition, colorIndex);
            CurrentTransitionLine = sourceState.CreateFirstTransitionLineElement(drawStartPosition, lineColor, inputDirection, CurrentTransitionCondition);
            _currentSubCellPosition =
                UIGridManager.GetNextSubCellPositionInDirection(drawStartPosition, inputDirection, true);
            _previousDrawDirection = inputDirection;

            if (inputIsHorizontal)
            {
                hoveredSubCell.PlacedHorizontalLine = CurrentTransitionLine;
            }
            else
            {
                hoveredSubCell.PlacedVerticalLine = CurrentTransitionLine;
            }

            return true;
        }

        public static bool DrawOnInput(Vector2 inputPosition)
        {
            if (!UIGridManager.IsPositionInsideGrid(inputPosition))
                return true;
            
            if(UIGridManager.IsPositionInsideSubCell(_currentSubCellPosition, inputPosition))
                return true;

            if (UIGridManager.CheckIfStateIsAdjacentToSubCell(_currentSubCellPosition, inputPosition,
                out var hoveredStateElement, out var directionToState))
            {
                // The same stateElement was already hovered
                var hoveredStatePlaceElement = hoveredStateElement.GetComponent<StateUIPlaceElement>();
                if (hoveredStatePlaceElement == null || hoveredStatePlaceElement == DestinationStateElement) 
                    return true;

                // Line returned to source state
                if (directionToState.IsOpposite(_previousDrawDirection))
                    return false;
                
                DestinationStateElement = hoveredStatePlaceElement;
                DestinationStateElement.HighlightAsTransitionDestination();
                var stateCell = DestinationStateElement.GetComponent<StateUIElement>().ConnectedCell;
                (_plugPosition, _plugDirection) =
                    UIGridManager.GetPlugAttributesForAdjacentState(_currentSubCellPosition, stateCell);
                return true;
            }
            
            if (DestinationStateElement != null)
            {
                DestinationStateElement.RemoveHighlight();
                DestinationStateElement = null;
                _plugPosition = Vector2.negativeInfinity;
            }

            if (UIGridManager.CheckIfSubCellIsAdjacentToSubCell(_currentSubCellPosition, inputPosition, out var newDirection))
            {
                if (newDirection.IsOpposite(_previousDrawDirection))
                {
                    CurrentTransitionLine.RemoveLastElement();

                    ref var previousSubCell = ref UIGridManager.GetSubCellOnPosition(_currentSubCellPosition);

                    if (previousSubCell.PlacedHorizontalLine == CurrentTransitionLine)
                        previousSubCell.PlacedHorizontalLine = null;

                    if (previousSubCell.PlacedVerticalLine == CurrentTransitionLine)
                        previousSubCell.PlacedVerticalLine = null;

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

                    ref var hoveredSubCell = ref UIGridManager.GetSubCellOnPosition(inputPosition);
                    ref var previousSubCell = ref UIGridManager.GetSubCellOnPosition(_currentSubCellPosition);

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
                
                _currentSubCellPosition = UIGridManager.GetNextSubCellPositionInDirection(_currentSubCellPosition, newDirection);
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
            DestinationStateElement.RemoveHighlight();
            
            CurrentTransitionLine.CreatePlug(_plugPosition, _plugDirection.ToZRotation());
            ref var plugSubCell = ref UIGridManager.GetSubCellOnPosition(_currentSubCellPosition);
            plugSubCell.PlacedHorizontalLine = CurrentTransitionLine;
            plugSubCell.PlacedVerticalLine = CurrentTransitionLine;
        }

        public static void TransitionLineRemoved(StateChartManager.TransitionCondition condition)
        {
            _numberOfLinesByCondition[condition]--;
        }

        public static void ResetColors()
        {
            _numberOfLinesByCondition = new ()
            {
                { StateChartManager.TransitionCondition.Default, 0 },
                { StateChartManager.TransitionCondition.IsInFrontOfWall, 0 },
                { StateChartManager.TransitionCondition.StandsOnKey, 0 },
                { StateChartManager.TransitionCondition.StandsOnOrange, 0 },
                { StateChartManager.TransitionCondition.StandsOnPurple, 0 },
            };
        }

        public static Color GetLineColor(StateChartManager.TransitionCondition condition, int colorIndex)
        {
            var (color1, color2) = _lineColors[condition];
            return Color32.Lerp(color1, color2, ColorStep * colorIndex);
        }
    }
}
