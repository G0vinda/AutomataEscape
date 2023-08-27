using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using Lofelt.NiceVibrations;
using Robot;
using UI.Grid;
using UI.State;
using UI.Transition;
using UnityEngine;

namespace UI
{
    public class UIInputProcess : MonoBehaviour
    {
        [SerializeField] private TransitionSelection transitionSelection;
        [SerializeField] private float transitionLineSelectionTime;
        [SerializeField] private GameObject transitionDeleteButtonPrefab;
        [Header("Blocked Cell Marking")]
        [SerializeField] private Transform blockedCellMarkingLayer;
        [SerializeField] private GameObject blockedCellMarkingPrefab;

        public Action InputStarted;
        public Action InputEnded;

        private InputManager _inputManager;
        private UIManager _uiManager;
        private StateChartPanel _stateChartPanel;
        private UIGridManager _uiGridManager;

        private UIInputPhase _inputPhase;
        
        private bool _dragEnded;
        private Vector2 _previousDragPosition;
        
        // Variables for selecting and deleting TransitionLines
        private SubCell _previousHoveredSubCell;
        private float _transitionLineSelectionTimer;
        private TransitionLine _selectedTransitionLine;
        private GameObject _activeTransitionDeleteButton;

        // Variables for dragging state element
        private StateUIPlaceElement _selectedDragStateElement;
        private StateChartCell _dragStartCell;
        private bool _dragStateElementWasOnGrid;
        private StateChartCell _hoveredDragStateChartCell;
        private List<GameObject> _blockedCellMarkings = new();

        // Variables for drawing transition line
        private StateUIElement _selectedDrawStateElement;
        private StateChartCell _selectedDrawStateCell;
        private Vector2Int _selectedDrawStateCellCoordinates;

        // Variables for zooming
        private float _zoomFactor;
        private const float MINZoomFactor = 1f;
        private const float MAXZoomFactor = 2f;

        #region OnEnable/OnDisable

        private void OnEnable()
        {
            ListenToInputEvents();
            _inputPhase = UIInputPhase.WaitingForInput;
        }

        private void ListenToInputEvents()
        {
            InputManager.StateElementSelected += HandleStatePlaceElementSelected;
            InputManager.StateElementDragStarted += HandleStatePlaceElementDragStart;
            InputManager.StateChartPanelPressed += HandleStateChartPanelDragStart;
            InputManager.TransitionLineDragStarted += HandleTransitionLineDragStart;
            InputManager.TransitionSelectElementSelected += HandleTransitionSelectElementSelected;
            InputManager.ZoomInputChanged += ProcessZoom;
        }
        
        private void OnDisable()
        {
            StopListeningToInputEvents();
        }

        private void StopListeningToInputEvents()
        {
            InputManager.StateElementSelected -= HandleStatePlaceElementSelected;
            InputManager.StateElementDragStarted -= HandleStatePlaceElementDragStart;
            InputManager.StateChartPanelPressed -= HandleStateChartPanelDragStart;
            InputManager.TransitionLineDragStarted -= HandleTransitionLineDragStart;
            InputManager.TransitionSelectElementSelected -= HandleTransitionSelectElementSelected;
            InputManager.ZoomInputChanged -= ProcessZoom;
        }

        #endregion

        public void Initialize(StateChartPanel stateChartPanel)
        {
            _uiManager = GameManager.Instance.GetUIManager();
            _inputManager = GameManager.Instance.GetInputManager();
            _stateChartPanel = stateChartPanel;
            _uiGridManager = _stateChartPanel.GetComponent<UIGridManager>();
            _zoomFactor = 1f;
        }

        private void Update()
        {
            var currentInputPosition = _inputManager.GetPointerPosition();
            switch (_inputPhase)
            {
                case UIInputPhase.WaitingForInput:
                    _dragEnded = false;
                    break;
                case UIInputPhase.DraggingStateElement:
                    if (_dragEnded)
                    {
                        ReleaseStatePlaceElement();
                        ChangeInputPhase(UIInputPhase.WaitingForInput);
                        break;
                    }

                    DragStatePlaceElement();
                    break;
                case UIInputPhase.DraggingStateChart:
                    if (_dragEnded)
                    {
                        ChangeInputPhase(UIInputPhase.WaitingForInput);
                        break;
                    }

                    DragStateChartPanel(currentInputPosition);
                    break;
                case UIInputPhase.InitiateTransitionLineDraw:
                    if (!StartDrawTransitionLine(currentInputPosition) || _dragEnded)
                    {
                        if (_selectedDrawStateElement != null)
                            _selectedDrawStateElement.UpdateScaling();

                        ChangeInputPhase(UIInputPhase.WaitingForInput);
                    }

                    break;
                case UIInputPhase.DrawingTransitionLine:
                    if (!TransitionLineDrawer.DrawOnInput(_inputManager.GetPointerPosition()) || _dragEnded)
                    {
                        ReleaseOnDrawing();
                        ChangeInputPhase(UIInputPhase.WaitingForInput);
                    }

                    break;
                case UIInputPhase.StartSelectingTransitionLine:
                    if(_dragEnded)
                        ChangeInputPhase(UIInputPhase.WaitingForInput);

                    var hoveredSubCell = _uiGridManager.GetSubCellOnPosition(_inputManager.GetPointerPosition());
                    if (hoveredSubCell != _previousHoveredSubCell)
                    {
                        ChangeInputPhase(UIInputPhase.DraggingStateChart);
                        break;
                    }
                    
                    _transitionLineSelectionTimer -= Time.deltaTime;
                    if (_transitionLineSelectionTimer <= 0)
                    {
                        _selectedTransitionLine.Highlight();
                        CreateTransitionDeleteButton();
                            
                        TransitionDeleteButton.ButtonPressed += HandleTransitionDeleteButtonPressed;
                        InputManager.InputOutsideOfDeleteButton += ExitDeleteState;
                        StopListeningToInputEvents();
                        
                        ChangeInputPhase(UIInputPhase.WaitingForDeleteButton);
                    }
                    
                    break;
                case UIInputPhase.WaitingForDeleteButton:
                    // meant to be empty
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeInputPhase(UIInputPhase newPhase)
        {
            if (_inputPhase == UIInputPhase.WaitingForInput && newPhase != UIInputPhase.WaitingForInput)
                InputStarted?.Invoke();

            if (_inputPhase != UIInputPhase.WaitingForInput && newPhase == UIInputPhase.WaitingForInput)
                InputEnded?.Invoke();
            
            _inputPhase = newPhase;
        }

        private void HandleDragEnded()
        {
            InputManager.DragEnded -= HandleDragEnded;
            _uiManager.ResetAllStatesSize();
            _dragEnded = true;
        }

        private void ProcessZoom(float zoomFactorChange, Vector2 zoomCenter)
        {
            if (Mathf.Approximately(_zoomFactor, MINZoomFactor) && zoomFactorChange < 0 ||
                Mathf.Approximately(_zoomFactor, MAXZoomFactor) && zoomFactorChange > 0)
                return;

            var zoomDelta = zoomFactorChange / _zoomFactor;
            _zoomFactor += zoomFactorChange;
            _zoomFactor = Mathf.Clamp(_zoomFactor, MINZoomFactor, MAXZoomFactor);
            _uiManager.ZoomStateChartPanel(_zoomFactor, zoomDelta, zoomCenter);
        }

        private void HandleStatePlaceElementSelected(StateUIElement stateUIElement)
        {
            stateUIElement.SetSizeToSelectedHighlight();
            InputManager.DragEnded += HandleDragEnded;
        }

        private void HandleTransitionLineDragStart(StateUIElement stateElement)
        {
            if (transitionSelection.CurrentSelected != null &&
                (stateElement.GetComponent<StartStateUIElement>() == null ||
                 transitionSelection.CurrentSelected.Condition == StateChartManager.TransitionCondition.Default))
            {
                InputManager.DragEnded += HandleDragEnded;
                ChangeInputPhase(UIInputPhase.InitiateTransitionLineDraw);
                _selectedDrawStateElement = stateElement;
                _selectedDrawStateCell = _selectedDrawStateElement.ConnectedCell;
                _selectedDrawStateCellCoordinates = _uiGridManager.GetCoordinatesFromCell(_selectedDrawStateCell);

                _selectedDragStateElement = null;
            }
        }

        private void HandleStatePlaceElementDragStart(StateUIElement stateElement)
        {
            if (!stateElement.TryGetComponent(out _selectedDragStateElement))
                return;

            InputManager.DragEnded += HandleDragEnded;
            stateElement.SetSizeToDragHighlight();
            _dragStartCell = stateElement.ConnectedCell;
            _uiManager.RemoveStateElementFromGrid(_selectedDragStateElement);

            _dragStateElementWasOnGrid = false;
            _selectedDragStateElement.SwitchAppearanceToOffGrid();
            _selectedDragStateElement.transform.SetAsLastSibling();
            ChangeInputPhase(UIInputPhase.DraggingStateElement);
            SoundPlayer.Instance.PlayStateDragStart();
            HapticPatterns.PlayConstant(0.7f, 0.7f, 0.04f);

            var blockedCellPositions = _uiGridManager.GetCellPositionsAdjacentToStates();
            foreach (var blockedCellPosition in blockedCellPositions)
            {
                var newBlockedCellMarking =
                    Instantiate(blockedCellMarkingPrefab, blockedCellPosition, Quaternion.identity,
                        blockedCellMarkingLayer);
                ((RectTransform)newBlockedCellMarking.transform).sizeDelta =
                    StateUIElement.StateSizeAttributes.StateSize;
                _blockedCellMarkings.Add(newBlockedCellMarking);
            }
        }

        private void HandleStateChartPanelDragStart()
        {
            InputManager.DragEnded += HandleDragEnded;
            var hoveredSubCell = _uiGridManager.GetSubCellOnPosition(_inputManager.GetPointerPosition());
            if (hoveredSubCell != null)
            {
                _previousHoveredSubCell = hoveredSubCell;
                _selectedTransitionLine = CheckForTransitionLine(hoveredSubCell);
                if (_selectedTransitionLine != null)
                {
                    _transitionLineSelectionTimer = transitionLineSelectionTime;
                    ChangeInputPhase(UIInputPhase.StartSelectingTransitionLine);
                    return;
                }
            }
            ChangeInputPhase(UIInputPhase.DraggingStateChart);
            _previousDragPosition = _inputManager.GetPointerPosition();
        }

        private TransitionLine CheckForTransitionLine(SubCell subCell)
        {
            var selectedTransitionLines = _uiGridManager.GetSubCellTransitionLines(subCell).ToList();
            if (selectedTransitionLines.Any())
            {
                if (selectedTransitionLines.Count() > 1)
                {
                    var line0 = selectedTransitionLines.ElementAt(0);
                    var line1 = selectedTransitionLines.ElementAt(1);
                    return line0.transform.GetSiblingIndex() > line1.transform.GetSiblingIndex() ? line0 : line1;
                }

                return selectedTransitionLines.ElementAt(0);
            }

            return null;
        }

        private void HandleTransitionSelectElementSelected(TransitionSelectElement transitionSelectElement)
        {
            SoundPlayer.Instance.PlayCableSelect();
            transitionSelection.SelectTransitionCondition(transitionSelectElement.Condition);
        }

        private void CreateTransitionDeleteButton()
        {
            var buttonOffset = new Vector2(0, 20f);
            var buttonPosition = _inputManager.GetPointerPosition() + buttonOffset;
            _activeTransitionDeleteButton = Instantiate(transitionDeleteButtonPrefab, buttonPosition,
                Quaternion.identity, transform);
        }

        private void HandleTransitionDeleteButtonPressed()
        {
            ExitDeleteState();
            _uiManager.RemoveTransition(_selectedTransitionLine.stateUIElement, _selectedTransitionLine.Condition);
        }

        private void ExitDeleteState()
        {
            _selectedTransitionLine.RemoveHighlight();
            Destroy(_activeTransitionDeleteButton);

            TransitionDeleteButton.ButtonPressed -= HandleTransitionDeleteButtonPressed;
            InputManager.InputOutsideOfDeleteButton -= ExitDeleteState;
            ListenToInputEvents();
            ChangeInputPhase(UIInputPhase.WaitingForInput);
        }

        private bool StartDrawTransitionLine(Vector2 inputPosition)
        {
            if (!_uiGridManager.TryScreenPositionToCellCoordinates(inputPosition, out var currentCellCoordinates))
                return false;

            if (currentCellCoordinates == _selectedDrawStateCellCoordinates)
                return true;

            if (TransitionLineDrawer.StartDrawingIfSubCellIsFree(inputPosition, _selectedDrawStateElement,
                _selectedDrawStateCellCoordinates))
            {
                ChangeInputPhase(UIInputPhase.DrawingTransitionLine);
                return true;
            }

            return true;
        }

        private void ReleaseOnDrawing()
        {
            var currentTransitionLine = TransitionLineDrawer.CurrentTransitionLine;
            if (_selectedDrawStateElement != null)
                _selectedDrawStateElement.UpdateScaling();
            if (TransitionLineDrawer.DestinationStateElement != null)
            {
                var destinationState = TransitionLineDrawer.DestinationStateElement;
                destinationState.GetComponent<StateUIElement>().UpdateScaling();
                TransitionLineDrawer.FinishLine();
                _uiManager.AddTransition(_selectedDrawStateElement, destinationState,
                    transitionSelection.CurrentSelected.Condition);
            }
            else
            {
                TransitionLineDrawer.CancelDraw();
                _selectedDrawStateElement.RemoveTransition(currentTransitionLine);
            }
        }

        private void DragStatePlaceElement()
        {
            Vector3 inputPosition = _inputManager.GetPointerPosition();
            var stateElementTransform = _selectedDragStateElement.transform;

            if (_uiGridManager.TryGetEmptyCellOnPosition(inputPosition, out _hoveredDragStateChartCell,
                out var cellPosition))
            {
                if (!_dragStateElementWasOnGrid)
                {
                    _selectedDragStateElement.SwitchAppearanceToOnGrid();
                }

                stateElementTransform.SetParent(_uiGridManager.transform);
                stateElementTransform.position = cellPosition;
                _dragStateElementWasOnGrid = true;
                return;
            }
            
            if (_dragStateElementWasOnGrid)
            {
                _selectedDragStateElement.SwitchAppearanceToOffGrid();
            }
            
            stateElementTransform.position = inputPosition;
            _dragStateElementWasOnGrid = false;
        }

        private void ReleaseStatePlaceElement()
        {
            if (_dragStateElementWasOnGrid)
            {
                _uiManager.PlaceStateElementOnGrid(_selectedDragStateElement, _hoveredDragStateChartCell);
                SoundPlayer.Instance.PlayStateDragEnd();
                HapticPatterns.PlayConstant(0.7f, 0.7f, 0.04f);
                _selectedDragStateElement = null;
            }
            else
            {
                _uiManager.PlaceStateElementOnGrid(_selectedDragStateElement, _dragStartCell);
                var cellCoordinates = _uiGridManager.GetCoordinatesFromCell(_dragStartCell);
                var cellPosition = _uiGridManager.CellCoordinatesToScreenPosition(cellCoordinates);
                _selectedDragStateElement.transform.position = cellPosition;
                _selectedDragStateElement.SwitchAppearanceToOnGrid();
            }

            foreach (var blockedCellMarking in _blockedCellMarkings)
            {
                Destroy(blockedCellMarking);
            }

            _blockedCellMarkings.Clear();
        }

        private void DragStateChartPanel(Vector2 currentInputPosition)
        {
            var inputDifference = currentInputPosition - _previousDragPosition;
            _stateChartPanel.MoveByVector(inputDifference);
            _previousDragPosition = currentInputPosition;
        }

        private enum UIInputPhase
        {
            WaitingForInput,
            DraggingStateElement,
            DraggingStateChart,
            StartSelectingTransitionLine,
            WaitingForDeleteButton,
            InitiateTransitionLineDraw,
            DrawingTransitionLine,
        }
    }
}