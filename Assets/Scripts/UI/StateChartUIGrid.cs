using System;
using System.Collections.Generic;
using Helper;
using UnityEngine;

namespace UI
{
    public struct ByteCoordinates
    {
        public ByteCoordinates(byte x, byte y)
        {
            X = x;
            Y = y;
        }
            
        public byte X { get; set; }
        public byte Y { get; set; }
    }
    public class StateChartUIGrid : MonoBehaviour
    {
        [SerializeField] private GameObject gridTestPrefab;
        [SerializeField] private int numberOfRows;
        [SerializeField] private bool drawTestGrid;

        private bool _initialized;
        private Dictionary<ByteCoordinates, StateChartCell> _gridCells = new ();
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
                    var cellCoordinates = new ByteCoordinates(x, y); 
                    _gridCells[cellCoordinates] = new StateChartCell();
                }
            }
            
            if(drawTestGrid)
                DrawGrid();
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
                if (connectedStateElement.TryGetComponent<StateUIPlaceElement>(out var connectedStatePlaceElement))
                {
                    connectedStatePlaceElement.SetPosition(newStatePosition);
                }
                else
                {
                    connectedStateElement.transform.position = newStatePosition;   
                }
                connectedStateElement.SetSizeToCellSize(zoomFactor);
            }
        }

        private Vector3 CellToScreenCoordinates(ByteCoordinates cellCoordinates)
        {
            var positionOffset = new Vector2(_cellSize * 0.5f, _cellSize * 0.5f);
            return new Vector2(cellCoordinates.X * _cellSize, cellCoordinates.Y * _cellSize) + _bottomLeftPosition + positionOffset;
        }
        
        // Check if screenPosition is inside grid before!
        private ByteCoordinates ScreenToCellCoordinates(Vector2 screenPosition)
        {
            var screenPositionInGrid = screenPosition - _bottomLeftPosition;
            var xCoordinate = (byte)Mathf.Floor(screenPositionInGrid.x / _cellSize);
            var yCoordinate = (byte)Mathf.Floor(screenPositionInGrid.y / _cellSize);
            return new ByteCoordinates(xCoordinate, yCoordinate);
        }

        private void DrawGrid()
        {
            var testScaleFactor = GetComponentInParent<StateChartPanel>().GetScaleFactor();
            foreach (var stateChartCell in _gridCells)
            {
                var gridTest = Instantiate(gridTestPrefab, CellToScreenCoordinates(stateChartCell.Key), Quaternion.identity, transform);
                gridTest.transform.localScale *= testScaleFactor;
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

        public StateChartCell GetCellOnCoordinates(ByteCoordinates cellCoordinates, out Vector3 cellPosition)
        {
            cellPosition = CellToScreenCoordinates(cellCoordinates);
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
