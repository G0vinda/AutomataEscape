using System;
using System.Collections.Generic;
using Robot;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Transition
{

    public enum Direction
    {
        Up = 0,
        Right,
        Down,
        Left
    }
    
    public class TransitionLine : Graphic
    {
        private class LineElement
        {
            public Direction Direction;
            public Vector3[] VertexPositions;

            public LineElement(Direction dir, Vector3[] vertexPositions)
            {
                Direction = dir;
                VertexPositions = vertexPositions;
            }
        }

        [SerializeField] private Image plugPrefab;

        public StateChartManager.TransitionCondition Condition { get; private set; }
        
        private List<LineElement> _lineElements = new ();
        private float _elementLength;
        private float _firstElementLength;
        private float _width;
        private RectTransform _plugTransform;


        public void Initialize(float firstElementLength, float elementLength, float width, Color lineColor, Direction startDirection, StateChartManager.TransitionCondition condition)
        {
            _elementLength = elementLength;
            _firstElementLength = firstElementLength;
            _width = width;
            color = lineColor;
            Condition = condition;
            _lineElements.Add(CreateFirstElement(startDirection));
            UpdateGeometry();
        }

        public void UpdateSize(float newFirstElementLength, float newElementLength, float newWidth)
        {
            var scaleFactor = newElementLength / _elementLength;
            
            _elementLength = newElementLength;
            _firstElementLength = newFirstElementLength;
            _width = newWidth;

            _lineElements[0] = CreateFirstElement(_lineElements[0].Direction);
            for (var i = 1; i < _lineElements.Count; i++)
            {
                _lineElements[i] = CreateLineElement(_lineElements[i].Direction, _lineElements[i - 1]);
            }

            if (_plugTransform != null)
            {
                _plugTransform.sizeDelta *= scaleFactor;
                var positionDeltaToLine = _plugTransform.position - transform.position;
                positionDeltaToLine *= scaleFactor;
                _plugTransform.position = transform.position + positionDeltaToLine;
            }
            
            UpdateGeometry();
        }

        public bool TryGetLastElementDirection(out Direction lastElementDirection)
        {
            lastElementDirection = 0; // Is invalid if returns false
            if (_lineElements.Count == 0)
                return false;
            
            lastElementDirection = _lineElements[^1].Direction;
            return true;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var vertex = UIVertex.simpleVert;
            vertex.color = color;
            
            for (var i = 0; i < _lineElements.Count; i++)
            {
                var vertexPositions = _lineElements[i].VertexPositions;
                for (var j = 0; j < 4; j++)
                {
                    vertex.position = vertexPositions[j];
                    vh.AddVert(vertex);
                }

                var vertexCount = i * 4;
                vh.AddTriangle(
                    0 + vertexCount,
                    1 + vertexCount,
                    2 + vertexCount);
                vh.AddTriangle(
                    2 + vertexCount,
                    3 + vertexCount,
                    0 + vertexCount);
            }
        }

        private LineElement CreateFirstElement(Direction startDirection)
        {
            var vertexPositions = new Vector3[4];

            switch (startDirection)
            {
                case Direction.Up:
                    vertexPositions[0] = new Vector3(-_width * 0.5f, 0);
                    vertexPositions[1] = new Vector3(-_width * 0.5f, _firstElementLength);
                    vertexPositions[2] = new Vector3(_width * 0.5f, _firstElementLength);
                    vertexPositions[3] = new Vector3(_width * 0.5f, 0);
                    break;
                case Direction.Down:
                    vertexPositions[0] = new Vector3(_width * 0.5f, 0);
                    vertexPositions[1] = new Vector3(_width * 0.5f, -_firstElementLength);
                    vertexPositions[2] = new Vector3(-_width * 0.5f, -_firstElementLength);
                    vertexPositions[3] = new Vector3(-_width * 0.5f, 0);
                    break;
                case Direction.Left:
                    vertexPositions[0] = new Vector3(0, -_width * 0.5f);
                    vertexPositions[1] = new Vector3(-_firstElementLength, -_width * 0.5f);
                    vertexPositions[2] = new Vector3(-_firstElementLength, _width * 0.5f);
                    vertexPositions[3] = new Vector3(0, _width * 0.5f);
                    break;
                case Direction.Right:
                    vertexPositions[0] = new Vector3(0, _width * 0.5f);
                    vertexPositions[1] = new Vector3(_firstElementLength, _width * 0.5f);
                    vertexPositions[2] = new Vector3(_firstElementLength, -_width * 0.5f);
                    vertexPositions[3] = new Vector3(0, -_width * 0.5f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new LineElement(startDirection, vertexPositions);
        }

        public void CreatePlug(Vector2 position, Quaternion rotation)
        {
            _plugTransform = Instantiate(plugPrefab, position, rotation, transform).rectTransform;
            _plugTransform.sizeDelta = new Vector2(_width * 1.3f, _width * 1.5f);
        }

        public void RemoveLastElement()
        {
            _lineElements.RemoveAt(_lineElements.Count - 1);
            UpdateGeometry();
        }

        public void DrawLineElement(Direction direction)
        {
            var newLineElement = CreateLineElement(direction, _lineElements[^1]);
            _lineElements.Add(newLineElement);
            UpdateGeometry();
        }
        
        private LineElement CreateLineElement(Direction direction, LineElement lastLineElement)
        {
            return direction switch
            {
                Direction.Up => CreateUpElement(lastLineElement, _elementLength),
                Direction.Down => CreateDownElement(lastLineElement, _elementLength),
                Direction.Left => CreateLeftElement(lastLineElement, _elementLength),
                Direction.Right => CreateRightElement(lastLineElement, _elementLength),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private LineElement CreateUpElement(LineElement lastElement, float length)
        {
            var lastDrawDirection = lastElement.Direction;
            var vertexPositions = new Vector3[4];
            
            switch (lastDrawDirection)
            {
                case Direction.Up:
                    vertexPositions[0] = lastElement.VertexPositions[1];
                    break;
                case Direction.Left:
                    vertexPositions[0] = lastElement.VertexPositions[2];
                    break;
                case Direction.Right:
                    vertexPositions[0] = lastElement.VertexPositions[1] - new Vector3(_width, 0);
                    break;
                case Direction.Down:
                    // Should be handled by calling method
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            vertexPositions[1] = vertexPositions[0] + new Vector3(0, length);
            vertexPositions[2] = vertexPositions[0] + new Vector3(_width, length);
            vertexPositions[3] = vertexPositions[0] + new Vector3(_width, 0);

            return new LineElement(Direction.Up, vertexPositions);
        }

        private LineElement CreateDownElement(LineElement lastElement, float length)
        {
            var lastDrawDirection = lastElement.Direction;
            var vertexPositions = new Vector3[4];
            
            switch (lastDrawDirection)
            {
                case Direction.Down:
                    vertexPositions[0] = lastElement.VertexPositions[1];
                    break;
                case Direction.Left:
                    vertexPositions[0] = lastElement.VertexPositions[1] + new Vector3(_width, 0);
                    break;
                case Direction.Right:
                    vertexPositions[0] = lastElement.VertexPositions[2];
                    break;
                case Direction.Up:
                    // Should be handled by calling method
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            vertexPositions[1] = vertexPositions[0] - new Vector3(0, length);
            vertexPositions[2] = vertexPositions[0] - new Vector3(_width, length);
            vertexPositions[3] = vertexPositions[0] - new Vector3(_width, 0);

            return new LineElement(Direction.Down, vertexPositions);
        }
        
        private LineElement CreateRightElement(LineElement lastElement, float length)
        {
            var lastDrawDirection = lastElement.Direction;
            var vertexPositions = new Vector3[4];
            
            switch (lastDrawDirection)
            {
                case Direction.Up:
                    vertexPositions[0] = lastElement.VertexPositions[2];
                    break;
                case Direction.Down:
                    vertexPositions[0] = lastElement.VertexPositions[1] + new Vector3(0, _width);
                    break;
                case Direction.Right:
                    vertexPositions[0] = lastElement.VertexPositions[1];
                    break;
                case Direction.Left:
                    // Should be handled by calling method
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            vertexPositions[1] = vertexPositions[0] + new Vector3(length, 0);
            vertexPositions[2] = vertexPositions[0] + new Vector3(length, -_width);
            vertexPositions[3] = vertexPositions[0] + new Vector3(0,-_width);
            
            return new LineElement(Direction.Right, vertexPositions);
        }
        
        private LineElement CreateLeftElement(LineElement lastElement, float length)
        {
            var lastDrawDirection = lastElement.Direction;
            var vertexPositions = new Vector3[4];
            
            switch (lastDrawDirection)
            {
                case Direction.Up:
                    vertexPositions[0] = lastElement.VertexPositions[1] - new Vector3(0, _width);
                    break;
                case Direction.Down:
                    vertexPositions[0] = lastElement.VertexPositions[2];
                    break;
                case Direction.Left:
                    vertexPositions[0] = lastElement.VertexPositions[1];
                    break;
                case Direction.Right:
                    // Should be handled by calling method
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            vertexPositions[1] = vertexPositions[0] + new Vector3(-length, 0);
            vertexPositions[2] = vertexPositions[0] + new Vector3(-length, _width);
            vertexPositions[3] = vertexPositions[0] + new Vector3(0, _width);

            return new LineElement(Direction.Left, vertexPositions);
        }
    }
}
