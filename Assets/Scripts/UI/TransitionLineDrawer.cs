using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace UI
{
    public class TransitionLineDrawer : MonoBehaviour
    {
        [SerializeField] private GameObject lineElementPrefab;

        public int connectedSlotId;

        private const float DrawDistanceThresholdFactor = 0.7f;
        private float _lineElementLength;
        private float _lineWidth;

        private bool _isDrawing;
        private Vector3 _previousInputPosition;
        private Vector3 _currentInputPosition;
        private Vector2 _previousDrawDirection;
        private Vector2 _currentDrawDirection;
        private StateUIPlaceElement[] _placedStates;
        private StateUIPlaceElement _stateInRange;
        private int _slotInRangeId = -1;
        private readonly List<GameObject> _inputLine = new();

        public static bool IsDistanceBigEnoughToDraw(float distance)
        {
            return distance > DrawDistanceThresholdFactor * 50f;
        }

        public void Initialize(Vector3 startPos, Vector2 dir)
        {
            _placedStates = FindObjectsOfType<StateUIPlaceElement>();

            _isDrawing = true;
            _previousInputPosition = startPos;
            _previousDrawDirection = dir;
            SpawnLineElement(dir);
            _previousInputPosition += (Vector3)dir * _lineElementLength;
        }

        private void Start()
        {
            var lineElementSize = lineElementPrefab.GetComponent<RectTransform>().sizeDelta;
            _lineElementLength = lineElementSize.y;
            _lineWidth = lineElementSize.x;
        }

        void Update()
        {
            if (Input.GetMouseButtonUp(0))
                HandleMouseButtonUp();

            _currentInputPosition = Input.mousePosition;
            _currentDrawDirection = CheckDragDistance(_previousInputPosition, _currentInputPosition);
            _isDrawing = !HelperFunctions.CheckIfMouseIsOverObjectWithTag("TransitionLineBlocker");

            if (_isDrawing)
            {
                if (!_currentDrawDirection.Equals(Vector2.zero))
                {
                    ProcessNewDrawStep();
                }

                if (_stateInRange == null)
                    return;

                var slotWasInRange = _slotInRangeId >= 0;
                _slotInRangeId = _stateInRange.IsPosInRangeOfEmptySlot(_currentInputPosition);
                if (!slotWasInRange && _slotInRangeId >= 0)
                {
                    _stateInRange.SetSizeToBig();
                }
                else if (slotWasInRange && _slotInRangeId < 0)
                {
                    _stateInRange.SetSizeToDefault();
                }
            }
            else
            {
                if (_stateInRange != null)
                {
                    _stateInRange.SetSizeToDefault();
                    _slotInRangeId = -1;
                }

                // TODO: Improve this
                if (Vector3.Distance(_currentInputPosition, _previousInputPosition) > _lineElementLength * 2)
                {
                    GetComponentInParent<TransitionPlug>().DisconnectLine();
                }
            }
        }

        private void HandleMouseButtonUp()
        {
            if (_slotInRangeId >= 0)
            {
                ConnectToState();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ConnectToState()
        {
            DrawLineToSlot(_stateInRange.GetSlotPosition(_slotInRangeId), _stateInRange.GetSlotDirection(_slotInRangeId));
            _stateInRange.SetSlotToOccupied(_slotInRangeId);
            connectedSlotId = _slotInRangeId;
            _stateInRange.SetSizeToDefault();
            enabled = false;
        }

        private void ProcessNewDrawStep()
        {
            if (_currentDrawDirection.Equals(_previousDrawDirection))
            {
                ExtendCurrentLineElement();
                _previousDrawDirection = _currentDrawDirection;
            }
            else if (_currentDrawDirection.Equals(-_previousDrawDirection))
            {
                _previousDrawDirection = TruncateCurrentLineElement();
            }
            else
            {
                SpawnLineElement(_currentDrawDirection);
                _previousDrawDirection = _currentDrawDirection;
            }

            _previousInputPosition += (Vector3)_currentDrawDirection * _lineElementLength;
            var prevStateInRange = _stateInRange;
            _stateInRange = null;

            foreach (var state in _placedStates)
            {
                if (state.IsPositionInRangeOfState(_previousInputPosition))
                {
                    _stateInRange = state;
                    break;
                }
            }

            if (prevStateInRange != null && prevStateInRange != _stateInRange)
            {
                prevStateInRange.SetSizeToDefault();
            }
        }

        private void ExtendCurrentLineElement()
        {
            var currentLineElementTransform = _inputLine.ElementAt(_inputLine.Count - 1).GetComponent<RectTransform>();
            var newElementSize = currentLineElementTransform.sizeDelta;
            newElementSize.y += _lineElementLength;
            currentLineElementTransform.sizeDelta = newElementSize;
        }

        private Vector2 TruncateCurrentLineElement()
        {
            var currentLineElementTransform = _inputLine.ElementAt(_inputLine.Count - 1).GetComponent<RectTransform>();
            if (currentLineElementTransform.sizeDelta.y >= _lineElementLength * 2)
            {
                var newElementSize = currentLineElementTransform.sizeDelta;
                newElementSize.y -= _lineElementLength;
                currentLineElementTransform.sizeDelta = newElementSize;
            }
            else
            {
                if (_inputLine.Count == 1)
                    GetComponentInParent<TransitionPlug>().DisconnectLine();
                _inputLine.Remove(currentLineElementTransform.gameObject);
                Destroy(currentLineElementTransform.gameObject);
            }

            return _inputLine.ElementAt(_inputLine.Count - 1).GetComponent<RectTransform>().ZRotToDir();
        }

        private void SpawnLineElement(Vector2 drawDir)
        {
            var newLineElementPosition = _previousInputPosition;
            var newLineElementRotation = Quaternion.LookRotation(Vector3.forward, new Vector3(drawDir.x, drawDir.y, 0));

            if (_inputLine.Any())
            {
                var previousLineElementTransform = GetCurrentLineElement().GetComponent<RectTransform>();
                newLineElementPosition = CalculateNewLineElementPos(drawDir, previousLineElementTransform);
                
                Debug.Log($"Last Line element length is: {GetCurrentLineElement().GetComponent<RectTransform>().sizeDelta.y}");
                
            }

            var newLineElement =
                Instantiate(lineElementPrefab, newLineElementPosition, newLineElementRotation, transform);
            
            Debug.Log($"Spawned line element with length: {newLineElement.GetComponent<RectTransform>().sizeDelta.y}");
            _inputLine.Add(newLineElement);
        }

        private bool DrawLineToSlot(Vector3 slotPos, Vector2 slotDir)
        {
            var currentElementTransform = GetCurrentLineElement().GetComponent<RectTransform>();
            var lineDir = currentElementTransform.ZRotToDir();
            var lineDirIsHorizontal = Mathf.Abs(lineDir.x) > 0;
            var linePos = currentElementTransform.position + (Vector3)lineDir * currentElementTransform.sizeDelta.y;

            if (lineDir - slotDir == Vector2.zero || lineDir + slotDir == Vector2.zero) // Directions are opposite
            {
                if (lineDirIsHorizontal)
                {
                    var line1Length = Mathf.Abs(slotPos.y - linePos.y);
                    var line1Dir = slotPos.y > linePos.y ? Vector2.up : Vector2.down;
                    var line1Transform = DrawSnapLine(line1Length, line1Dir, currentElementTransform);

                    var line2Length = Mathf.Abs(slotPos.x - linePos.x);
                    var line2Dir = slotPos.x > linePos.x ? Vector2.right : Vector2.left;
                    DrawSnapLine(line2Length, line2Dir, line1Transform);
                }
                else
                {
                    var line1Length = Mathf.Abs(slotPos.x - linePos.x);
                    var line1Dir = slotPos.x > linePos.x ? Vector2.right : Vector2.left;
                    var line1Transform = DrawSnapLine(line1Length, line1Dir, currentElementTransform);

                    var line2Length = Mathf.Abs(slotPos.y - linePos.y);
                    var line2Dir = slotPos.y > linePos.y ? Vector2.up : Vector2.down;
                    DrawSnapLine(line2Length, line2Dir, line1Transform);
                }
            }
            else if (Vector2.Dot(lineDir, slotDir) == 0) // Directions are orthogonal to each other
            {
                var xDiff = slotPos.x - linePos.x;
                var yDiff = slotPos.y - linePos.y;

                if (lineDirIsHorizontal)
                {
                    int extensionFactor = lineDir.x > 0 ? 1 : -1;
                    SetLineElementLength(currentElementTransform, extensionFactor * xDiff + 0.5f * _lineWidth, true);

                    var line2Length = Mathf.Abs(yDiff) - 0.5f * _lineWidth;
                    var line2Dir = yDiff > 0 ? Vector2.up : Vector2.down;
                    DrawSnapLine(line2Length, line2Dir, currentElementTransform);
                }
                else
                {
                    int extensionFactor = lineDir.y > 0 ? 1 : -1;
                    SetLineElementLength(currentElementTransform, extensionFactor * yDiff + 0.5f * _lineWidth, true);

                    var line2Length = Mathf.Abs(xDiff) - 0.5f * _lineWidth;
                    var line2Dir = xDiff > 0 ? Vector2.right : Vector2.left;
                    DrawSnapLine(line2Length, line2Dir, currentElementTransform);
                }
            }

            return true; // TODO: replace by return values that make sense
        }

        private RectTransform DrawSnapLine(float lineLength, Vector2 lineDir, RectTransform prevElement)
        {
            var lineRot = Quaternion.LookRotation(Vector3.forward, new Vector3(lineDir.x, lineDir.y, 0));
            var linePos = CalculateNewLineElementPos(lineDir, prevElement);
            var lineTransform = Instantiate(lineElementPrefab, linePos, lineRot, transform)
                .GetComponent<RectTransform>();
            SetLineElementLength(lineTransform, lineLength);
            return lineTransform;
        }

        private static void SetLineElementLength(RectTransform transform, float length, bool extend = false)
        {
            var lineSize = transform.sizeDelta;
            length = extend ? lineSize.y + length : length;
            lineSize.y = length;
            transform.sizeDelta = lineSize;
        }

        private GameObject GetCurrentLineElement()
        {
            return _inputLine.ElementAt(_inputLine.Count - 1);
        }

        private Vector2 CheckDragDistance(Vector3 previousPos, Vector3 nextPos)
        {
            var xDiff = nextPos.x - previousPos.x;
            var yDiff = nextPos.y - previousPos.y;

            if (Mathf.Abs(xDiff) > _lineElementLength * DrawDistanceThresholdFactor)
            {
                // Draw horizontal
                return xDiff > 0 ? Vector2.right : Vector2.left;
            }

            if (Mathf.Abs(yDiff) > _lineElementLength * DrawDistanceThresholdFactor)
            {
                // Draw vertical
                return yDiff > 0 ? Vector2.up : Vector2.down;
            }

            return Vector2.zero;
        }

        private Vector3 CalculateNewLineElementPos(Vector2 drawDir, RectTransform previousLineElementTransform)
        {
            var previousLineElementDirection = previousLineElementTransform.ZRotToDir();
            var previousLineElementSizeOffset = previousLineElementDirection * previousLineElementTransform.sizeDelta.y;

            var vecToNewLineElementPosition = new Vector3((drawDir.x - previousLineElementDirection.x) * _lineWidth / 2,
                (drawDir.y - previousLineElementDirection.y) * _lineWidth / 2, 0);

            return previousLineElementTransform.position + (Vector3)previousLineElementSizeOffset +
                   vecToNewLineElementPosition;
        }

        public void DestroyTransitionLineDrawer()
        {
            Destroy(gameObject);
        }
    }
}