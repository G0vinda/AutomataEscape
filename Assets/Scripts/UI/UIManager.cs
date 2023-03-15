using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Helper;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private List<StateUIElementStack> stateUIElementStacks;
        [SerializeField] private List<TransitionSelectElement> transitionSelectElements;
        [SerializeField] private TransitionPlaceElement transitionPlaceElementPrefab;
        [SerializeField] private StartStateUIElement startStateUIElement;
        [SerializeField] private StateChartPanel stateChartPanel;

        private StateChartManager _stateChartManager;
        private StateChartUIGrid _stateChartUIGrid;
        private bool _setupUIOnEnable;
        private StateUIPlaceElement _statePlaceElement;
        private List<StateUIPlaceElement> _placedStateElements;
        private TransitionPlaceElement _transitionPlaceElement;
        private List<LevelData.AvailableStateInfo> _availableStateInfo;
        private List<StateChartManager.TransitionCondition> _availableTransitionConditions;
        private InputManager _inputManager;
        private float _zoomFactor;
        private bool _dragEnded;
        private bool _stateChartUIInitialized;

        private Dictionary<(StateUIElement, StateUIPlaceElement), TransitionPlug> _connectedTransitions = new();
        private Canvas _canvas;

        private void OnEnable()
        {
            InputManager.StateElementTapped += HandleStatePlaceElementTapped;
            InputManager.StateChartPanelTapped += HandleStateChartPanelTapped;
            InputManager.StateElementDragStarted += HandleStatePlaceElementDragStart;
            InputManager.StateChartPanelDragStarted += HandleStateChartPanelDragStart;
            InputManager.ZoomInputChanged += ProcessZoom;
            
            if (!_setupUIOnEnable) return;

            SetupStateChartUI();
            EnableAvailableUIElements();
            _setupUIOnEnable = false;
        }

        private void OnDisable()
        {
            InputManager.StateElementTapped -= HandleStatePlaceElementTapped;
            InputManager.StateChartPanelTapped -= HandleStateChartPanelTapped;
            InputManager.StateElementDragStarted -= HandleStatePlaceElementDragStart;
            InputManager.StateChartPanelDragStarted -= HandleStateChartPanelDragStart;
            InputManager.ZoomInputChanged -= ProcessZoom;
        }

        public void Initialize()
        {
            _canvas = GetComponent<Canvas>();
            _stateChartManager = GameManager.Instance.GetStateChartManager();
            _inputManager = GameManager.Instance.GetInputManager();
            _stateChartUIGrid = stateChartPanel.GetComponent<StateChartUIGrid>();
            _placedStateElements = new List<StateUIPlaceElement>();
            _zoomFactor = 1f;
        }

        public void ProcessZoom(float zoomFactorChange, Vector2 zoomCenter)
        {
            const float minZoomFactor = 1f;
            const float maxZoomFactor = 2f;
            Debug.Log($"ProcessZoom got called with factorChange of: {zoomFactorChange}");
            if (Mathf.Approximately(_zoomFactor, minZoomFactor)  && zoomFactorChange < 0 || Mathf.Approximately(_zoomFactor, maxZoomFactor) && zoomFactorChange > 0)
                return;

            var zoomDelta = zoomFactorChange / _zoomFactor;
            _zoomFactor += zoomFactorChange;
            _zoomFactor = Mathf.Clamp(_zoomFactor, minZoomFactor, maxZoomFactor);
            Debug.Log($"Zoom Factor is {_zoomFactor}");
            LogStateChartPanelTransform();
            stateChartPanel.ZoomChart(_zoomFactor, zoomDelta, zoomCenter);
            LogStateChartPanelTransform();
        }

        private void LogStateChartPanelTransform()
        {
            var sTransform = stateChartPanel.transform;
            Debug.Log($"Panel Position is {sTransform.position}");
            Debug.Log($"Panel Scale is {((RectTransform)transform).sizeDelta}");
        }

        public void SetupUI(List<LevelData.AvailableStateInfo> availableStateInfo,
            List<StateChartManager.TransitionCondition> availableTransitionConditions)
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
                stateChartPanel.Initialize();
                _stateChartUIInitialized = true;
            }
            
            var startCell = _stateChartUIGrid.GetCellOnCoordinates(new ByteCoordinates(0, 3), out var cellPosition);
            startCell.PlaceStateElement(startStateUIElement);
            startStateUIElement.Initialize(stateChartPanel.GetScaleFactor());
            startStateUIElement.transform.position = cellPosition;
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
            foreach (var stateUIElementStack in stateUIElementStacks)
            {
                if(!stateUIElementStack.gameObject.activeSelf)
                    continue;
                
                stateUIElementStack.DestroyStates();
            }
            
            _stateChartUIGrid.ClearGridCells();
            _placedStateElements.Clear();
            startStateUIElement.ClearDefaultStateLine();
            _connectedTransitions.Clear();
        }

        public void HandleTransitionSelectElementClicked(TransitionUIData transitionUIData)
        {
            _transitionPlaceElement = Instantiate(transitionPlaceElementPrefab, Input.mousePosition,
                Quaternion.identity, transform);
            _transitionPlaceElement.Initialize(this, transitionUIData);
        }

        private void HandleStatePlaceElementTapped(StateUIPlaceElement stateElement)
        {
            // Todo: highlight connected transitions
        }

        private void HandleStateChartPanelTapped()
        {
            // Todo: reset transition highlighting
        }

        private void HandleStatePlaceElementDragStart(StateUIPlaceElement stateElement)
        {
            InputManager.DragEnded += HandleDragEnded;
            _statePlaceElement = stateElement;
            
            var connectedCell = _statePlaceElement.GetConnectedCell();
            if (connectedCell != null)
            {
                RemoveStatePlaceElement(_statePlaceElement);
                connectedCell.RemoveStateElement();
            }
            else
            {
                stateUIElementStacks.First(stack => stack.GetAction() == _statePlaceElement.GetAction()).RemoveState(_statePlaceElement);
            }
            
            StartCoroutine(DragStatePlaceElement());
        }

        private void HandleStateChartPanelDragStart()
        {
            InputManager.DragEnded += HandleDragEnded;
            StartCoroutine(DragStateChartPanel());
        }
        
        private void HandleDragEnded()
        {
            InputManager.DragEnded -= HandleDragEnded;
            Debug.Log("Input was released.");
            _dragEnded = true;
        }

        private IEnumerator DragStatePlaceElement()
        {
            var wasOnGrid = false;
            _statePlaceElement.SwitchAppearanceToOffGrid();
            _statePlaceElement.transform.SetAsLastSibling();
            StateChartCell currentCell;

            do
            {
                Vector3 inputPosition = _inputManager.GetPointerPosition();
                Debug.Log($"Will drag panel to this position: {inputPosition}");
                var stateElementTransform = _statePlaceElement.transform;
                currentCell = _stateChartUIGrid.TryGetEmptyCellOnPosition(inputPosition, out var cellPosition);
                if (currentCell != null)
                {
                    if (!wasOnGrid)
                    {
                        _statePlaceElement.SwitchAppearanceToOnGrid(_zoomFactor);
                    }
                    stateElementTransform.position = cellPosition;
                    wasOnGrid = true;
                }
                else
                {
                    if (wasOnGrid)
                    {
                        _statePlaceElement.SwitchAppearanceToOffGrid();
                    }
                    stateElementTransform.position = inputPosition;
                    wasOnGrid = false;
                }

                yield return null;
            } while (!_dragEnded);

            if (wasOnGrid)
            {
                currentCell.PlaceStateElement(_statePlaceElement);
                _statePlaceElement.PlaceState(currentCell, stateChartPanel.transform);
                
                var assignedId = _stateChartManager.AddState(_statePlaceElement.GetAction());
                _statePlaceElement.SetAssignedId(assignedId);
                _placedStateElements.Add(_statePlaceElement);
                _statePlaceElement = null;
                
            }
            else
            {
                stateUIElementStacks.First(s => s.GetAction() == _statePlaceElement.GetAction())
                    .AddState(stateChartPanel.GetScaleFactor());
                Destroy(_statePlaceElement.gameObject);
            }

            _dragEnded = false;
        }

        private IEnumerator DragStateChartPanel()
        {
            Vector3 previousInputPosition = _inputManager.GetPointerPosition();
            do
            {
                Vector3 currentInputPosition = _inputManager.GetPointerPosition();
                var inputDifference = currentInputPosition - previousInputPosition;
                stateChartPanel.MoveByVector(inputDifference);
                previousInputPosition = currentInputPosition;
                yield return null;
            } while (!_dragEnded);

            _dragEnded = false;
        }

        private void RemoveStatePlaceElement(StateUIPlaceElement placeElement)
        {
            for (int i = 0; i < _connectedTransitions.Count; i++)
            {
                var connectedTransition = _connectedTransitions.ElementAt(i);
                if (connectedTransition.Key.Item2 != placeElement &&
                    connectedTransition.Key.Item1 != placeElement.GetComponent<StateUIElement>())
                    continue;

                var plug = connectedTransition.Value;
                RemoveTransitionByPlug(plug);
                i--;
            }

            _stateChartManager.RemoveState(_statePlaceElement.GetAssignedId());
            _placedStateElements.Remove(placeElement);
            _statePlaceElement.SetAssignedId(-1);
        }

        public void RemoveTransitionByPlug(TransitionPlug plug, bool destinationStateWillBeRemoved = false)
        {
            var transitionCondition = plug.transitionCondition;
            var fromState = plug.connectedState;
            if (transitionCondition == StateChartManager.TransitionCondition.Default)
            {
                _stateChartManager.RemoveDefaultTransition(fromState.AssignedId);
            }
            else
            {
                _stateChartManager.RemoveTransition(plug.transitionCondition, fromState.AssignedId);
            }

            var keyToRemove = _connectedTransitions.First(item => item.Value == plug).Key;
            _connectedTransitions.Remove(keyToRemove);


            if (!destinationStateWillBeRemoved)
            {
                keyToRemove.Item2.SetSlotToEmpty(plug.GetConnectedSlotId());
            }

            plug.DisconnectLine();
        }
        
        public void HandleTransitionPlaceElementReleased(TransitionUIData transitionUIData)
        {
            var mouseOverState = HelperFunctions.CheckIfMouseIsOverObjectWithComponent<StateUIPlaceElement>();
            if (mouseOverState == null)
            {
                return;
            }

            mouseOverState.AddTransitionPlugToState(transitionUIData);
        }

        public void HandleNewTransitionConnected(StateUIElement state1, StateUIPlaceElement state2,
            TransitionPlug plug)
        {
            _connectedTransitions.Add((state1, state2), plug);
            if (plug.transitionCondition == StateChartManager.TransitionCondition.Default)
            {
                _stateChartManager.AddDefaultTransition(state1.AssignedId, state2.GetAssignedId());
            }
            else
            {
                _stateChartManager.AddTransition(plug.transitionCondition, state1.AssignedId,
                    state2.GetAssignedId());
            }
        }

        public List<StateUIPlaceElement> GetPlacedStates()
        {
            return _placedStateElements;
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