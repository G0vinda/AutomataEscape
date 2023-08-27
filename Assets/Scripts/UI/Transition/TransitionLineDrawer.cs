using System;
using System.Collections.Generic;
using Helper;
using Robot;
using UI.Grid;
using UI.State;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace UI.Transition
{
    public static class TransitionLineDrawer
    {
        public static UIGridManager UIGridManager { get; set; }
        public static StateChartManager.TransitionCondition CurrentTransitionCondition { get; set; }
        public static TransitionLine CurrentTransitionLine { get; private set; }
        public static StateUIPlaceElement DestinationStateElement { get; set; }

        private static Dictionary<StateChartManager.TransitionCondition, int> _numberOfLinesByCondition;

        private static Dictionary<StateChartManager.TransitionCondition, (Color32 startColor, Color32 endColor)>
            _lineColors =
                new()
                {
                    {
                        StateChartManager.TransitionCondition.Default,
                        (new Color32(0x2E, 0x2E, 0x2E, 0xFF), new Color32(0xE0, 0xE0, 0xE0, 0xFF))
                    },
                    {
                        StateChartManager.TransitionCondition.IsInFrontOfWall,
                        (new Color32(0x92, 0xF8, 0xFF, 0xFF), new Color32(0x10, 0x4B, 0x50, 0xFF))
                    },
                    {
                        StateChartManager.TransitionCondition.StandsOnKey,
                        (new Color32(0x3C, 0xFA, 0x9E, 0xFF), new Color32(0x10, 0x4F, 0x24, 0xFF))
                    },
                    {
                        StateChartManager.TransitionCondition.StandsOnOrange,
                        (new Color32(0xF8, 0x90, 0x41, 0xFF), new Color32(0x59, 0x26, 0x00, 0xFF))
                    },
                    {
                        StateChartManager.TransitionCondition.StandsOnPurple,
                        (new Color32(0xB3, 0x4A, 0xA2, 0xFF), new Color32(0x24, 0x1A, 0x5B, 0xFF))
                    },
                };

        private static readonly float ColorStep = 1f / 9;

        private static SubCell _currentSubCell;
        private static SubCell _startSubCell;
        private static List<SubCell> _currentLinePath;
        private static StateUIElement _currentSourceState;
        private static Vector2 _plugPosition;
        private static Direction _plugDirection;

        public static bool StartDrawingIfSubCellIsFree(Vector2 inputPosition, StateUIElement sourceState,
            Vector2Int sourceStateCoordinates)
        {
            if (!UIGridManager.TryScreenPositionToCellCoordinates(inputPosition, out var hoveredCellCoordinates))
                return false;

            var hoveredCell = UIGridManager.GetCellOnCoordinates(hoveredCellCoordinates);

            var sourceCell = UIGridManager.GetCellOnCoordinates(sourceStateCoordinates);
            if (sourceCell == hoveredCell)
                return false;    
            
            var hoveredSubCell = UIGridManager.GetSubCellOnPosition(inputPosition);
            Vector2 drawStartPosition;
            int colorIndex;
            Color lineColor;
            Direction inputDirection;
            bool inputIsHorizontal;

            if (UIGridManager.CheckIfSubCellIsAdjacentToCell(sourceCell, inputPosition))
            {
                inputDirection = (hoveredCellCoordinates - sourceStateCoordinates).ToDirection();
                inputIsHorizontal = inputDirection == Direction.Left || inputDirection == Direction.Right;

                if (inputIsHorizontal && hoveredSubCell.BlockingHorizontalLine != null)
                    return false;

                if (!inputIsHorizontal && hoveredSubCell.BlockingVerticalLine != null)
                    return false;

                drawStartPosition =
                    UIGridManager.GetStateBorderPosition(sourceState.transform.position, inputPosition);
                colorIndex = _numberOfLinesByCondition[CurrentTransitionCondition];
                lineColor = GetLineColor(CurrentTransitionCondition, colorIndex);
                CurrentTransitionLine = GameManager.Instance.GetUIManager().StartNewTransitionLine(sourceState, drawStartPosition, lineColor,
                    inputDirection, CurrentTransitionCondition);
                _currentSubCell = UIGridManager.GetNextSubCellInDirection(drawStartPosition, inputDirection, true);
                _startSubCell = _currentSubCell;
                _currentLinePath = new List<SubCell> { _startSubCell };
                _currentSourceState = sourceState;

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
            
            var startPath = FindStartPath(hoveredSubCell, sourceCell);
            if (startPath == null)
                return false;

            _startSubCell = startPath[0];

            var startSubCellPosition = UIGridManager.SubCellCoordinatesToScreenPosition(_startSubCell.Coordinates);
            drawStartPosition =
                UIGridManager.GetStateBorderPosition(sourceState.transform.position, startSubCellPosition);
            colorIndex = _numberOfLinesByCondition[CurrentTransitionCondition];
            lineColor = GetLineColor(CurrentTransitionCondition, colorIndex);
            UIGridManager.TryScreenPositionToCellCoordinates(startSubCellPosition, out var startCellCoordinates);
            
            inputDirection = (startCellCoordinates - sourceStateCoordinates).ToDirection();
            CurrentTransitionLine = GameManager.Instance.GetUIManager().StartNewTransitionLine(sourceState, drawStartPosition, lineColor,
                inputDirection, CurrentTransitionCondition);

            inputIsHorizontal = inputDirection == Direction.Left || inputDirection == Direction.Right;
            if(inputIsHorizontal)
            {
                hoveredSubCell.BlockingHorizontalLine = CurrentTransitionLine;
            }
            else
            {
                hoveredSubCell.BlockingVerticalLine = CurrentTransitionLine;
            }
            
            UpdateLinePath(startPath);
            
            _currentSubCell = startPath[^1];
            _currentSourceState = sourceState;

            return true;
        }

        public static bool DrawOnInput(Vector2 inputPosition)
        {
            if (!UIGridManager.IsPositionInsideGrid(inputPosition))
                return true;

            if (UIGridManager.IsPositionInsideSubCell(_currentSubCell, inputPosition)) // Input did not move
                return true;

            var hoveredStateElement = UIGridManager.GetStateUIElementOnPosition(inputPosition);

            if (hoveredStateElement != null)
            {
                // is hovered state start state or already hovered
                var hoveredStatePlaceElement = hoveredStateElement.GetComponent<StateUIPlaceElement>();
                if (hoveredStatePlaceElement == null || hoveredStatePlaceElement == DestinationStateElement)
                    return true;

                // is line completely reverted
                if (hoveredStateElement == _currentSourceState && _currentLinePath.Count < 2)
                    return false;

                // hovered state is valid destination state
                UIGridManager.TryScreenPositionToCellCoordinates(inputPosition, out var stateCoordinates);
                // var statePosition = UIGridManager.CellCoordinatesToScreenPosition(stateCoordinates);
                // var stateCenterSubCell = UIGridManager.GetSubCellOnPosition(statePosition);
                var hoveredSubCellOnState = UIGridManager.GetSubCellOnPosition(inputPosition);

                var newPath = FindPathWithBreakpoints(hoveredSubCellOnState, hoveredStateElement);
                if (newPath == null)
                    return true;

                DestinationStateElement = hoveredStatePlaceElement;
                DestinationStateElement.HighlightAsTransitionDestination();
                var stateCell = DestinationStateElement.GetComponent<StateUIElement>().ConnectedCell;
                (_plugPosition, _plugDirection) =
                    UIGridManager.GetPlugAttributesForAdjacentState(_currentSubCell, stateCell);

                return true;
            }

            if (DestinationStateElement != null)
            {
                DestinationStateElement.RemoveHighlight();
                DestinationStateElement = null;
                _plugPosition = Vector2.negativeInfinity;
            }

            var hoveredSubCell = UIGridManager.GetSubCellOnPosition(inputPosition);
            FindPathWithBreakpoints(hoveredSubCell);

            return true;
        }

        private static List<SubCell> FindStartPath(SubCell destinationSubCell, StateChartCell startCell)
        {
            var startStateUIElement = startCell.PlacedStateElement;
            var startSubCell = UIGridManager.GetCenterSubCellFromCell(startCell);
            
            return new LinePathFinder(startSubCell, destinationSubCell).GetStartPath(startStateUIElement);
        }

        private static List<SubCell> FindPathWithBreakpoints(SubCell destinationSubCell,
            StateUIElement allowedStateUIElement = null)
        {
            if (_currentLinePath.Contains(destinationSubCell))
            {
                var reversedSubCellIndex = _currentLinePath.IndexOf(destinationSubCell);
                var trimLength = _currentLinePath.Count - (reversedSubCellIndex + 1);
                _currentLinePath.RemoveRange(reversedSubCellIndex + 1, trimLength);
                UpdateLinePath(_currentLinePath);
                return _currentLinePath;
            }

            var checkCounter = _currentLinePath.Count - 1;
            SubCell checkedSubCell;
            do
            {
                checkedSubCell = _currentLinePath[checkCounter];
                var newPath =
                    new LinePathFinder(checkedSubCell, destinationSubCell).GetPath(CurrentTransitionLine,
                        allowedStateUIElement);

                if (newPath != null)
                {
                    if (checkCounter > 0)
                        newPath = RemovePathDuplicates(newPath);

                    UpdateLinePath(newPath);

                    return newPath;
                }

                checkCounter = checkCounter > 3 ? checkCounter - 3 : 0;
            } while (checkedSubCell != _startSubCell);

            return null;
        }

        public static List<SubCell> RemovePathDuplicates(List<SubCell> path)
        {
            var currentPathKeepCount = _currentLinePath.Count - 1; // Discard last element
            for (var i = path.Count - 1; i > 0; i--)
            {
                if (!_currentLinePath.Contains(path[i]))
                    continue;

                currentPathKeepCount = _currentLinePath.IndexOf(path[i]) + 1;
                path.RemoveRange(0, i + 1);
                break;
            }

            var newPath = _currentLinePath.GetRange(0, currentPathKeepCount);
            newPath.AddRange(path);
            
            var pathCoordinatesMessage = "-- Path Finding Coordinates --\n";
            for (var i = 0; i < newPath.Count; i++)
            {
                pathCoordinatesMessage += $"({i}) {newPath[i].Coordinates}\n";
            }

            return newPath;
        }

        private static void UpdateLinePath(List<SubCell> newPath)
        {
            if (newPath == null)
                return;

            if (_currentLinePath != null)
            {
                for (var i = 1; i < _currentLinePath.Count; i++)
                {
                    _currentLinePath[i].RemoveBlockingLine(CurrentTransitionLine);
                }
            }

            for (var i = 0; i < newPath.Count - 1; i++)
            {
                if (newPath[i].Coordinates.x != newPath[i + 1].Coordinates.x)
                {
                    newPath[i].BlockingHorizontalLine = CurrentTransitionLine;
                    newPath[i + 1].BlockingHorizontalLine = CurrentTransitionLine;
                    continue;
                }

                if (newPath[i].Coordinates.y != newPath[i + 1].Coordinates.y)
                {
                    newPath[i].BlockingVerticalLine = CurrentTransitionLine;
                    newPath[i + 1].BlockingVerticalLine = CurrentTransitionLine;
                    continue;
                }

                throw new ArgumentException(
                    $"SubCell{newPath[i].Coordinates} and SubCell{newPath[i + 1].Coordinates} are not adjacent");
            }

            CurrentTransitionLine.ParsePathToCreateLine(newPath);

            _currentLinePath = newPath;
            _currentSubCell = _currentLinePath[^1];
        }

        public static void CancelDraw()
        {
            SoundPlayer.Instance.PlayCableRelease();
            if (DestinationStateElement != null)
                DestinationStateElement.RemoveHighlight();

            DestinationStateElement = null;
            _currentSubCell = null;
            _currentSourceState = null;
            _currentLinePath = null;
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
            _numberOfLinesByCondition = new()
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