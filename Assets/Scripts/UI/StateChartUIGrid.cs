using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using UnityEngine;

namespace UI
{
    public class StateChartUIGrid : MonoBehaviour
    {
        [SerializeField] private GameObject gridTestPrefab;
        [SerializeField] private int numberOfRows;
        [SerializeField] private bool drawTestGrid;

        private List<GameObject> _gridTestObjects = new ();
        private bool _initialized;
        private Dictionary<Vector2Int, StateChartCell> _gridCells = new ();
        private float _cellSize;
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
            
            if(drawTestGrid)
                DrawGrid(1);
        }

        private void SetGridValues(float gridHeight, Vector2 bottomLeftPosition)
        {
            _gridHeight = gridHeight;
            _cellSize = _gridHeight / numberOfRows;
            _bottomLeftPosition = bottomLeftPosition;
        }

        public void UpdateGrid(float gridHeight, Vector2 bottomLeftPosition, float zoomFactor)
        {
            SetGridValues(gridHeight, bottomLeftPosition);

            foreach (var stateChartCell in _gridCells)
            {
                var connectedStateElement = stateChartCell.Value.PlacedStateElement;
                if(connectedStateElement == null)
                    continue;

                var newStatePosition = CellToScreenCoordinates(stateChartCell.Key);
                
                connectedStateElement.transform.position = newStatePosition;
                connectedStateElement.ApplyZoomFactor(zoomFactor);
            }
            
            if(drawTestGrid)
                DrawGrid(zoomFactor);
        }

        public Vector3 CellToScreenCoordinates(Vector2Int cellCoordinates)
        {
            var positionOffset = new Vector2(_cellSize * 0.5f, _cellSize * 0.5f);
            return new Vector2(cellCoordinates.x * _cellSize, cellCoordinates.x * _cellSize) + _bottomLeftPosition + positionOffset;
        }
        
        // Check if screenPosition is inside grid before!
        public Vector2Int ScreenToCellCoordinates(Vector2 screenPosition)
        {
            var screenPositionInGrid = screenPosition - _bottomLeftPosition;
            var xCoordinate = (byte)Mathf.Floor(screenPositionInGrid.x / _cellSize);
            var yCoordinate = (byte)Mathf.Floor(screenPositionInGrid.y / _cellSize);
            return new Vector2Int(xCoordinate, yCoordinate);
        }

        public Vector2Int GetCoordinatesFromCell(StateChartCell cell)
        {
            return _gridCells.First(gridCell => gridCell.Value == cell).Key;
        }

        private void DrawGrid(float zoomFactor)
        {
            if (_gridTestObjects.Count != 0)
            {
                foreach (var gridTestObject in _gridTestObjects)
                {
                    Destroy(gridTestObject.gameObject);
                }
                _gridTestObjects.Clear();
            }
            
            var testScaleFactor = GetComponentInParent<StateChartPanel>().GetScaleFactor() * zoomFactor;
            foreach (var stateChartCell in _gridCells)
            {
                var gridTest = Instantiate(gridTestPrefab, CellToScreenCoordinates(stateChartCell.Key), Quaternion.identity, transform);
                ((RectTransform)gridTest.transform).sizeDelta *= testScaleFactor;
                _gridTestObjects.Add(gridTest);
            }
        }

        public bool IsPositionInsideGrid(Vector2 position)
        {
            return position.IsInsideSquare(_bottomLeftPosition, _gridHeight);
        }

        public StateChartCell TryGetEmptyCellOnPosition(Vector2 screenPosition, out Vector3 cellPosition)
        {
            if (IsPositionInsideGrid(screenPosition))
            {
                var cellCoordinates = ScreenToCellCoordinates(screenPosition);
                var cell = _gridCells[cellCoordinates];
                
                if (cell.PlacedStateElement == null)
                {
                    cellPosition = CellToScreenCoordinates(cellCoordinates);
                    return cell;
                }
            }
            
            cellPosition = Vector3.zero;
            return null;
        }

        public StateChartCell GetCellOnCoordinates(Vector2Int cellCoordinates)
        {
            return _gridCells[cellCoordinates];
        }

        public void ClearGridCells()
        {
            foreach (var stateChartCell in _gridCells)
            {
                stateChartCell.Value.RemoveStateElement();
            }
        }
    }
}
