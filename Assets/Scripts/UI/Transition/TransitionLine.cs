using System;
using System.Collections.Generic;
using DG.Tweening;
using Helper;
using Robot;
using UI.Grid;
using UI.State;
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
        [SerializeField] private float transparencyValue;
        [SerializeField] private float fadeInTime;
        [SerializeField] private float fadeOutTime;
        [SerializeField] private TransitionLine transitionLinePrefab;
        [SerializeField] private Color highlightLineColor;
        [SerializeField] private float highlightLineSizeFactor;

        public StateChartManager.TransitionCondition Condition { get; private set; }
        public StateUIElement stateUIElement;
        
        private List<LineElement> _lineElements = new ();
        private float _elementLength;
        private float _firstElementLength;
        private float _width;
        private RectTransform _plugTransform;
        private Color _solidColor;
        private Color _transparentColor;
        private List<SubCell> _currentPath;
        private TransitionLine _highlightLine;


        public void Initialize(StateUIElement stateUIElement, float firstElementLength, float elementLength, float width, Color lineColor, Direction startDirection, StateChartManager.TransitionCondition condition)
        {
            this.stateUIElement = stateUIElement;
            _elementLength = elementLength;
            _firstElementLength = firstElementLength;
            _width = width;
            Condition = condition;
            _lineElements.Add(CreateFirstElement(startDirection));

            _solidColor = lineColor;
            _transparentColor = _solidColor;
            _transparentColor.a = transparencyValue;
            color = _transparentColor;
            
            UpdateGeometry();
        }

        public void Highlight()
        {
            _highlightLine = Instantiate(transitionLinePrefab, transform.position, Quaternion.identity,
                transform.parent);
            _highlightLine.Initialize(
                null,
                _firstElementLength,
                _elementLength,
                _width * highlightLineSizeFactor,
                highlightLineColor,
                _lineElements[0].Direction,
                StateChartManager.TransitionCondition.Default
                );
            _highlightLine.ParsePathToCreateLine(_currentPath);
            transform.SetAsLastSibling();
            _plugTransform.localScale = highlightLineSizeFactor * Vector3.one;
        }

        public void RemoveHighlight()
        {
            Destroy(_highlightLine.gameObject);
            _plugTransform.localScale = Vector3.one;
        }

        public void ParsePathToCreateLine(List<SubCell> path)
        {
            _currentPath = path;
            _lineElements.RemoveRange(1, _lineElements.Count - 1);
            
            for (var i = 0; i < path.Count - 1; i++)
            {
                var distance = path[i + 1].Coordinates - path[i].Coordinates;
                if(!Mathf.Approximately(distance.magnitude, 1f))
                    Debug.Log("Error found in given path");
                var direction = distance.ToDirection();
                _lineElements.Add(CreateLineElement(direction, _lineElements[^1]));
            }
            
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
            FadeColorToFinish();
            _plugTransform = Instantiate(plugPrefab, position, rotation, transform).rectTransform;
            _plugTransform.sizeDelta = new Vector2(_width * 1.3f, _width * 1.5f);
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

        private void FadeColorToFinish()
        {
            DOVirtual.Color(_transparentColor, _solidColor, fadeInTime, value =>
            {
                color = value;
                UpdateGeometry();
            }).SetEase(Ease.OutCubic);
        }

        public void FadeColorToDestroy()
        {
            if(_plugTransform != null)
                Destroy(_plugTransform.gameObject);
            
            DOVirtual.Float(color.a, 0, fadeOutTime, value =>
            {
                var newColor = color;
                newColor.a = value;
                color = newColor;

                UpdateGeometry();
            }).SetEase(Ease.OutCubic).OnComplete(() => { Destroy(gameObject);});
        }
    }
}
