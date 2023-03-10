using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Helper;
using UnityEngine.InputSystem;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private List<StateUIElementStack> stateUIElementStacks;
        [SerializeField] private StateUIPlaceElement statePlaceElementPrefab;
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
        private UIInput _input;
        private float _zoomFactor;
        private bool _inputReleased;

        private Dictionary<(StateUIElement, StateUIPlaceElement), TransitionPlug> _connectedTransitions = new();
        private Canvas _canvas;

        private WaitForEndOfFrame _nextFrame;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _input = new UIInput();
            _nextFrame = new WaitForEndOfFrame();
        }

        private void OnEnable()
        {
            _input.Enable();
            _input.Input.Zoom.performed += ProcessZoom;
            
            if (!_setupUIOnEnable) return;

            SetupStateChartUI();
            EnableAvailableUIElements();
            _setupUIOnEnable = false;
        }

        private void OnDisable()
        {
            _input.Disable();
            _input.Input.Zoom.performed -= ProcessZoom;
        }
        
        private void ProcessZoom(InputAction.CallbackContext context)
        {
            float zoomDelta;
            if (context.ReadValue<float>() > 0)
            {
                if(_zoomFactor > 1.85f)
                    return;

                zoomDelta = 0.15f;
            }
            else
            {
                if(_zoomFactor < 1.15f)
                    return;
                
                zoomDelta = -0.15f;
            }

            _zoomFactor += zoomDelta;
            stateChartPanel.ZoomChart(_zoomFactor, zoomDelta, _input.Input.Position.ReadValue<Vector2>());
        }

        private void Start()
        {
            _stateChartManager = GameManager.Instance.GetStateChartManager();
            _placedStateElements = new List<StateUIPlaceElement>();
            stateChartPanel.Initialize();
            _stateChartUIGrid = stateChartPanel.GetComponent<StateChartUIGrid>();
            _zoomFactor = 1f;
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
            var startCell = _stateChartUIGrid.GetCellOnCoordinates(new ByteCoordinates(0, 3), out var cellPosition);
            startCell.PlaceStateElement(startStateUIElement);
            startStateUIElement.Initialize(stateChartPanel.GetScaleFactor());
            startStateUIElement.transform.position = cellPosition;
        }

        private void EnableAvailableUIElements()
        {
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

        public void HandleStatePlaceElementClicked(StateUIPlaceElement clickedElement, StateChartCell connectedCell)
        {
            StartCoroutine(ProcessPlayerInput(clickedElement, connectedCell));
        }

        private IEnumerator ProcessPlayerInput(StateUIPlaceElement clickedElement, StateChartCell connectedCell)
        {
            _input.Input.PressRelease.performed += OnInputRelease;
            while (true)
            {
                if (_inputReleased) // Player input was click
                {
                    StatePlaceElementClicked();
                    _input.Input.PressRelease.performed -= OnInputRelease;
                    break;
                }

                if (_input.Input.PositionDelta.ReadValue<Vector2>() != Vector2.zero) // Player input is drag
                {
                    _statePlaceElement = clickedElement;

                    if (connectedCell != null)
                    {
                        RemoveStatePlaceElement(clickedElement);
                        connectedCell.RemoveStateElement();
                    }
                    else
                    {
                        stateUIElementStacks.First(s => s.GetAction() == clickedElement.GetAction()).RemoveState(clickedElement);
                    }

                    StartCoroutine(DragStatePlaceElement());
                    break;
                }
                yield return _nextFrame;
            }
        }

        private void StatePlaceElementClicked()
        {
            _inputReleased = false;
        }

        private IEnumerator DragStatePlaceElement()
        {
            var wasOnGrid = false;
            _statePlaceElement.SwitchAppearanceToOffGrid();
            _statePlaceElement.transform.SetAsLastSibling();
            StateChartCell currentCell;

            do
            {
                Vector3 inputPosition = _input.Input.Position.ReadValue<Vector2>();
                currentCell = _stateChartUIGrid.TryGetEmptyCellOnPosition(inputPosition, out var cellPosition);
                if (currentCell != null)
                {
                    if (!wasOnGrid)
                    {
                        _statePlaceElement.SwitchAppearanceToOnGrid(_zoomFactor);
                    }
                    _statePlaceElement.transform.position = cellPosition;
                    wasOnGrid = true;
                }
                else
                {
                    if (wasOnGrid)
                    {
                        _statePlaceElement.SwitchAppearanceToOffGrid();
                    }
                    _statePlaceElement.transform.position = inputPosition;
                    wasOnGrid = false;
                }

                yield return _nextFrame;
            } while (!_inputReleased);

            if (wasOnGrid)
            {
                currentCell.PlaceStateElement(_statePlaceElement);
                _statePlaceElement.PlaceState(currentCell);
                
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

            _inputReleased = false;
            _input.Input.PressRelease.performed += OnInputRelease;
        }
        
        private void OnInputRelease(InputAction.CallbackContext obj)
        {
            _inputReleased = true;
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