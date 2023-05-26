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
        [SerializeField] private List<StateUIElementStack> stateUIElementStacks;
        [SerializeField] private List<TransitionSelectElement> transitionSelectElements;
        [SerializeField] private StartStateUIElement startStateUIElement;
        [SerializeField] private StateChartPanel stateChartPanel;
        [SerializeField] private Image selectPanel;
        [SerializeField] private GameObject menuButton;

        public static Action<bool> ViewStateChanged;

        private StateChartManager _stateChartManager;
        private UIGridManager _uiGridManager;
        private UIInputProcess _inputProcess;
        private InputManager _inputManager;
        private Canvas _canvas;

        private List<StateUIPlaceElement> _placedStateElements;
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

            if (_inputProcess == null) 
                return;
            
            _inputProcess.InputStarted -= SetMenuButtonToInactive;
            _inputProcess.InputEnded -= SetMenuButtonToActive;
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

            _inputProcess.InputStarted += SetMenuButtonToInactive;
            _inputProcess.InputEnded += SetMenuButtonToActive;
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
                EnableAvailableSelection();
            }
            else
            {
                _availableSelectChanged = true;
            }
        }

        private void SetupStateChartUI()
        {
            var availableHorizontalSpace =
                selectPanel.transform.position.x - ScaleFloat(selectPanel.rectTransform.sizeDelta.x);
            stateChartPanel.Initialize(availableHorizontalSpace);

            var startStateCoordinates = new Vector2Int(0, 3);
            var startCell = _uiGridManager.GetCellOnCoordinates(startStateCoordinates);
            var startCellPosition = _uiGridManager.CellCoordinatesToScreenPosition(startStateCoordinates);
            _uiGridManager.PlaceStateElementOnCell(startStateUIElement.GetComponent<StateUIElement>(), startCell);
            startStateUIElement.Initialize(startCell);
            startStateUIElement.transform.position = startCellPosition;
        }

        private void EnableAvailableSelection()
        {
            var stateChartPanelScaleFactor = stateChartPanel.GetScaleFactor();
            stateUIElementStacks.ForEach(stack => stack.gameObject.SetActive(false));
            foreach (var availableStateInfo in _availableStateInfo)
            {
                if (availableStateInfo.StartPositionOnGrid != -Vector2Int.one)
                {
                    
                    continue;
                }
                var availableStack =
                    stateUIElementStacks.First(stack => stack.GetAction() == availableStateInfo.Action);
                availableStack.gameObject.SetActive(true);
                availableStack.Setup(availableStateInfo.Amount, stateChartPanelScaleFactor);
            }
            
            foreach (var transitionSelectElement in transitionSelectElements)
            {
                transitionSelectElement.gameObject.SetActive(
                    _availableTransitionConditions.Contains(transitionSelectElement.Condition));
            }
        }

        private void ClearStateChartUI()
        {
            _connectedTransitions.Clear();
            _uiGridManager.RemoveStateElementsFromGrid();
            _placedStateElements.Clear();
            startStateUIElement.RemoveDefaultTransitionLine();
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
            selectPanel.gameObject.SetActive(true);
            ViewStateChanged?.Invoke(true);
            _inputManager.enabled = true;
            
            if (!_stateChartPanelInitialized)
            {
                SetupStateChartUI();
                _stateChartPanelInitialized = true;
            }

            if (_availableSelectChanged)
            {
                EnableAvailableSelection();
                _availableSelectChanged = false;
            }
        }

        public void SwitchToLevelView()
        {
            _uiActive = false;
            stateChartPanel.gameObject.SetActive(false);
            selectPanel.gameObject.SetActive(false);
            ViewStateChanged?.Invoke(false);
            _inputManager.enabled = false;
        }

        public void ZoomStateChartPanel(float zoomFactor, float zoomDelta, Vector2 zoomCenter)
        {
            stateChartPanel.ZoomChart(zoomFactor, zoomDelta, zoomCenter);
        }

        private void PlaceNewStateElementOnGrid(StateChartManager.StateAction stateAction, Vector2Int placeCoordinates)
        {
            
        }

        public void PlaceStateElementOnGrid(StateUIPlaceElement placeElement, StateChartCell connectedCell)
        {
            _uiGridManager.PlaceStateElementOnCell(placeElement.GetComponent<StateUIElement>(), connectedCell);
            placeElement.PlaceOnCell(connectedCell, stateChartPanel.transform);
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

        public void PlaceStateElementOnStack(StateUIPlaceElement placeElement)
        {
            stateUIElementStacks.First(s => s.GetAction() == placeElement.GetAction())
                .AddState();
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

        public StateUIElementStack RemoveStateElementFromStack(StateUIPlaceElement placeElement)
        {
            var stack = stateUIElementStacks.First(stack => stack.GetAction() == placeElement.GetAction());
            stack.RemoveState(placeElement);
            return stack;
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
                _connectedTransitions.FirstOrDefault(transition => transition.Key.Item1 == sourceState && transition.Value == condition);
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
        
        public float ScaleFloat(float scaledFloat)
        {
            return scaledFloat * _canvas.scaleFactor;
        }

        public void SetMenuButtonToInactive()
        {
            menuButton.SetActive(false);
        }
        
        public void SetMenuButtonToActive()
        {
            menuButton.SetActive(true);
        }
    }
}