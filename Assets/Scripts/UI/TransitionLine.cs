using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TransitionLine : Graphic
    {
        private class LineElement
        {
            public Vector2 Direction;
            public Vector3[] VertexPositions;

            public LineElement(Vector2 dir, Vector3[] vertexPositions)
            {
                Direction = dir;
                VertexPositions = vertexPositions;
            }
        }

        private List<LineElement> _lineElements = new ();
        private float _elementLength;
        private float _width;
        private bool _downScaled;
        

        public void Initialize(float elementLength, float width, Vector2 startDirection, float scaleFactor)
        {
            if (!_downScaled)
            {
                transform.localScale *= scaleFactor;
                _downScaled = true;
            }
            
            _elementLength = elementLength;
            _width = width;
            DrawFirstElement(startDirection);
            SetAllDirty();
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

        private void DrawFirstElement(Vector2 startDirection)
        {
            var vertexPositions = new Vector3[4];

            if (startDirection.Equals(Vector2.right))
            {
                vertexPositions[0] = new Vector3(0, _width * 0.5f);
                vertexPositions[1] = new Vector3(_elementLength, _width * 0.5f);
                vertexPositions[2] = new Vector3(_elementLength, -_width * 0.5f);
                vertexPositions[3] = new Vector3(0, -_width * 0.5f);
            }
            else if (startDirection.Equals(Vector2.left))
            {
                vertexPositions[0] = new Vector3(0, -_width * 0.5f);
                vertexPositions[1] = new Vector3(-_elementLength, -_width * 0.5f);
                vertexPositions[2] = new Vector3(-_elementLength, _width * 0.5f);
                vertexPositions[3] = new Vector3(0, _width * 0.5f);
            }
            else if (startDirection.Equals(Vector2.up))
            {
                vertexPositions[0] = new Vector3(-_width * 0.5f, 0);
                vertexPositions[1] = new Vector3(-_width * 0.5f, _elementLength);
                vertexPositions[2] = new Vector3(_width * 0.5f, _elementLength);
                vertexPositions[3] = new Vector3(_width * 0.5f, 0);
            }
            else if (startDirection.Equals(Vector2.down))
            {
                vertexPositions[0] = new Vector3(_width * 0.5f, 0);
                vertexPositions[1] = new Vector3(_width * 0.5f, -_elementLength);
                vertexPositions[2] = new Vector3(-_width * 0.5f, -_elementLength);
                vertexPositions[3] = new Vector3(-_width * 0.5f, 0);
            }
            
            _lineElements.Add(new LineElement(startDirection, vertexPositions));
        }

        public void DrawLineElement(Vector2 direction)
        {
            var lastLineElement = _lineElements.ElementAt(_lineElements.Count - 1);
            var lastDrawDirection = lastLineElement.Direction;

            if (direction == -lastDrawDirection)
            {
                _lineElements.Remove(lastLineElement);
                Debug.Log($"Line removed, {_lineElements.Count} left");
                SetAllDirty();
                return;
            }
            
            LineElement newLineElement;
            if (direction == Vector2.up)
            {
                newLineElement = CreateUpElement(lastLineElement, _elementLength);
            }
            else if (direction == Vector2.down)
            {
                newLineElement = CreateDownElement(lastLineElement, _elementLength);
            }
            else if (direction == Vector2.right)
            {
                newLineElement = CreateRightElement(lastLineElement, _elementLength);
            }
            else if (direction == Vector2.left)
            {
                newLineElement = CreateLeftElement(lastLineElement, _elementLength);
            }
            else
            {
                newLineElement = null;
            }
                
            _lineElements.Add(newLineElement);
            SetAllDirty();
        }

        public void Clear()
        {
            _lineElements.Clear();
            SetAllDirty();
        }

        private LineElement CreateUpElement(LineElement lastElement, float length)
        {
            var lastDrawDirection = lastElement.Direction;
            var vertexPositions = new Vector3[4];
            
            if (lastDrawDirection == Vector2.up)
            {
                vertexPositions[0] = lastElement.VertexPositions[1];
            }
            if (lastDrawDirection == Vector2.left)
            {
                vertexPositions[0] = lastElement.VertexPositions[2];
            }
            if (lastDrawDirection == Vector2.right)
            {
                vertexPositions[0] = lastElement.VertexPositions[1] - new Vector3(_width, 0);
            }
            vertexPositions[1] = vertexPositions[0] + new Vector3(0, length);
            vertexPositions[2] = vertexPositions[0] + new Vector3(_width, length);
            vertexPositions[3] = vertexPositions[0] + new Vector3(_width, 0);

            return new LineElement(Vector2.up, vertexPositions);
        }

        private LineElement CreateDownElement(LineElement lastElement, float length)
        {
            var lastDrawDirection = lastElement.Direction;
            var vertexPositions = new Vector3[4];
            
            if (lastDrawDirection == Vector2.down)
            {
                vertexPositions[0] = lastElement.VertexPositions[1];
            }
            if (lastDrawDirection == Vector2.left)
            {
                vertexPositions[0] = lastElement.VertexPositions[1] + new Vector3(_width, 0);
            }
            if (lastDrawDirection == Vector2.right)
            {
                vertexPositions[0] = lastElement.VertexPositions[2];
            }
            vertexPositions[1] = vertexPositions[0] - new Vector3(0, length);
            vertexPositions[2] = vertexPositions[0] - new Vector3(_width, length);
            vertexPositions[3] = vertexPositions[0] - new Vector3(_width, 0);

            return new LineElement(Vector2.down, vertexPositions);
        }
        
        private LineElement CreateRightElement(LineElement lastElement, float length)
        {
            var lastDrawDirection = lastElement.Direction;
            var vertexPositions = new Vector3[4];
            
            if (lastDrawDirection == Vector2.up)
            {
                vertexPositions[0] = lastElement.VertexPositions[2];
            }
            if (lastDrawDirection == Vector2.down)
            {
                vertexPositions[0] = lastElement.VertexPositions[1] + new Vector3(0, _width);
            }
            if (lastDrawDirection == Vector2.right)
            {
                vertexPositions[0] = lastElement.VertexPositions[1];
            }
            vertexPositions[1] = vertexPositions[0] + new Vector3(length, 0);
            vertexPositions[2] = vertexPositions[0] + new Vector3(length, -_width);
            vertexPositions[3] = vertexPositions[0] + new Vector3(0,-_width);
            
            return new LineElement(Vector2.right, vertexPositions);
        }
        
        private LineElement CreateLeftElement(LineElement lastElement, float length)
        {
            var lastDrawDirection = lastElement.Direction;
            var vertexPositions = new Vector3[4];
            
            if (lastDrawDirection == Vector2.up)
            {
                vertexPositions[0] = lastElement.VertexPositions[1] - new Vector3(0, _width);
            }
            if (lastDrawDirection == Vector2.left)
            {
                vertexPositions[0] = lastElement.VertexPositions[1];
            }
            if (lastDrawDirection == Vector2.down)
            {
                vertexPositions[0] = lastElement.VertexPositions[2];
            }
            vertexPositions[1] = vertexPositions[0] + new Vector3(-length, 0);
            vertexPositions[2] = vertexPositions[0] + new Vector3(-length, _width);
            vertexPositions[3] = vertexPositions[0] + new Vector3(0, _width);

            return new LineElement(Vector2.left, vertexPositions);
        }

        private void ResizeLastElement(float newLength)
        {
            var lastElement = _lineElements.ElementAt(_lineElements.Count - 1);
            var lastDirection = lastElement.Direction;
            var secondLastElement = _lineElements.ElementAt(_lineElements.Count - 2);
            LineElement replaceElement;
            
            if (lastDirection == Vector2.up)
            {
                replaceElement = CreateUpElement(secondLastElement, newLength);
            }
            else if (lastDirection == Vector2.down)
            {
                replaceElement = CreateDownElement(secondLastElement, newLength);
            }
            else if (lastDirection == Vector2.right)
            {
                replaceElement = CreateRightElement(secondLastElement, newLength);
            }
            else
            {
                replaceElement = CreateLeftElement(secondLastElement, newLength);
            }

            _lineElements.Remove(lastElement);
            _lineElements.Add(replaceElement);
        }

        public void DrawLineToSlot(Vector3 linePos, Vector3 slotPos, Vector2 slotDir)
        {
            var lastLineElement = _lineElements.ElementAt(_lineElements.Count - 1);
            var lineDir = lastLineElement.Direction;
            var lineDirIsHorizontal = Mathf.Abs(lineDir.x) > 0;

            if (lineDir - slotDir == Vector2.zero || lineDir + slotDir == Vector2.zero) // Directions are opposite
            {
                if (lineDirIsHorizontal)
                {
                    var distance1 = Mathf.Abs(slotPos.y - linePos.y);
                    var line1Length = StateChartUIManager.Instance.ScaleFloat(distance1);
                    var line1 = slotPos.y > linePos.y
                        ? CreateUpElement(lastLineElement, line1Length)
                        : CreateDownElement(lastLineElement, line1Length);
                    _lineElements.Add(line1);
                    
                    var distance2 = Mathf.Abs(slotPos.x - linePos.x);
                    var line2Length = StateChartUIManager.Instance.ScaleFloat(distance2);
                    var line2 = slotPos.x > linePos.x 
                        ? CreateRightElement(line1, line2Length)
                        : CreateLeftElement(line1, line2Length);
                    _lineElements.Add(line2);
                }
                else
                {
                    var distance1 = Mathf.Abs(slotPos.x - linePos.x);
                    var line1Length = StateChartUIManager.Instance.ScaleFloat(distance1);
                    var line1 = slotPos.x > linePos.x 
                        ? CreateRightElement(lastLineElement, line1Length)
                        : CreateLeftElement(lastLineElement, line1Length);
                    _lineElements.Add(line1);

                    var distance2 = Mathf.Abs(slotPos.y - linePos.y);
                    var line2Length = StateChartUIManager.Instance.ScaleFloat(distance2);
                    var line2 = slotPos.y > linePos.y 
                        ? CreateUpElement(line1, line2Length)
                        : CreateDownElement(line1, line2Length);
                    _lineElements.Add(line2);
                }
            }   
            else if (Vector2.Dot(lineDir, slotDir) == 0) // Directions are orthogonal to each other
            {
                var xDiff = slotPos.x - linePos.x;
                var yDiff = slotPos.y - linePos.y;
                var scaledXDiff = StateChartUIManager.Instance.ScaleFloat(xDiff);
                var scaledYDiff = StateChartUIManager.Instance.ScaleFloat(yDiff);

                if (lineDirIsHorizontal)
                {
                    var newLength = lineDir.x > 0 
                        ? scaledXDiff + 0.5f * _width + _elementLength 
                        : -scaledXDiff + 0.5f * _width + _elementLength;
                    ResizeLastElement(newLength);

                    var lastElement = _lineElements.ElementAt(_lineElements.Count - 1);
                    var line2Length = StateChartUIManager.Instance.ScaleFloat(Mathf.Abs(yDiff)) - 0.5f * _width;
                    var finishElement = yDiff > 0 ? CreateUpElement(lastElement, line2Length) : CreateDownElement(lastElement, line2Length);
                    _lineElements.Add(finishElement);
                }
                else
                {
                    var newLength = lineDir.y > 0 
                        ? scaledYDiff + 0.5f * _width + _elementLength 
                        : -scaledYDiff + 0.5f * _width + _elementLength;
                    ResizeLastElement(newLength);
                    
                    var lastElement = _lineElements.ElementAt(_lineElements.Count - 1);
                    var line2Length = StateChartUIManager.Instance.ScaleFloat(Mathf.Abs(xDiff)) - 0.5f * _width;
                    var finishElement = xDiff > 0 ? CreateRightElement(lastElement, line2Length) : CreateLeftElement(lastElement, line2Length);
                    _lineElements.Add(finishElement);
                }
            }
            
            SetAllDirty();
        }
    }
}
