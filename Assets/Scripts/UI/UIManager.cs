using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using Robot;
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
        [SerializeField] private GameObject runButton;
        [SerializeField] private GameObject viewButton;
        [SerializeField] private StateUIElementFactory stateUIElementFactory;
        [SerializeField] private Transform stateLayer;
        [SerializeField] private Transform transitionLayer;
        [SerializeField] private TransitionLine transitionLinePrefab;
        [SerializeField] private Image invalidActionMarker;

        public static event Action<bool> ViewStateChanged;

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
        private Dictionary<(StateUIElement, StateUIPlaceElement), TransitionCondition> _connectedTransitions = new();

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

        public void ToggleUI()
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

        public void SetButtonsActive(bool active)
        {
            runButton.SetActive(active);
            viewButton.SetActive(active);
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
                if (connectedTransition.Key.Item2 != placeElement &&
                    connectedTransition.Key.Item1 != placeElement.GetComponent<StateUIElement>())
                    continue;

                var condition = connectedTransition.Value;
                RemoveTransition(connectedTransition.Key.Item1, condition);
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
            var newKey = (sourceState, destinationState);
            if (_connectedTransitions.ContainsKey(newKey))
            {
                RemoveTransition(sourceState, _connectedTransitions[newKey]);
            }

            var transitionWithSameCondition =
                _connectedTransitions.FirstOrDefault(transition =>
                    transition.Key.Item1 == sourceState && transition.Value == condition);
            if (!transitionWithSameCondition.IsDefault())
            {
                RemoveTransition(sourceState, condition);
            }

            _connectedTransitions.Add(newKey, condition);
            _stateChartManager.AddTransition(condition, sourceState.AssignedId, destinationState.GetAssignedId());
        }

        public void RemoveTransition(StateUIElement sourceState, TransitionCondition condition)
        {
            var keyToRemove = _connectedTransitions.First(transition =>
                transition.Key.Item1 == sourceState && transition.Value == condition).Key;
            _connectedTransitions.Remove(keyToRemove);

            sourceState.RemoveTransitionByCondition(condition);
            _stateChartManager.RemoveTransition(condition, sourceState.AssignedId);
            TransitionLineDrawer.TransitionLineRemoved(condition);
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
            invalidActionMarker.gameObject.SetActive(false);
        }
        
        public float ScaleFloat(float scaledFloat)
        {
            return scaledFloat * _canvas.scaleFactor;
        }
    }
}