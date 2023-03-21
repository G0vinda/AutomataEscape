using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static StateChartManager;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private List<StateUIElementStack> stateUIElementStacks;
        [SerializeField] private List<TransitionSelectElement> transitionSelectElements;
        [SerializeField] private StartStateUIElement startStateUIElement;
        [SerializeField] private StateChartPanel stateChartPanel;
        [SerializeField] private Image selectPanel;

        private StateChartManager _stateChartManager;
        private StateChartUIGrid _stateChartUIGrid;
        private UIInputProcess _inputProcess;
        private Canvas _canvas;
        
        private List<StateUIPlaceElement> _placedStateElements;
        private List<LevelData.AvailableStateInfo> _availableStateInfo;
        private List<TransitionCondition> _availableTransitionConditions;
        private bool _stateChartUIInitialized;
        private bool _setupUIOnEnable;

        private Dictionary<(StateUIElement, StateUIPlaceElement), TransitionCondition> _connectedTransitions = new();

        private void OnEnable()
        {
            if (!_setupUIOnEnable) return;

            SetupStateChartUI();
            EnableAvailableUIElements();
            _setupUIOnEnable = false;
        }

        public void Initialize()
        {
            _canvas = GetComponent<Canvas>();
            _inputProcess = GetComponent<UIInputProcess>();
            _inputProcess.Initialize(stateChartPanel);
            _stateChartManager = GameManager.Instance.GetStateChartManager();
            _stateChartUIGrid = stateChartPanel.GetComponent<StateChartUIGrid>();
            TransitionLineDrawer.StateChartUIGrid = _stateChartUIGrid;
            _placedStateElements = new List<StateUIPlaceElement>();
        }
        
        public void SetupUI(List<LevelData.AvailableStateInfo> availableStateInfo,
            List<TransitionCondition> availableTransitionConditions)
        {
            ClearStateChartUI();
            _availableStateInfo = availableStateInfo;
            _availableTransitionConditions = availableTransitionConditions;

            if (gameObject.activeSelf)
            {
                SetupStateChartUI();
                EnableAvailableUIElements();
            }
            else
            {
                _setupUIOnEnable = true;
            }
        }
        
        private void SetupStateChartUI()
        {
            if (!_stateChartUIInitialized)
            {
                var availableHorizontalSpace = selectPanel.transform.position.x - selectPanel.rectTransform.sizeDelta.x;
                stateChartPanel.Initialize(availableHorizontalSpace);
                _stateChartUIInitialized = true;
            }

            var startCoordinates = new Vector2Int(0, 3);
            var startCell = _stateChartUIGrid.GetCellOnCoordinates(startCoordinates);
            var startCellPosition = _stateChartUIGrid.CellToScreenCoordinates(startCoordinates);
            startCell.PlaceStateElement(startStateUIElement); // TODO: connect cell to startstate
            startStateUIElement.Initialize(startCell);
            startStateUIElement.transform.position = startCellPosition;
        }

        private void EnableAvailableUIElements()
        {
            Debug.Log("EnableAvailableUIElements is called");
            Debug.Log($"List of available stacks has {_availableStateInfo.Count} elements");
            var stateChartPanelScaleFactor = stateChartPanel.GetScaleFactor();
            foreach (var availableStateInfo in _availableStateInfo)
            {
                var availableStack = stateUIElementStacks.First(stack => stack.GetAction() == availableStateInfo.Action);
                availableStack.gameObject.SetActive(true);
                availableStack.Initialize(availableStateInfo.Amount, stateChartPanelScaleFactor);
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
            _stateChartUIGrid.RemoveCellsFromGrid();
            _placedStateElements.Clear();
            startStateUIElement.RemoveDefaultTransitionLine();
        }

        public void ZoomStateChartPanel(float zoomFactor, float zoomDelta, Vector2 zoomCenter)
        {
            stateChartPanel.ZoomChart(zoomFactor, zoomDelta, zoomCenter);
        }

        public void PlaceStateElementOnGrid(StateUIPlaceElement placeElement, StateChartCell connectedCell)
        {
            connectedCell.PlaceStateElement(placeElement);
            placeElement.PlaceState(connectedCell, stateChartPanel.transform);
                
            var assignedId = _stateChartManager.AddState(placeElement.GetAction());
            placeElement.SetAssignedId(assignedId);
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

            _stateChartManager.RemoveState(placeElement.GetAssignedId());
            _placedStateElements.Remove(placeElement);
            placeElement.SetAssignedId(-1);
        }

        public void RemoveStateElementFromStack(StateUIPlaceElement placeElement)
        {
            stateUIElementStacks.First(stack => stack.GetAction() == placeElement.GetAction()).RemoveState(placeElement);
        }

        public void AddTransition(StateUIElement sourceState, StateUIPlaceElement destinationState,
            TransitionCondition condition)
        {
            _connectedTransitions.Add((sourceState, destinationState), condition);
            if (condition == TransitionCondition.Default)
            {
                _stateChartManager.AddDefaultTransition(sourceState.AssignedId, destinationState.GetAssignedId());
            }
            else
            {
                _stateChartManager.AddTransition(condition, sourceState.AssignedId, destinationState.GetAssignedId());
            }
        }

        public void RemoveTransition(StateUIElement sourceState, TransitionCondition condition)
        {
            var  keyToRemove = _connectedTransitions.First(transition =>
                transition.Key.Item1 == sourceState && transition.Value == condition).Key;
            _connectedTransitions.Remove(keyToRemove);
            
            sourceState.RemoveTransitionByCondition(condition);
            if (condition == TransitionCondition.Default)
            {
                _stateChartManager.RemoveDefaultTransition(sourceState.AssignedId);
            }
            else
            {
                _stateChartManager.RemoveTransition(condition, sourceState.AssignedId);
            }
        }

        public float UnscaleFloat(float downscaledFloat)
        {
            return downscaledFloat / _canvas.scaleFactor;
        }

        public float ScaleFloat(float scaledFloat)
        {
            return scaledFloat * _canvas.scaleFactor;
        }
    }
}