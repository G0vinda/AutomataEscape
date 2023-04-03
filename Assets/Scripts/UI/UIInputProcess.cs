using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using Robot;
using UI.Grid;
using UI.State;
using UI.Transition;
using UnityEngine;

namespace UI
{
    public class UIInputProcess : MonoBehaviour
    {
        [SerializeField] private GameObject blockedCellMarkingPrefab;
            
        private InputManager _inputManager;
        private UIManager _uiManager;
        private StateChartPanel _stateChartPanel;
        private UIGridManager _uiGridManager;
        
        private UIInputPhase _inputPhase;
        
        private bool _dragEnded;
        private Vector2 _previousDragPosition;

        // Variables for dragging state element
        private StateUIPlaceElement _selectedDragStateElement;
        private StateUIElementStack _selectedDragStateStack;
        private bool _dragStateElementWasOnGrid;
        private StateChartCell _hoveredDragStateChartCell;
        private List<GameObject> _blockedCellMarkings = new ();

        // Variables for drawing transition line
        private TransitionSelectElement _selectedTransitionElement;
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
            InputManager.StateElementTapped += HandleStatePlaceElementTapped;
            InputManager.StateChartPanelTapped += HandleStateChartPanelTapped;
            InputManager.StateElementDragStarted += HandleStatePlaceElementDragStart;
            InputManager.StateChartPanelDragStarted += HandleStateChartPanelDragStart;
            InputManager.TransitionElementSelected += HandleTransitionSelected;
            InputManager.TransitionDeselected += HandleTransitionDeselected;
            InputManager.ZoomInputChanged += ProcessZoom;

            _inputPhase = UIInputPhase.WaitingForInput;
        }

        private void OnDisable()
        {
            InputManager.StateElementTapped -= HandleStatePlaceElementTapped;
            InputManager.StateChartPanelTapped -= HandleStateChartPanelTapped;
            InputManager.StateElementDragStarted -= HandleStatePlaceElementDragStart;
            InputManager.StateChartPanelDragStarted -= HandleStateChartPanelDragStart;
            InputManager.TransitionElementSelected -= HandleTransitionSelected;
            InputManager.TransitionDeselected -= HandleTransitionDeselected;
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
                        _inputPhase = UIInputPhase.WaitingForInput;
                        break;
                    }
                    DragStatePlaceElement();
                    break;
                case UIInputPhase.DraggingStateChart:
                    if (_dragEnded)
                    {
                        _inputPhase = UIInputPhase.WaitingForInput;
                        break;
                    }
                    DragStateChartPanel(currentInputPosition);
                    break;
                case UIInputPhase.InitiateTransitionLineDraw:
                    if (!StartDrawTransitionLine(currentInputPosition) || _dragEnded)
                    {
                        _inputPhase = UIInputPhase.WaitingForInput;
                    }
                    break;
                case UIInputPhase.DrawingTransitionLine:
                    if (!TransitionLineDrawer.DrawOnInput(_inputManager.GetPointerPosition()) || _dragEnded)
                    {
                        ReleaseOnDrawing();
                        _inputPhase = UIInputPhase.WaitingForInput;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleDragEnded()
        {
            InputManager.DragEnded -= HandleDragEnded;
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

        private void HandleStatePlaceElementTapped(StateUIElement stateElement)
        {
            // Todo: highlight connected transitions
        }

        private void HandleStateChartPanelTapped()
        {
            // Todo: reset transition highlighting
        }

        private void HandleStatePlaceElementDragStart(StateUIElement stateElement)
        {
            InputManager.DragEnded += HandleDragEnded;
            if (stateElement.ConnectedCell != null)
            {
                if (_selectedTransitionElement != null && (stateElement.GetComponent<StartStateUIElement>() == null ||
                    _selectedTransitionElement.Condition == StateChartManager.TransitionCondition.Default))
                {
                    _inputPhase = UIInputPhase.InitiateTransitionLineDraw;
                    _selectedDrawStateElement = stateElement;
                    _selectedDrawStateElement.SetSizeToHighlight();
                    _selectedDrawStateCell = _selectedDrawStateElement.ConnectedCell;
                    _selectedDrawStateCellCoordinates = _uiGridManager.GetCoordinatesFromCell(_selectedDrawStateCell);

                    _selectedDragStateElement = null;
                    return;
                }

                if (!stateElement.TryGetComponent(out _selectedDragStateElement))
                {
                    InputManager.DragEnded -= HandleDragEnded;
                    return;
                }

                _uiManager.RemoveStateElementFromGrid(_selectedDragStateElement);
                stateElement.ConnectedCell.RemoveStateElement();
            }
            else
            {
                HandleTransitionDeselected(); 
                _selectedDragStateElement = stateElement.GetComponent<StateUIPlaceElement>();
                if (!_selectedDragStateElement.Draggable)
                {
                    InputManager.DragEnded -= HandleDragEnded;
                    return;
                }
                _selectedDragStateStack = _uiManager.RemoveStateElementFromStack(_selectedDragStateElement);
            }

            _dragStateElementWasOnGrid = false;
            _selectedDragStateElement.SwitchAppearanceToOffGrid();
            _selectedDragStateElement.transform.SetAsLastSibling();
            _inputPhase = UIInputPhase.DraggingStateElement;

            var blockedCellPositions = _uiGridManager.GetCellPositionsAdjacentToStates();
            foreach (var blockedCellPosition in blockedCellPositions)
            {
                var newBlockedCellMarking =
                    Instantiate(blockedCellMarkingPrefab, blockedCellPosition, Quaternion.identity, transform);
                ((RectTransform)newBlockedCellMarking.transform).sizeDelta =
                    StateUIElement.StateSizeAttributes.StateSize;
                _blockedCellMarkings.Add(newBlockedCellMarking);
            }
        }

        private void HandleStateChartPanelDragStart()
        {
            InputManager.DragEnded += HandleDragEnded;
            _inputPhase = UIInputPhase.DraggingStateChart;
            _previousDragPosition = _inputManager.GetPointerPosition();
        }

        private void HandleTransitionSelected(TransitionSelectElement transitionSelectElement)
        {
            if(_selectedTransitionElement != null)
                _selectedTransitionElement.HideSelectionMarking();
        
            _selectedTransitionElement = transitionSelectElement;
            _selectedTransitionElement.ShowSelectionMarking();
            TransitionLineDrawer.CurrentTransitionCondition = transitionSelectElement.GetCondition();
        }
    
        private void HandleTransitionDeselected()
        {
            if (_selectedTransitionElement == null) 
                return;
        
            _selectedTransitionElement.HideSelectionMarking();
            _selectedTransitionElement = null;
        }

        private bool StartDrawTransitionLine(Vector2 inputPosition)
        {
            if (!_uiGridManager.TryScreenPositionToCellCoordinates(inputPosition, out var currentCellCoordinates))
                return false;

            if (currentCellCoordinates == _selectedDrawStateCellCoordinates)
                return true;

            if (!_uiGridManager.CheckIfSubCellIsAdjacentToCell(_selectedDrawStateCell, inputPosition))
            {
                return false;
            }
            
            var drawDirection = (currentCellCoordinates - _selectedDrawStateCellCoordinates).ToDirection();
            if (TransitionLineDrawer.StartDrawingIfSubCellIsFree(inputPosition, drawDirection, _selectedDrawStateElement))
            {
                _inputPhase = UIInputPhase.DrawingTransitionLine;
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
                _uiManager.AddTransition(_selectedDrawStateElement, destinationState, _selectedTransitionElement.GetCondition());
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
            var raycastResults = HelperFunctions.GetRaycastResultsOnPosition(inputPosition);
            
            if (raycastResults.All(x => x.gameObject.name != "SelectPanel"))
            {
                if (_uiGridManager.TryGetEmptyCellOnPosition(inputPosition, out _hoveredDragStateChartCell, out var cellPosition))
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
            }
            
            
            if (_dragStateElementWasOnGrid)
            {
                _selectedDragStateElement.SwitchAppearanceToOffGrid();
            }

            stateElementTransform.SetParent(_selectedDragStateStack.transform);
            stateElementTransform.position = inputPosition;
            _dragStateElementWasOnGrid = false;
        }

        private void ReleaseStatePlaceElement()
        {
            if (_dragStateElementWasOnGrid)
            {
                _uiManager.PlaceStateElementOnGrid(_selectedDragStateElement, _hoveredDragStateChartCell);
                _selectedDragStateElement = null;
            }
            else
            {
                _uiManager.PlaceStateElementOnStack(_selectedDragStateElement);
                Destroy(_selectedDragStateElement.gameObject);
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
            InitiateTransitionLineDraw,
            DrawingTransitionLine,
        }
    }
}