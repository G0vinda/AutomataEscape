using System.Collections.Generic;
using Helper;
using PlasticPipe.PlasticProtocol.Messages;
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
        private static SubCell _currentSubCell;
        private static SubCell _startSubCell;
        private static StateUIElement _currentSourceState;
        private static Direction _previousDrawDirection;
        private static Vector2 _plugPosition;
        private static Direction _plugDirection;
        
        public static bool StartDrawingIfSubCellIsFree(Vector2 inputPosition, Direction inputDirection, StateUIElement sourceState)
        {
            var inputIsHorizontal = inputDirection == Direction.Left || inputDirection == Direction.Right;
            if (!UIGridManager.IsPositionInsideGrid(inputPosition))
                return false;

            var hoveredSubCell = UIGridManager.GetSubCellOnPosition(inputPosition);

            if (inputIsHorizontal && hoveredSubCell.BlockingHorizontalLine != null)
                return false;
            
            if (!inputIsHorizontal && hoveredSubCell.BlockingVerticalLine != null)
                return false;
            
            var drawStartPosition = UIGridManager.GetStateBorderPosition(sourceState.transform.position, inputPosition, inputDirection);
            var colorIndex = _numberOfLinesByCondition[CurrentTransitionCondition];
            var lineColor = GetLineColor(CurrentTransitionCondition, colorIndex);
            CurrentTransitionLine = sourceState.CreateFirstTransitionLineElement(drawStartPosition, lineColor, inputDirection, CurrentTransitionCondition);
            _currentSubCellPosition =
                UIGridManager.GetNextSubCellPositionInDirection(drawStartPosition, inputDirection, true);
            _currentSubCell = UIGridManager.GetNextSubCellInDirection(drawStartPosition, inputDirection, true);
            _startSubCell = _currentSubCell;
            _currentSourceState = sourceState;
            _previousDrawDirection = inputDirection;

            if (inputIsHorizontal)
            {
                hoveredSubCell.BlockingHorizontalLine = CurrentTransitionLine;
            }
            else
            {
                hoveredSubCell.BlockingVerticalLine = CurrentTransitionLine;
            }

            SoundPlayer.Instance.PlayCableStart();
            return true;
        }

        public static bool DrawOnInput(Vector2 inputPosition)
        {
            if (!UIGridManager.IsPositionInsideGrid(inputPosition))
                return true;
            
            if(UIGridManager.IsPositionInsideSubCell(_currentSubCell, inputPosition)) // Input did not move
                return true;

            var hoveredStateElement = UIGridManager.GetStateUIElementOnPosition(inputPosition);
            LinePathFinder pathFinder;
            
            if (hoveredStateElement != null)
            {
                // is hovered state start state or already hovered
                var hoveredStatePlaceElement = hoveredStateElement.GetComponent<StateUIPlaceElement>();
                if (hoveredStatePlaceElement == null || hoveredStatePlaceElement == DestinationStateElement) 
                    return true;
                
                // is hovered state source state
                // TODO: instead of canceling this case could be handled otherwise
                if (hoveredStateElement == _currentSourceState)
                    return false;
                
                // hovered state is valid destination state
                UIGridManager.TryScreenPositionToCellCoordinates(inputPosition, out var stateCoordinates);
                var statePosition = UIGridManager.CellCoordinatesToScreenPosition(stateCoordinates);
                var stateCenterSubCell = UIGridManager.GetSubCellOnPosition(statePosition);
                pathFinder = new LinePathFinder(SubCell.Grid, _startSubCell, stateCenterSubCell, CurrentTransitionLine, hoveredStateElement);
                var linePath = pathFinder.Path;
                CurrentTransitionLine.ParsePathToCreateLine(linePath);

                _currentSubCell = linePath[^1]; 
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
            
            var hoveredSubCell = UIGridManager.GetSubCellOnPosition(inputPosition);
            pathFinder = new LinePathFinder(SubCell.Grid, _startSubCell, hoveredSubCell, CurrentTransitionLine);
            CurrentTransitionLine.ParsePathToCreateLine(pathFinder.Path);

            _currentSubCell = hoveredSubCell;
            return true;
        }

        public static void CancelDraw()
        {
            SoundPlayer.Instance.PlayCableRelease();
            if(DestinationStateElement != null)
                DestinationStateElement.RemoveHighlight();

            DestinationStateElement = null;
            _currentSubCell = null;
            _currentSourceState = null;
        }

        public static void FinishLine()
        {
            _numberOfLinesByCondition[CurrentTransitionCondition]++;
            DestinationStateElement.RemoveHighlight();
            SoundPlayer.Instance.PlayCableConnect();
            
            CurrentTransitionLine.CreatePlug(_plugPosition, _plugDirection.ToZRotation());
            
            _currentSubCell.BlockingHorizontalLine = CurrentTransitionLine;
            _currentSubCell.BlockingVerticalLine = CurrentTransitionLine;
            
            DestinationStateElement = null;
            _currentSubCell = null;
            _currentSourceState = null;
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
