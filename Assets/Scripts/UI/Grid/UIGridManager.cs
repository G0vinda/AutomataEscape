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

            SubCell.CreateSubCellGrid();
        }

        private void SetGridValues(float gridHeight, Vector2 bottomLeftPosition)
        {
            _gridHeight = gridHeight;
            _cellSize = _gridHeight / numberOfRows;
            _subCellSize = _cellSize / 3;
            _bottomLeftPosition = bottomLeftPosition;
        }

        public float GetSubCellSize()
        {
            return _subCellSize;
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

                if (connectedStateElement.TryGetComponent(out StartStateUIElement startState))
                {
                    startState.UpdateFrontImagScaling();
                }
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

        public Vector2Int CellCoordinatesToSubCellCoordinates(Vector2Int cellCoordinates)
        {
            return cellCoordinates * 3 + Vector2Int.one;
        }

        private Vector2Int SubCellCoordinatesToCellCoordinates(Vector2Int subCellCoordinates)
        {
            var x = (int)Mathf.Floor(subCellCoordinates.x / 3f);
            var y = (int)Mathf.Floor(subCellCoordinates.y / 3f);
            return new Vector2Int(x, y);
        }

        public Vector2 SubCellCoordinatesToScreenPosition(Vector2Int subCellCoordinates)
        {
            var bottomLeftOffset = _bottomLeftPosition + Vector2.one * _subCellSize * 0.5f;
            return bottomLeftOffset + (Vector2)subCellCoordinates * _subCellSize;
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

        public SubCell GetSubCellOnPosition(Vector2 position) // Check if position is inside grid before
        {
            if (!IsPositionInsideGrid(position))
                return null;
            
            var relativePosition = position - _bottomLeftPosition;
            var xCoordinate = (int)Mathf.Floor(relativePosition.x / _subCellSize);
            var yCoordinate = (int)Mathf.Floor(relativePosition.y / _subCellSize);
            var subCellCoordinates = new Vector2Int(xCoordinate, yCoordinate);

            return SubCell.Grid[subCellCoordinates];
        }

        public bool IsPositionInsideSubCell(SubCell subCell, Vector2 checkPosition)
        {
            var halfSubCellSize = _subCellSize * 0.5f;
            var subCellPosition = _bottomLeftPosition + new Vector2(
                                        halfSubCellSize + subCell.Coordinates.x * _subCellSize,
                                        halfSubCellSize + subCell.Coordinates.y * _subCellSize);
            
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
        
        public SubCell GetCenterSubCellFromCell(StateChartCell cell)
        {
            var cellCoordinates = GetCoordinatesFromCell(cell);
            var cellPosition = CellCoordinatesToScreenPosition(cellCoordinates);

            return GetSubCellOnPosition(cellPosition);
        }

        public List<TransitionLine> GetCellTransitionLines(StateChartCell cell)
        {
            var transitionLines = new List<TransitionLine>();

            var cellCoordinates = GetCoordinatesFromCell(cell);
            var leftSubCellCoordinate = CellCoordinatesToSubCellCoordinates(cellCoordinates) - Vector2Int.one;
            var currentCoordinates = leftSubCellCoordinate;
            for (var i = 0; i < 3; i++)
            {
                currentCoordinates = leftSubCellCoordinate + Vector2Int.right * i;
                for (var j = 0; j < 3; j++)
                {
                    transitionLines.AddRange(GetSubCellTransitionLines(SubCell.Grid[currentCoordinates]));
                    currentCoordinates += Vector2Int.up;
                }
            }

            return transitionLines.Distinct().ToList();
        }

        public StateUIElement GetStateUIElementOnPosition(Vector2 position)
        {
            if (!TryScreenPositionToCellCoordinates(position, out var cellCoordinates))
                return null;

            return _gridCells[cellCoordinates].PlacedStateElement;
        }

        public IEnumerable<TransitionLine> GetSubCellTransitionLines(SubCell subCell)
        {
            List<TransitionLine> transitionLines = new List<TransitionLine>();
            if(subCell.BlockingHorizontalLine != null)
                transitionLines.Add(subCell.BlockingHorizontalLine);
            
            if(subCell.BlockingVerticalLine != null)
                transitionLines.Add(subCell.BlockingVerticalLine);

            return transitionLines.Distinct();
        }

        public (Vector2, Direction) GetPlugAttributesForAdjacentState(SubCell subCell, StateChartCell stateCell)
        {
            var adjacentStateCoordinates = GetCoordinatesFromCell(stateCell);
            var sourceCellCoordinates = SubCellCoordinatesToCellCoordinates(subCell.Coordinates);

            var coordinateDelta = sourceCellCoordinates - adjacentStateCoordinates;
            var adjacentStatePosition = CellCoordinatesToScreenPosition(adjacentStateCoordinates);

            var plugDirection = coordinateDelta.ToDirection();
            var subCellPosition = SubCellCoordinatesToScreenPosition(subCell.Coordinates);
            var plugPosition = GetStateBorderPosition(adjacentStatePosition, subCellPosition);

            return (plugPosition, plugDirection);
        }
        
        public SubCell GetNextSubCellInDirection(Vector2 startPosition, Direction direction, bool fromBorder = false)
        {
            var distanceFactor = fromBorder ? 0.5f : 1;
            return GetSubCellOnPosition(startPosition + direction.ToVector2() * _subCellSize * distanceFactor);
        }
        
        public Vector2 GetStateBorderPosition(Vector2 statePosition, Vector2 inputPosition)
        {
            var positionDiff = inputPosition - statePosition;
            Direction drawDirection;
            if (positionDiff.x > _subCellSize * 1.5f)
                drawDirection = Direction.Right;
            else if (positionDiff.x < -_subCellSize * 1.5f)
                drawDirection = Direction.Left;
            else if (positionDiff.y > _subCellSize * 1.5f)
                drawDirection = Direction.Up;
            else if (positionDiff.y < -_subCellSize * 1.5f)
                drawDirection = Direction.Down;
            else
                throw new ArgumentException("inputPosition is not adjacent to state");
            
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

        public void RemoveStateElementFromGrid(StateUIElement stateUIElement)
        {
            var cell = stateUIElement.ConnectedCell;
            cell.RemoveStateElement();
            
            var cellCoordinates = GetCoordinatesFromCell(cell);
            var bottomLeftSubCellCoordinates = CellCoordinatesToSubCellCoordinates(cellCoordinates) - Vector2Int.one;
            for (var x = 0; x < 3; x++)
            {
                for (var y = 0; y < 3; y++)
                {
                    var currentCoordinates = bottomLeftSubCellCoordinates + new Vector2Int(x, y);
                    SubCell.Grid[currentCoordinates].BlockingState = null;
                }
            }
            
        }

        public void RemoveStateElementsFromGrid()
        {
            foreach (var (_, stateChartCell) in _gridCells)
            {
                if (stateChartCell.PlacedStateElement == null || 
                    stateChartCell.PlacedStateElement.GetComponent<StateUIPlaceElement>() == null) 
                    continue;

                var stateElement = stateChartCell.PlacedStateElement;
                RemoveStateElementFromGrid(stateElement);
                Destroy(stateElement.gameObject);
            }
        }

        public void PlaceStateElementOnCell(StateUIElement stateUIElement, StateChartCell cell)
        {
            cell.PlaceStateElement(stateUIElement);
            var cellCoordinates = GetCoordinatesFromCell(cell);
            var bottomLeftSubCellCoordinates = CellCoordinatesToSubCellCoordinates(cellCoordinates) - Vector2Int.one;
            for (var x = 0; x < 3; x++)
            {
                for (var y = 0; y < 3; y++)
                {
                    var currentCoordinates = bottomLeftSubCellCoordinates + new Vector2Int(x, y);
                    SubCell.Grid[currentCoordinates].BlockingState = stateUIElement;
                }
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
