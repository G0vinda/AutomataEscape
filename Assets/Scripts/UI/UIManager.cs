using System;
using System.Collections.Generic;
using System.Linq;
using Robot;
using UI.Buttons;
using UI.Grid;
using UI.State;
using UI.Transition;
using UnityEngine;
using UnityEngine.UI;
using TransitionCondition = Robot.StateChartManager.TransitionCondition;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TransitionSelection transitionSelection;
        [SerializeField] private StartStateUIElement startStateUIElement;
        [SerializeField] private StateChartPanel stateChartPanel;
        [SerializeField] private GameObject runButtonObject;
        [SerializeField] private GameObject viewButtonObject;
        [SerializeField] private StateUIElementFactory stateUIElementFactory;
        [SerializeField] private Transform stateLayer;
        [SerializeField] private Transform transitionLayer;
        [SerializeField] private TransitionLine transitionLinePrefab;
        [SerializeField] private Image invalidActionMarker;

        public static event Action<bool> ViewStateChanged;
        public static event Action<(StateUIElement, StateUIPlaceElement)> TransitionCreated;
        public static event Action<(StateUIElement, StateUIPlaceElement)> TransitionRemoved;

        private StateChartManager _stateChartManager;
        private UIGridManager _uiGridManager;
        private UIInputProcess _inputProcess;
        private InputManager _inputManager;
        private Canvas _canvas;

        private List<StateUIPlaceElement> _placedStateElements;
        private Dictionary<StateUIElement, TransitionLine> _connectedTransitionLines;
        private List<LevelData.AvailableStateInfo> _availableStateInfo;
        private List<TransitionCondition> _availableTransitionConditions;
        private bool _stateChartPanelInitialized;
        private bool _availableSelectChanged;
        private bool _uiActive;
        private List<ConnectedTransitionData> _connectedTransitions = new();
        private IButtonResettable _runButton;
        private IButtonResettable _viewButton;

        private void OnEnable()
        {
            StateChartManager.StateIsActive += SetStateImageToActive;
            StateChartManager.StateIsInactive += SetStateImageToInactive;
        }
        
        private void OnDisable()
        {
            StateChartManager.StateIsActive -= SetStateImageToActive;
            StateChartManager.StateIsInactive -= SetStateImageToInactive;
        }

        public void Initialize()
        {
            _canvas = GetComponent<Canvas>();
            _inputProcess = GetComponent<UIInputProcess>();
            _inputProcess.Initialize(stateChartPanel);
            _inputManager = GameManager.Instance.GetInputManager();
            _uiGridManager = stateChartPanel.GetComponent<UIGridManager>();
            TransitionLineDrawer.UIGridManager = _uiGridManager;
            _placedStateElements = new List<StateUIPlaceElement>();
            _runButton = runButtonObject.GetComponent<IButtonResettable>();
            _viewButton = viewButtonObject.GetComponent<IButtonResettable>();
        }

        public void SetupUIForLevel(List<LevelData.AvailableStateInfo> availableStateInfo,
            List<TransitionCondition> availableTransitionConditions,
            StateChartManager stateChartManager)
        {
            ClearStateChartUI();
            _availableStateInfo = availableStateInfo;
            _availableTransitionConditions = availableTransitionConditions;
            _stateChartManager = stateChartManager;
            if (_uiActive)
            {
                SetupStatesAndTransitionSelection();
            }
            else
            {
                _availableSelectChanged = true;
            }
        }

        private void SetupStateChartUI()
        {
            stateChartPanel.Initialize();

            var startStateCoordinates = new Vector2Int(0, 3);
            var startCell = _uiGridManager.GetCellOnCoordinates(startStateCoordinates);
            var startCellPosition = _uiGridManager.CellCoordinatesToScreenPosition(startStateCoordinates);
            _uiGridManager.PlaceStateElementOnCell(startStateUIElement.GetComponent<StateUIElement>(), startCell);
            startStateUIElement.Initialize(startCell);
            startStateUIElement.transform.position = startCellPosition;
        }

        private void SetupStatesAndTransitionSelection()
        {
            foreach (var availableStateInfo in _availableStateInfo)
            {
                PlaceNewStateElementOnGrid(availableStateInfo.Action, availableStateInfo.StartPositionOnGrid);
            }
            
            transitionSelection.Setup(_availableTransitionConditions);
        }

        public void ResetAllStatesSize()
        {
            startStateUIElement.GetComponent<StateUIElement>().UpdateScaling();
            _placedStateElements.ForEach(state => state.GetComponent<StateUIElement>().UpdateScaling());
        }

        private void ClearStateChartUI()
        {
            _connectedTransitions.Clear();
            _uiGridManager.RemoveStateElementsFromGrid();
            _placedStateElements.Clear();
            startStateUIElement.RemoveDefaultTransitionLine();
        }

        public void PlayButtonClick()
        {
            SoundPlayer.Instance.PlayButtonClick();
        }

        public void ToggleStateChartPanel()
        {
            
            if (_uiActive)
            {
                SwitchToLevelView();
            }
            else
            {
                SwitchToProgramView();
            }
        }

        public void SwitchToProgramView()
        {
            _uiActive = true;
            stateChartPanel.gameObject.SetActive(true);
            transitionSelection.TrySetActive(true);
            ViewStateChanged?.Invoke(true);
            _inputManager.enabled = true;
            
            if (!_stateChartPanelInitialized)
            {
                SetupStateChartUI();
                _stateChartPanelInitialized = true;
            }

            if (_availableSelectChanged)
            {
                SetupStatesAndTransitionSelection();
                _availableSelectChanged = false;
            }
        }

        public void SwitchToLevelView()
        {
            _uiActive = false;
            stateChartPanel.gameObject.SetActive(false);
            transitionSelection.TrySetActive(false);
            ViewStateChanged?.Invoke(false);
            _inputManager.enabled = false;
        } 

        public void HideInGameButtons()
        {
            _runButton.Hide();
            _viewButton.Hide();
        }

        public void ResetInGameButtons()
        {
            _runButton.Reset();
            _viewButton.Reset();
        }

        public void ZoomStateChartPanel(float zoomFactor, float zoomDelta, Vector2 zoomCenter)
        {
            stateChartPanel.ZoomChart(zoomFactor, zoomDelta, zoomCenter);
        }

        private void PlaceNewStateElementOnGrid(StateChartManager.StateAction stateAction, Vector2Int placeCoordinates)
        {
            var newState = stateUIElementFactory.CreateStateUIElement(stateAction);
            var stateCell = _uiGridManager.GetCellOnCoordinates(placeCoordinates);
            newState.transform.position = _uiGridManager.CellCoordinatesToScreenPosition(placeCoordinates);
            PlaceStateElementOnGrid(newState, stateCell);
        }

        public void PlaceStateElementOnGrid(StateUIPlaceElement placeElement, StateChartCell connectedCell)
        {
            _uiGridManager.PlaceStateElementOnCell(placeElement.GetComponent<StateUIElement>(), connectedCell);
            placeElement.PlaceOnCell(connectedCell, stateLayer);
            var assignedId = _stateChartManager.AddState(placeElement.GetAction());
            placeElement.SetAssignedId(assignedId);
            _placedStateElements.Add(placeElement);
            
            var transitionLinesToRemove = _uiGridManager.GetCellTransitionLines(connectedCell);
            foreach (var transitionLine in transitionLinesToRemove)
            {
                var sourceState = transitionLine.GetComponentInParent<StateUIElement>();
                RemoveTransition(sourceState, transitionLine.Condition);
            }
        }

        public TransitionLine StartNewTransitionLine(StateUIElement stateUIElement, Vector2 position, Color lineColor,
            Direction direction, TransitionCondition condition)
        {
            var newTransitionLine = Instantiate(transitionLinePrefab, position, Quaternion.identity, transitionLayer);
            newTransitionLine.Initialize(
                stateUIElement,
                StateUIElement.StateSizeAttributes.FirstLineElementLength,
                StateUIElement.StateSizeAttributes.LineElementLength, 
                StateUIElement.StateSizeAttributes.LineWidth, 
                lineColor,
                direction,
                condition);
            stateUIElement.AddTransitionLine(newTransitionLine);

            return newTransitionLine;
        }

        public void RemoveStateElementFromGrid(StateUIPlaceElement placeElement)
        {
            for (var i = 0; i < _connectedTransitions.Count; i++)
            {
                var connectedTransition = _connectedTransitions.ElementAt(i);
                if (connectedTransition.DestinationState != placeElement &&
                    connectedTransition.SourceState != placeElement.GetComponent<StateUIElement>())
                    continue;
                
                RemoveTransition(connectedTransition);
                i--;
            }

            placeElement.SetImageToActive(false);
            _stateChartManager.RemoveStateById(placeElement.GetAssignedId());
            _placedStateElements.Remove(placeElement);
            _uiGridManager.RemoveStateElementFromGrid(placeElement.GetComponent<StateUIElement>());
            placeElement.SetAssignedId(-1);
        }

        public void AddTransition(StateUIElement sourceState, StateUIPlaceElement destinationState,
            TransitionCondition condition)
        {
            var transitionWithSameCondition =
                _connectedTransitions.FirstOrDefault(transition =>
                    transition.SourceState == sourceState && transition.Condition == condition);
            if (transitionWithSameCondition != null)
            {
                RemoveTransition(transitionWithSameCondition);
            }

            TransitionCreated?.Invoke((sourceState, destinationState));
            _connectedTransitions.Add(new ConnectedTransitionData(sourceState, destinationState, condition));
            _stateChartManager.AddTransition(condition, sourceState.AssignedId, destinationState.GetAssignedId());
        }

        public void RemoveTransition(StateUIElement sourceState, TransitionCondition condition)
        {
            var transitionToRemove = _connectedTransitions.First(transition =>
                transition.SourceState == sourceState && transition.Condition == condition);
            RemoveTransition(transitionToRemove);
        }

        private void RemoveTransition(ConnectedTransitionData connectedTransition)
        {
            TransitionRemoved?.Invoke((connectedTransition.SourceState, connectedTransition.DestinationState));
            connectedTransition.SourceState.RemoveTransitionByCondition(connectedTransition.Condition);
            _stateChartManager.RemoveTransition(connectedTransition.Condition, connectedTransition.SourceState.AssignedId);
            TransitionLineDrawer.TransitionLineRemoved(connectedTransition.Condition);
            
            _connectedTransitions.Remove(connectedTransition);
        }

        public void SetStateImageToActive(int stateId)
        {
            _placedStateElements.First(state => state.GetAssignedId() == stateId).SetImageToActive(true);
        }
        
        public void SetStateImageToInactive(int stateId)
        {
            _placedStateElements.First(state => state.GetAssignedId() == stateId).SetImageToActive(false);
        }

        public void SetInvalidActionMarkerPosition(Vector2 position)
        {
            invalidActionMarker.gameObject.SetActive(true);
            invalidActionMarker.transform.position = position;
            invalidActionMarker.transform.SetAsLastSibling();
        }

        public void DisableInvalidActionMarker()
        {
            if (invalidActionMarker.gameObject.activeSelf)
            {
                SoundPlayer.Instance.PlayInvalidActionSound();
                invalidActionMarker.gameObject.SetActive(false);   
            }
        }
        
        public float ScaleFloat(float scaledFloat)
        {
            return scaledFloat * _canvas.scaleFactor;
        }

        public float UnscaleFloat(float unscaledFloat)
        {
            return unscaledFloat / _canvas.scaleFactor;
        }
        
        private class ConnectedTransitionData
        {
            public StateUIElement SourceState;
            public StateUIPlaceElement DestinationState;
            public TransitionCondition Condition;

            public ConnectedTransitionData(StateUIElement source, StateUIPlaceElement destination,
                TransitionCondition condition)
            {
                SourceState = source;
                DestinationState = destination;
                Condition = condition;
            }
        }
    }
}