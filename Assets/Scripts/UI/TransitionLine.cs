using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
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

        private List<LineElement> _lineElements = new ();
        private float _elementLength;
        private float _firstElementLength;
        private float _width;


        public void Initialize(float firstElementLength, float elementLength, float width, Direction startDirection)
        {
            transform.rotation = startDirection.ToZRotation();
            _elementLength = elementLength;
            _firstElementLength = firstElementLength;
            _width = width;
            _lineElements.Add(CreateFirstElement(startDirection));
            SetAllDirty();
        }

        public void UpdateSize(float firstElementLength, float elementLength, float width)
        {
            _elementLength = elementLength;
            _firstElementLength = firstElementLength;
            _width = width;

            if (_lineElements.Count > 0) // is this if clause needed?
            {
                _lineElements[0] = CreateFirstElement(_lineElements[0].Direction);
                SetAllDirty();   
            }
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
            startDirection = Direction.Up;
            
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

        public void DrawLineElement(Direction direction)
        {
            var lastLineElement = _lineElements.ElementAt(_lineElements.Count - 1);
            var lastDrawDirection = lastLineElement.Direction;

            if (false)// Todo: this needs to be reworked direction == -lastDrawDirection)
            {
                _lineElements.Remove(lastLineElement);
                Debug.Log($"Line removed, {_lineElements.Count} left");
                SetAllDirty();
                return;
            }
            
            LineElement newLineElement;
            switch (direction)
            {
                case Direction.Up:
                    newLineElement = CreateUpElement(lastLineElement, _elementLength);
                    break;
                case Direction.Down:
                    newLineElement = CreateDownElement(lastLineElement, _elementLength);
                    break;
                case Direction.Left:
                    newLineElement = CreateLeftElement(lastLineElement, _elementLength);
                    break;
                case Direction.Right:
                    newLineElement = CreateRightElement(lastLineElement, _elementLength);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        // This will be probably not be needed anymore
        // private void ResizeLastElement(float newLength)
        // {
        //     var lastElement = _lineElements.ElementAt(_lineElements.Count - 1);
        //     var lastDirection = lastElement.Direction;
        //     var secondLastElement = _lineElements.ElementAt(_lineElements.Count - 2);
        //     LineElement replaceElement;
        //     
        //     if (lastDirection == Vector2.up)
        //     {
        //         replaceElement = CreateUpElement(secondLastElement, newLength);
        //     }
        //     else if (lastDirection == Vector2.down)
        //     {
        //         replaceElement = CreateDownElement(secondLastElement, newLength);
        //     }
        //     else if (lastDirection == Vector2.right)
        //     {
        //         replaceElement = CreateRightElement(secondLastElement, newLength);
        //     }
        //     else
        //     {
        //         replaceElement = CreateLeftElement(secondLastElement, newLength);
        //     }
        //
        //     _lineElements.Remove(lastElement);
        //     _lineElements.Add(replaceElement);
        // }

        // This will need a rework
        // public void DrawLineToSlot(Vector3 linePos, Vector3 slotPos, Vector2 slotDir)
        // {
        //     var lastLineElement = _lineElements.ElementAt(_lineElements.Count - 1);
        //     var lineDir = lastLineElement.Direction;
        //     var lineDirIsHorizontal = Mathf.Abs(lineDir.x) > 0;
        //     var uiManager = GameManager.Instance.GetUIManager();
        //
        //     if (lineDir - slotDir == Vector2.zero || lineDir + slotDir == Vector2.zero) // Directions are opposite
        //     {
        //         if (lineDirIsHorizontal)
        //         {
        //             var distance1 = Mathf.Abs(slotPos.y - linePos.y);
        //             var line1Length = uiManager.UnscaleFloat(distance1);
        //             var line1 = slotPos.y > linePos.y
        //                 ? CreateUpElement(lastLineElement, line1Length)
        //                 : CreateDownElement(lastLineElement, line1Length);
        //             _lineElements.Add(line1);
        //             
        //             var distance2 = Mathf.Abs(slotPos.x - linePos.x);
        //             var line2Length = uiManager.UnscaleFloat(distance2);
        //             var line2 = slotPos.x > linePos.x 
        //                 ? CreateRightElement(line1, line2Length)
        //                 : CreateLeftElement(line1, line2Length);
        //             _lineElements.Add(line2);
        //         }
        //         else
        //         {
        //             var distance1 = Mathf.Abs(slotPos.x - linePos.x);
        //             var line1Length = uiManager.UnscaleFloat(distance1);
        //             var line1 = slotPos.x > linePos.x 
        //                 ? CreateRightElement(lastLineElement, line1Length)
        //                 : CreateLeftElement(lastLineElement, line1Length);
        //             _lineElements.Add(line1);
        //
        //             var distance2 = Mathf.Abs(slotPos.y - linePos.y);
        //             var line2Length = uiManager.UnscaleFloat(distance2);
        //             var line2 = slotPos.y > linePos.y 
        //                 ? CreateUpElement(line1, line2Length)
        //                 : CreateDownElement(line1, line2Length);
        //             _lineElements.Add(line2);
        //         }
        //     }   
        //     else if (Vector2.Dot(lineDir, slotDir) == 0) // Directions are orthogonal to each other
        //     {
        //         var xDiff = slotPos.x - linePos.x;
        //         var yDiff = slotPos.y - linePos.y;
        //         var scaledXDiff = uiManager.UnscaleFloat(xDiff);
        //         var scaledYDiff = uiManager.UnscaleFloat(yDiff);
        //
        //         if (lineDirIsHorizontal)
        //         {
        //             var newLength = lineDir.x > 0 
        //                 ? scaledXDiff + 0.5f * _width + _elementLength 
        //                 : -scaledXDiff + 0.5f * _width + _elementLength;
        //             ResizeLastElement(newLength);
        //
        //             var lastElement = _lineElements.ElementAt(_lineElements.Count - 1);
        //             var line2Length = uiManager.UnscaleFloat(Mathf.Abs(yDiff)) - 0.5f * _width;
        //             var finishElement = yDiff > 0 ? CreateUpElement(lastElement, line2Length) : CreateDownElement(lastElement, line2Length);
        //             _lineElements.Add(finishElement);
        //         }
        //         else
        //         {
        //             var newLength = lineDir.y > 0 
        //                 ? scaledYDiff + 0.5f * _width + _elementLength 
        //                 : -scaledYDiff + 0.5f * _width + _elementLength;
        //             ResizeLastElement(newLength);
        //             
        //             var lastElement = _lineElements.ElementAt(_lineElements.Count - 1);
        //             var line2Length = uiManager.UnscaleFloat(Mathf.Abs(xDiff)) - 0.5f * _width;
        //             var finishElement = xDiff > 0 ? CreateRightElement(lastElement, line2Length) : CreateLeftElement(lastElement, line2Length);
        //             _lineElements.Add(finishElement);
        //         }
        //     }
        //     
        //     SetAllDirty();
        //}
    }
}
