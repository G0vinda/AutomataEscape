using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using UI.State;
using UI.Transition;
using UnityEngine;

namespace UI.Grid
{
    public class UIGridManager : MonoBehaviour
    {
        [SerializeField] private int numberOfRows;

        private bool _initialized;
        private Dictionary<Vector2Int, StateChartCell> _gridCells = new ();
        private float _cellSize;
        private float _subCellSize;
        private float _gridHeight;
        private Vector2 _bottomLeftPosition;

        public void Initialize(float gridHeight, Vector2 bottomLeftPosition)
        {
            SetGridValues(gridHeight, bottomLeftPosition);

            for (byte x = 0; x < numberOfRows; x++)
            {
                for (byte y = 0; y < numberOfRows; y++)
                {
                    var cellCoordinates = new Vector2Int(x, y); 
                    _gridCells[cellCoordinates] = new StateChartCell();
                }
            }
        }

        private void SetGridValues(float gridHeight, Vector2 bottomLeftPosition)
        {
            _gridHeight = gridHeight;
            _cellSize = _gridHeight / numberOfRows;
            _subCellSize = _cellSize / 3;
            _bottomLeftPosition = bottomLeftPosition;
        }

        public void UpdateGrid(float gridHeight, Vector2 bottomLeftPosition)
        {
            SetGridValues(gridHeight, bottomLeftPosition);

            foreach (var (coordinates, cell) in _gridCells)
            {
                var connectedStateElement = cell.PlacedStateElement;
                if(connectedStateElement == null)
                    continue;

                var newStatePosition = CellCoordinatesToScreenPosition(coordinates);
                
                connectedStateElement.transform.position = newStatePosition;
                connectedStateElement.UpdateScaling();
            }
        }
        
        public bool IsPositionInsideGrid(Vector2 position)
        {
            return position.IsInsideSquare(_bottomLeftPosition, _gridHeight);
        }

        public Vector2 CellCoordinatesToScreenPosition(Vector2Int cellCoordinates)
        {
            var positionOffset = new Vector2(_cellSize * 0.5f, _cellSize * 0.5f);
            return new Vector2(cellCoordinates.x * _cellSize, cellCoordinates.y * _cellSize) + _bottomLeftPosition + positionOffset;
        }
        
        public bool TryScreenPositionToCellCoordinates(Vector2 screenPosition, out Vector2Int coordinates) 
        {
            coordinates = Vector2Int.zero;
            if (!IsPositionInsideGrid(screenPosition))
                return false;
            
            var screenPositionInGrid = screenPosition - _bottomLeftPosition;
            coordinates.x = (int)Mathf.Floor(screenPositionInGrid.x / _cellSize);
            coordinates.y = (int)Mathf.Floor(screenPositionInGrid.y / _cellSize);
            
            return true;
        }

        public Vector2Int GetCoordinatesFromCell(StateChartCell cell)
        {
            return _gridCells.First(gridCell => gridCell.Value == cell).Key;
        }
        
        public StateChartCell GetCellOnCoordinates(Vector2Int cellCoordinates)
        {
            return _gridCells[cellCoordinates];
        }
        

        public ref StateChartCell.SubCell GetSubCellOnPosition(Vector2 position) // Check if position is inside grid before
        {
            if (!TryScreenPositionToCellCoordinates(position, out var parentCellCoordinates))
                throw new ArgumentException();
            
            var parentCell = GetCellOnCoordinates(parentCellCoordinates);
            var parentCellPosition = CellCoordinatesToScreenPosition(parentCellCoordinates);

            var positionDifference = position - parentCellPosition;
            var cellSizeDividedBy6 = _cellSize / 6;
            
            var subCellCoordinates = new Vector2Int(0,0);
            if (positionDifference.x > cellSizeDividedBy6)
            {
                subCellCoordinates.x = 1;
            }else if (positionDifference.x < -cellSizeDividedBy6)
            {
                subCellCoordinates.x = -1;
            }
            
            if (positionDifference.y > cellSizeDividedBy6)
            {
                subCellCoordinates.y = 1;
            }else if (positionDifference.y < -cellSizeDividedBy6)
            {
                subCellCoordinates.y = -1;
            }

            Vector2Int subCellCoordinatesInSubCellPresentation = subCellCoordinates + Vector2Int.one;
            return ref parentCell.GetSubCellOnCoordinates(subCellCoordinatesInSubCellPresentation);
        }

        public bool IsPositionInsideSubCell(Vector2 subCellPosition, Vector2 checkPosition)
        {
            var halfSubCellSize = _subCellSize * 0.5f;
            return checkPosition.x > subCellPosition.x - halfSubCellSize &&
                   checkPosition.x < subCellPosition.x + halfSubCellSize &&
                   checkPosition.y > subCellPosition.y - halfSubCellSize &&
                   checkPosition.y < subCellPosition.y + halfSubCellSize;
        }
        
        public bool CheckIfSubCellIsAdjacentToCell(StateChartCell sourceCell, Vector2 positionOnSubCell)
        {
            if (!TryScreenPositionToCellCoordinates(positionOnSubCell, out var hoveredCellCoordinates))
                return false;
            var sourceCellCoordinates = GetCoordinatesFromCell(sourceCell);
            var coordinateDifference = hoveredCellCoordinates - sourceCellCoordinates;

            if (!Mathf.Approximately(coordinateDifference.magnitude, 1f)) // Cells are not adjacent
                return false;

            var sourceCellPosition = CellCoordinatesToScreenPosition(sourceCellCoordinates);

            if (coordinateDifference.x != 0)
            {
                float cellXBorderPosition;
                if (coordinateDifference.x > 0)
                {
                    cellXBorderPosition = sourceCellPosition.x + _cellSize * 0.5f;
                    return positionOnSubCell.x > cellXBorderPosition &&
                           positionOnSubCell.x < cellXBorderPosition + _subCellSize;
                }
                
                cellXBorderPosition = sourceCellPosition.x - _cellSize * 0.5f;
                return positionOnSubCell.x < cellXBorderPosition &&
                       positionOnSubCell.x > cellXBorderPosition - _subCellSize;
            }
            
            float cellYBorderPosition;
            if (coordinateDifference.y > 0)
            {
                cellYBorderPosition = sourceCellPosition.y + _cellSize * 0.5f;
                return positionOnSubCell.y > cellYBorderPosition &&
                       positionOnSubCell.y < cellYBorderPosition + _subCellSize;
            }
                
            cellYBorderPosition = sourceCellPosition.y - _cellSize * 0.5f;
            return positionOnSubCell.y < cellYBorderPosition &&
                   positionOnSubCell.y > cellYBorderPosition - _subCellSize;
        }

        public bool CheckIfSubCellIsAdjacentToSubCell(Vector2 subCellPosition, Vector2 positionOnOtherSubCell, out Direction direction)
        {
            var halfSubCellSize = _subCellSize * 0.5f;
            var sameColumnAsSubCell = positionOnOtherSubCell.x > subCellPosition.x - halfSubCellSize &&
                                      positionOnOtherSubCell.x < subCellPosition.x + halfSubCellSize;
            var sameRowAsSubCell = positionOnOtherSubCell.y > subCellPosition.y - halfSubCellSize &&
                                   positionOnOtherSubCell.y < subCellPosition.y + halfSubCellSize;
            direction = 0; // Is invalid if returns false

            if (sameColumnAsSubCell)
            {
                if (positionOnOtherSubCell.y > subCellPosition.y &&
                    positionOnOtherSubCell.y < subCellPosition.y + _subCellSize * 1.5f)
                {
                    direction = Direction.Up;
                    return true;
                }

                if (positionOnOtherSubCell.y < subCellPosition.y &&
                    positionOnOtherSubCell.y > subCellPosition.y - _subCellSize * 1.5f)
                {
                    direction = Direction.Down;
                    return true;
                }
                
                return false;
            }

            if (!sameRowAsSubCell) 
                return false;
            
            if (positionOnOtherSubCell.x > subCellPosition.x &&
                positionOnOtherSubCell.x < subCellPosition.x + _subCellSize * 1.5f)
            {
                direction = Direction.Right;
                return true;
            }

            if (positionOnOtherSubCell.x < subCellPosition.x &&
                positionOnOtherSubCell.x > subCellPosition.x - _subCellSize * 1.5f)
            {
                direction = Direction.Left;
                return true;
            }

            return false;
        }

        public bool CheckIfStateIsAdjacentToSubCell(Vector2 subCellPosition, Vector2 positionOnState,
            out StateUIElement state, out Direction directionToState)
        {
            state = null;
            directionToState = 0;

            if (!TryScreenPositionToCellCoordinates(subCellPosition, out var sourceCellCoordinates))
                return false;

            if (!TryScreenPositionToCellCoordinates(positionOnState, out var checkedCoordinates))
                return false;

            var positionDiff = checkedCoordinates - sourceCellCoordinates;
            if (!Mathf.Approximately(positionDiff.magnitude, 1f))
                return false;

            directionToState = positionDiff.ToDirection();
            var subCellOffset = subCellPosition - CellCoordinatesToScreenPosition(sourceCellCoordinates);
            var subCellIsAdjacent = directionToState switch
                {
                    Direction.Up => subCellOffset.y > 0,
                    Direction.Right => subCellOffset.x > 0,
                    Direction.Down => subCellOffset.y < 0,
                    Direction.Left => subCellOffset. x < 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(directionToState), directionToState, null)
                };

            if (!subCellIsAdjacent)
                return false;
            
            state = GetCellOnCoordinates(checkedCoordinates).PlacedStateElement;

            return state != null;
        }

        public List<TransitionLine> GetCellTransitionLines(StateChartCell cell)
        {
            List<TransitionLine> transitionLines = new List<TransitionLine>();
            foreach (var subCell in cell.SubCells)
            {
                transitionLines.AddRange(GetSubCellTransitionLines(subCell));
            }

            return transitionLines.Distinct().ToList();
        }

        private IEnumerable<TransitionLine> GetSubCellTransitionLines(StateChartCell.SubCell subCell)
        {
            List<TransitionLine> transitionLines = new List<TransitionLine>();
            if(subCell.PlacedHorizontalLine != null)
                transitionLines.Add(subCell.PlacedHorizontalLine);
            
            if(subCell.PlacedVerticalLine != null)
                transitionLines.Add(subCell.PlacedVerticalLine);

            return transitionLines.Distinct();
        }

        public (Vector2, Direction) GetPlugAttributesForAdjacentState(Vector2 subCellPosition, StateChartCell stateCell)
        {
            var adjacentStateCoordinates = GetCoordinatesFromCell(stateCell);
            if (!TryScreenPositionToCellCoordinates(subCellPosition, out var sourceCellPosition))
                throw new ArgumentException();
            
            var coordinateDelta = sourceCellPosition - adjacentStateCoordinates;
            var adjacentStatePosition = CellCoordinatesToScreenPosition(adjacentStateCoordinates);

            var plugDirection = coordinateDelta.ToDirection();
            var plugPosition = GetStateBorderPosition(adjacentStatePosition, subCellPosition,
                plugDirection);

            return (plugPosition, plugDirection);
        }

        public Vector2 GetNextSubCellPositionInDirection(Vector2 startPosition, Direction direction, bool fromBorder = false)
        {
            var distanceFactor = fromBorder ? 0.5f : 1;
            return startPosition + direction.ToVector2() * _subCellSize * distanceFactor;
        }
        
        public Vector2 GetStateBorderPosition(Vector2 statePosition, Vector2 inputPosition, Direction drawDirection)
        {
            float xValue, yValue;
            switch (drawDirection)
            {
                case Direction.Up:
                    xValue = 0f;
                    yValue = _cellSize * 0.5f;
                    if (inputPosition.x > statePosition.x + _subCellSize * 0.5f)
                    {
                        xValue = _subCellSize;
                    }
                    else if (inputPosition.x < statePosition.x - _subCellSize * 0.5f)
                    {
                        xValue = -_subCellSize;
                    }
                    return statePosition + new Vector2(xValue, yValue);
                case Direction.Down:
                    xValue = 0f;
                    yValue = -_cellSize * 0.5f;
                    if (inputPosition.x > statePosition.x + _subCellSize * 0.5f)
                    {
                        xValue = _subCellSize;
                    }
                    else if (inputPosition.x < statePosition.x - _subCellSize * 0.5f)
                    {
                        xValue = -_subCellSize;
                    }
                    return statePosition + new Vector2(xValue, yValue);
                case Direction.Left:
                    xValue = -_cellSize * 0.5f;
                    yValue = 0f;
                    if (inputPosition.y > statePosition.y + _subCellSize * 0.5f)
                    {
                        yValue = _subCellSize;
                    }
                    else if (inputPosition.y < statePosition.y - _subCellSize * 0.5f)
                    {
                        yValue = -_subCellSize;
                    }
                    return statePosition + new Vector2(xValue, yValue);
                case Direction.Right:
                    xValue = _cellSize * 0.5f;
                    yValue = 0f;
                    if (inputPosition.y > statePosition.y + _subCellSize * 0.5f)
                    {
                        yValue = _subCellSize;
                    }
                    else if (inputPosition.y < statePosition.y - _subCellSize * 0.5f)
                    {
                        yValue = -_subCellSize;
                    }
                    return statePosition + new Vector2(xValue, yValue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        

        public bool TryGetEmptyCellOnPosition(Vector2 screenPosition, out StateChartCell emptyCell, out Vector2 cellPosition)
        {
            cellPosition = Vector2.zero;
            emptyCell = null;
            
            if (!TryScreenPositionToCellCoordinates(screenPosition, out var cellCoordinates))
                return false;

            var cell = GetCellOnCoordinates(cellCoordinates);
            if (!cell.IsEmpty)
                return false;

            foreach (var adjacentCell in GetAdjacentCells(cellCoordinates))
            {
                if (!adjacentCell.IsEmpty)
                    return false;
            }
            
            emptyCell = cell;
            cellPosition = CellCoordinatesToScreenPosition(cellCoordinates);
            return true;
        }

        public void RemoveStateElementsFromGrid()
        {
            foreach (var (key, stateChartCell) in _gridCells)
            {
                if (stateChartCell.PlacedStateElement == null || 
                    stateChartCell.PlacedStateElement.GetComponent<StateUIPlaceElement>() == null) 
                    continue;
                
                Destroy(stateChartCell.PlacedStateElement.gameObject);
                stateChartCell.RemoveStateElement();
            }
        }

        public List<Vector2> GetCellPositionsAdjacentToStates()
        {
            var positionList = new List<Vector2>();
            foreach (var (coordinates, cell) in _gridCells)
            {
                if(cell.IsEmpty)
                    continue;

                foreach (var adjacentCoordinates in coordinates.GetAdjacentCoordinates())
                {
                    if(_gridCells.ContainsKey(adjacentCoordinates))
                        positionList.Add(CellCoordinatesToScreenPosition(adjacentCoordinates));   
                }
            }

            positionList = positionList.Distinct().ToList();
            return positionList;
        }

        public List<StateChartCell> GetAdjacentCells(Vector2Int coordinates)
        {
            var adjacentCells = new List<StateChartCell>();
            
            foreach (var adjacentCoordinates in coordinates.GetAdjacentCoordinates())
            {
                if(_gridCells.ContainsKey(adjacentCoordinates))
                    adjacentCells.Add(_gridCells[adjacentCoordinates]);    
            }

            return adjacentCells;
        }
        
    }
}
