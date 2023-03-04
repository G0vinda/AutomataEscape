using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Helper;
using PlasticGui;

namespace UI
{
    public class StateChartUIManager : MonoBehaviour
    {
        [SerializeField] private List<StateUIElementStack> stateUIElementStacks;
        [SerializeField] private StateUIPlaceElement statePlaceElementPrefab;
        [SerializeField] private List<TransitionSelectElement> transitionSelectElements;
        [SerializeField] private TransitionPlaceElement transitionPlaceElementPrefab;
        [SerializeField] private StartStateUIElement startStateUIElement;
        [SerializeField] private StateChartUIScaler stateChartUIScaler;

        private StateChartManager _stateChartManager;
        private bool _isInPlacement;
        private bool _setupUIOnEnable;
        private StateUIPlaceElement _statePlaceElement;
        private List<StateUIPlaceElement> _placedStateElements;
        private TransitionPlaceElement _transitionPlaceElement;
        private List<LevelData.AvailableStateInfo> _availableStateInfo;
        private List<StateChartManager.TransitionCondition> _availableTransitionConditions;

        private Dictionary<(StateUIElement, StateUIPlaceElement), TransitionPlug> _connectedTransitions = new();
        private Canvas _canvas;

        private void OnEnable()
        {
            _canvas = GetComponent<Canvas>();

            if (!_setupUIOnEnable) return;

            SetupStateChartUI();
            EnableAvailableUIElements();
            _setupUIOnEnable = false;
        }

        private void Start()
        {
            _stateChartManager = GameManager.Instance.GetStateChartManager();
            _placedStateElements = new List<StateUIPlaceElement>();
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
            // The stateChartUIScaler causes Bugs at the moment and will only be necessary for a mobile version of the game
            // var stateChartUIScaleFactor = stateChartUIScaler.ScaleChart();
            // var startStateElementOffset = new Vector3(100, 0, 0) * stateChartUIScaleFactor;
            // var startStateElementTransform = startStateElement.transform;
            // startStateElementTransform.position = startStateElementTransform.parent.position + startStateElementOffset;
        }

        private void EnableAvailableUIElements()
        {
            foreach (var availableStateInfo in _availableStateInfo)
            {
                var availableStack = stateUIElementStacks.First(stack => stack.GetAction() == availableStateInfo.Action);
                availableStack.gameObject.SetActive(true);
                availableStack.Initialize(availableStateInfo.Amount);
            }
   
            foreach (var transitionSelectElement in transitionSelectElements)
            {
                transitionSelectElement.gameObject.SetActive(
                    _availableTransitionConditions.Contains(transitionSelectElement.Condition));
            }
        }


        private void ClearStateChartUI()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var childObject = transform.GetChild(i).gameObject;
                if (childObject.TryGetComponent(out StateUIPlaceElement placeElement))
                {
                    Destroy(childObject);
                }
            }

            _placedStateElements.Clear();
            startStateUIElement.ClearDefaultStateLine();
            _connectedTransitions.Clear();
        }

        public void HandleStateSelectElementClicked(StateUIData stateUIData)
        {
            if (_isInPlacement)
            {
                return;
            }

            // TODO: Adjust to touch input
            _statePlaceElement =
                Instantiate(statePlaceElementPrefab, Input.mousePosition, Quaternion.identity, transform);
            _statePlaceElement.Initialize(stateUIData);
            _isInPlacement = true;
        }

        public void HandleTransitionSelectElementClicked(TransitionUIData transitionUIData)
        {
            if (_isInPlacement)
            {
                return;
            }

            _transitionPlaceElement = Instantiate(transitionPlaceElementPrefab, Input.mousePosition,
                Quaternion.identity, transform);
            _transitionPlaceElement.Initialize(this, transitionUIData);
            _isInPlacement = true;
        }

        public void HandleStatePlaceElementClicked(StateUIPlaceElement placeElement, bool wasPlaced)
        {
            if (_isInPlacement)
            {
                return;
            }

            if (wasPlaced)
            {
                RemoveStatePlaceElement(placeElement);
            }
            else
            {
                stateUIElementStacks.First(s => s.GetAction() == placeElement.GetAction()).RemoveState(placeElement);
            }

            _statePlaceElement = placeElement;
            _isInPlacement = true;
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


        public bool HandleStatePlaceElementReleased(StateUIData stateUIData)
        {
            _isInPlacement = false;
            if (!HelperFunctions.CheckIfMouseIsOverObjectWithTag("StateChartDropZone"))
            {
                stateUIElementStacks.First(s => s.GetAction() == stateUIData.action).AddState();
                return false;
            }

            var assignedId = _stateChartManager.AddState(stateUIData.action);
            _statePlaceElement.SetAssignedId(assignedId);
            _placedStateElements.Add(_statePlaceElement);
            return true;
        }

        public void HandleTransitionPlaceElementReleased(TransitionUIData transitionUIData)
        {
            _isInPlacement = false;
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

        public float ScaleFloat(float downscaledFloat)
        {
            return downscaledFloat / _canvas.scaleFactor;
        }

        public float DownscaleFloat(float scaledFloat)
        {
            return scaledFloat * _canvas.scaleFactor;
        }
    }
}