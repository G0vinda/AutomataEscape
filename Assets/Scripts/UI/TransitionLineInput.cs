using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using UnityEngine;

namespace UI
{
    public class TransitionLineInput : MonoBehaviour
    {
        private class DrawInput
        {
            public Vector3 Position;
            public Vector2 Direction;

            public DrawInput(Vector3 position, Vector2 direction)
            {
                Position = position;
                Direction = direction;
            }
        }
        
        [SerializeField] private TransitionLine transitionLine;
        [SerializeField] private float lineElementLength;
        [SerializeField] private float lineElementWidth;
        [SerializeField] private float drawingThreshold;
        [SerializeField] private float maxBlockDistance;
        [SerializeField] private GameObject uiGizmo;

        private bool _isDrawing;
        private List<DrawInput> _inputs = new ();
        private float _scaledLineElementLength;
        private StateUIPlaceElement[] _placedStates;
        private StateUIPlaceElement _stateInRange;
        private int _slotInRangeId = -1;
        private GameObject _currentUiGizmo;
        
        private void OnEnable()
        {
            _scaledLineElementLength = GameManager.Instance.GetStateChartUIManager().ScaleFloat(lineElementLength);
        }

        void Update()
        {
            if (!_isDrawing)
                return;

            if (HelperFunctions.CheckIfMouseIsOverObjectWithTag("TransitionLineBlocker"))
            {
                if (_stateInRange != null)
                {
                    _stateInRange.SetSizeToDefault();
                    _slotInRangeId = -1;
                }

                if (Vector3.Distance(Input.mousePosition, _inputs[^1].Position) > maxBlockDistance)
                {
                    _isDrawing = false;
                    transitionLine.Clear();
                }
                return;
            }

            CheckDragLengthForDrawing();
            if (Input.GetMouseButtonUp(0))
            {
                HandleMouseRelease();
            }

            if (_stateInRange != null)
            {
                var slotWasInRange = _slotInRangeId >= 0;
                _slotInRangeId = _stateInRange.IsPosInRangeOfEmptySlot(Input.mousePosition);
                if (!slotWasInRange && _slotInRangeId >= 0)
                {
                    _stateInRange.SetSizeToBig();
                }
                else if (slotWasInRange && _slotInRangeId < 0)
                {
                    _stateInRange.SetSizeToDefault();
                }
            }
        }

        public bool StartDrawingIfInRange(float distance, Vector2 direction)
        {
            if (_isDrawing || distance < lineElementLength * drawingThreshold)
                return false;
            
            var lineScaleFactor = 1f / transform.parent.localScale.x;
            transitionLine.Initialize(_scaledLineElementLength, lineElementWidth, direction, lineScaleFactor);
            var transitionLinePosition = transitionLine.transform.position;
            //_inputs.Add(new DrawInput(transitionLinePosition, direction));
            _inputs.Add(new DrawInput(transitionLinePosition + (Vector3)direction * lineElementLength, direction));
            _placedStates = FindObjectsOfType<StateUIPlaceElement>();
            CheckIfStateIsInRange();
            _isDrawing = true;
            return true;
        }

        private void CheckDragLengthForDrawing()
        {
            var diffVector = Input.mousePosition - _inputs[^1].Position;
            Debug.Log($"DiffVector: {diffVector.x}, mousePos: {Input.mousePosition.x}, lastInput: {_inputs[^1].Position.x}, lineElementLength: {lineElementLength * drawingThreshold}");
            var diffVectorAbs = new Vector2(Mathf.Abs(diffVector.x), Mathf.Abs(diffVector.y));

            if (diffVectorAbs.x < lineElementLength * drawingThreshold && diffVectorAbs.y < lineElementLength * drawingThreshold)
            {
                return;
            }

            if (diffVectorAbs.x > diffVectorAbs.y)
            {
                var direction = diffVector.x > 0 ? Vector2.right : Vector2.left;
                transitionLine.DrawLineElement(direction);
                CalculateLastInput(direction);
            }
            else
            {
                var direction = diffVector.y > 0 ? Vector2.up : Vector2.down; 
                transitionLine.DrawLineElement(direction);
                CalculateLastInput(direction);
            }

            if (_currentUiGizmo != null)
            {
                Destroy(_currentUiGizmo);
            }

            if (_isDrawing)
            {
                //_currentUiGizmo = Instantiate(uiGizmo, _inputs[^1].Position, Quaternion.identity, transform);
                CheckIfStateIsInRange();
            }
        }

        private void CalculateLastInput(Vector2 currentDirection)
        {
            var lastDirection = _inputs[^1].Direction;
            var lastDirectionIsHorizontal = Mathf.Abs(lastDirection.x) > 0;
            var currentDirectionIsHorizontal = Mathf.Abs(currentDirection.x) > 0;
            
            var downScaledWidth = GameManager.Instance.GetStateChartUIManager().DownscaleFloat(lineElementWidth);
            var halfWidth = downScaledWidth * 0.5f;
            Vector3 currentInputPositionOffset;

            if (currentDirection == -lastDirection)
            {
                _inputs.RemoveAt(_inputs.Count - 1);
                Debug.Log($"lastDirection: {lastDirection} currentDirection: {currentDirection}");
                Debug.Log($"Call to remove line. {_inputs.Count} left");
                if(_inputs.Count == 0)
                    _isDrawing = false;
                return;
            }

            if (lastDirectionIsHorizontal)
            {
                if (currentDirectionIsHorizontal)
                {
                    currentInputPositionOffset = new Vector3(
                        currentDirection.x > 0 ? lineElementLength : -lineElementLength, 
                        0, 
                        0);
                }
                else
                {
                    currentInputPositionOffset = new Vector3(
                        lastDirection.x > 0 ? -halfWidth : halfWidth,
                        currentDirection.y > 0 ? lineElementLength + halfWidth : -(lineElementLength + halfWidth),
                        0);
                }
            }
            else
            {
                if (currentDirectionIsHorizontal)
                {
                    currentInputPositionOffset = new Vector3(
                        currentDirection.x > 0 ? lineElementLength + halfWidth : -(lineElementLength + halfWidth),
                        lastDirection.y > 0 ? -halfWidth : halfWidth,
                        0);
                }
                else
                {
                    currentInputPositionOffset = new Vector3(
                        0,
                        currentDirection.y > 0 ? lineElementLength : -lineElementLength,
                        0);
                }
            }

            var newInputPosition = _inputs[^1].Position + currentInputPositionOffset;
            _inputs.Add(new DrawInput(newInputPosition, currentDirection));
        }

        private void CheckIfStateIsInRange()
        {
            foreach (var state in _placedStates)
            {
                if (state.IsPositionInRangeOfState(_inputs[^1].Position))
                {
                    _stateInRange = state;
                    return;
                }
            }

            _stateInRange = null;
        }

        private void HandleMouseRelease()
        {
            _isDrawing = false;
            
            if (_slotInRangeId >= 0)
            {
                var slotPosition = _stateInRange.GetSlotPosition(_slotInRangeId);
                var slotDirection = _stateInRange.GetSlotDirection(_slotInRangeId);
                GetComponent<TransitionPlug>().OnTransitionConnected(_stateInRange, _slotInRangeId);
                _stateInRange.SetSlotToOccupied(_slotInRangeId);
                _stateInRange.SetSizeToDefault();
                transitionLine.DrawLineToSlot(_inputs[^1].Position, slotPosition, slotDirection);
            }
            else
            {
                _inputs.Clear();
                transitionLine.Clear();   
            }
        }

        public void ClearLine()
        {
            transitionLine.Clear();
        }

    }
}
