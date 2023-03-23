using System;
using Helper;
using UI;
using UnityEngine;
using static StateChartManager;

public class UIInputProcess : MonoBehaviour
{
    private InputManager _inputManager;
    private UIManager _uiManager;
    private StateChartPanel _stateChartPanel;
    private StateChartUIGrid _stateChartUIGrid;

    private float _zoomFactor;
    private bool _dragEnded;

    private UIInputState _inputState;

    // Variables for dragging state element
    private StateUIPlaceElement _selectedDragStateElement;
    private bool _dragStateElementWasOnGrid;
    private StateChartCell _hoveredDragStateChartCell;


    // Variables for drawing transition line
    private TransitionCondition? _selectedTransitionCondition;
    private StateUIElement _selectedDrawStateElement;
    private StateChartCell _selectedDrawStateCell;
    private Vector2Int _selectedDrawStateCellCoordinates;

    private Vector2 _previousDragPosition;

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

        _inputState = UIInputState.WaitingForInput;
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
        _stateChartUIGrid = _stateChartPanel.GetComponent<StateChartUIGrid>();
        _zoomFactor = 1f;
    }

    private void Update()
    {
        var currentInputPosition = _inputManager.GetPointerPosition();
        switch (_inputState)
        {
            case UIInputState.WaitingForInput:
                _dragEnded = false;
                break;
            case UIInputState.DraggingStateElement:
                if (_dragEnded)
                {
                    ReleaseStatePlaceElement();
                    _inputState = UIInputState.WaitingForInput;
                    break;
                }
                DragStatePlaceElement();
                break;
            case UIInputState.DraggingStateChart:
                if (_dragEnded)
                {
                    _inputState = UIInputState.WaitingForInput;
                    break;
                }
                DragStateChartPanel(currentInputPosition);
                break;
            case UIInputState.InitiateTransitionLineDraw:
                if (!StartDrawTransitionLine(currentInputPosition) || _dragEnded)
                {
                    _inputState = UIInputState.WaitingForInput;
                }
                break;
            case UIInputState.DrawingTransitionLine:
                if (!TransitionLineDrawer.DrawOnInput(_inputManager.GetPointerPosition()) || _dragEnded)
                {
                    ReleaseOnDrawing();
                    _inputState = UIInputState.WaitingForInput;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleDragEnded()
    {
        InputManager.DragEnded -= HandleDragEnded;
        Debug.Log("Input was released.");
        _dragEnded = true;
    }
    
    private void ProcessZoom(float zoomFactorChange, Vector2 zoomCenter)
    {
        const float minZoomFactor = 1f;
        const float maxZoomFactor = 2f;
        Debug.Log($"ProcessZoom got called with factorChange of: {zoomFactorChange}");
        if (Mathf.Approximately(_zoomFactor, minZoomFactor) && zoomFactorChange < 0 ||
            Mathf.Approximately(_zoomFactor, maxZoomFactor) && zoomFactorChange > 0)
            return;

        var zoomDelta = zoomFactorChange / _zoomFactor;
        _zoomFactor += zoomFactorChange;
        _zoomFactor = Mathf.Clamp(_zoomFactor, minZoomFactor, maxZoomFactor);
        Debug.Log($"Zoom Factor is {_zoomFactor}");
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
            if (_selectedTransitionCondition != null)
            {
                _inputState = UIInputState.InitiateTransitionLineDraw;
                _selectedDrawStateElement = stateElement;
                _selectedDrawStateCell = _selectedDrawStateElement.ConnectedCell;
                _selectedDrawStateCellCoordinates = _stateChartUIGrid.GetCoordinatesFromCell(_selectedDrawStateCell);

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
            _selectedDragStateElement = stateElement.GetComponent<StateUIPlaceElement>();
            _selectedTransitionCondition = null; // TODO: unhighlight transitionSelectElement 
            _uiManager.RemoveStateElementFromStack(_selectedDragStateElement);
        }

        _dragStateElementWasOnGrid = false;
        _selectedDragStateElement.SwitchAppearanceToOffGrid();
        _selectedDragStateElement.transform.SetAsLastSibling();
        _inputState = UIInputState.DraggingStateElement;
    }

    private void HandleStateChartPanelDragStart()
    {
        InputManager.DragEnded += HandleDragEnded;
        _inputState = UIInputState.DraggingStateChart;
        _previousDragPosition = _inputManager.GetPointerPosition();
    }

    private void HandleTransitionSelected(TransitionCondition condition)
    {
        // TODO: Highlight new transitionElement, Unhighlight old transitionElement
        Debug.Log($"New TransitionCondition is now {condition}");
        _selectedTransitionCondition = condition;
        TransitionLineDrawer.CurrentTransitionCondition = condition;
    }
    
    private void HandleTransitionDeselected()
    {
        // TODO: Unhighlight transitionElement
        _selectedTransitionCondition = null;
    }

    private bool StartDrawTransitionLine(Vector2 inputPosition)
    {
        // Todo: Highlight State
        Vector2Int currentCellCoordinates;

        if (_stateChartUIGrid.IsPositionInsideGrid(inputPosition))
        {
            currentCellCoordinates = _stateChartUIGrid.ScreenToCellCoordinates(inputPosition);
        }
        else
        {
            return false;
        }

        if (currentCellCoordinates == _selectedDrawStateCellCoordinates)
            return true;

        var drawDirection = (currentCellCoordinates - _selectedDrawStateCellCoordinates).ToDirection();

        if (!_stateChartUIGrid.CheckIfSubCellIsAdjacentToCell(_selectedDrawStateCell, inputPosition))
        {
            Debug.Log("New Cell was not adjacent. Draw will be stopped.");
            return false;
        }

        if (TransitionLineDrawer.StartDrawingIfSubCellIsFree(inputPosition, drawDirection, _selectedDrawStateElement))
        {
            //StartCoroutine(DrawTransitionLine());
            _inputState = UIInputState.DrawingTransitionLine;
            return true;
        }

        return true;
    }

    private void DragStatePlaceElement()
    {
        Vector3 inputPosition = _inputManager.GetPointerPosition();
        var stateElementTransform = _selectedDragStateElement.transform;
        _hoveredDragStateChartCell = _stateChartUIGrid.TryGetEmptyCellOnPosition(inputPosition, out var cellPosition);
        if (_hoveredDragStateChartCell != null)
        {
            if (!_dragStateElementWasOnGrid)
            {
                _selectedDragStateElement.SwitchAppearanceToOnGrid();
            }

            stateElementTransform.position = cellPosition;
            _dragStateElementWasOnGrid = true;
        }
        else
        {
            if (_dragStateElementWasOnGrid)
            {
                _selectedDragStateElement.SwitchAppearanceToOffGrid();
            }

            stateElementTransform.position = inputPosition;
            _dragStateElementWasOnGrid = false;
        }
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
    }
    
    private void ReleaseOnDrawing()
    {
        var currentTransitionLine = TransitionLineDrawer.CurrentTransitionLine;
        if (TransitionLineDrawer.DestinationStateElement != null)
        {
            var destinationState = TransitionLineDrawer.DestinationStateElement;
            TransitionLineDrawer.FinishLine();
            _uiManager.AddTransition(_selectedDrawStateElement, destinationState, _selectedTransitionCondition.Value);
            Debug.Log("Transition would be created!");   
        }
        else
        {
            _selectedDrawStateElement.RemoveTransitionByCondition(currentTransitionLine.Condition);
        }
    }

    private void DragStateChartPanel(Vector2 currentInputPosition)
    {
        var inputDifference = currentInputPosition - _previousDragPosition;
        _stateChartPanel.MoveByVector(inputDifference);
        _previousDragPosition = currentInputPosition;
    }


    private enum UIInputState
    {
        WaitingForInput,
        DraggingStateElement,
        DraggingStateChart,
        InitiateTransitionLineDraw,
        DrawingTransitionLine,
    }
}