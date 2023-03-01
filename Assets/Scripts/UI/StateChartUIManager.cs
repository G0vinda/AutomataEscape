using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Helper;

namespace UI
{
    public class StateChartUIManager : MonoBehaviour
    {
        [SerializeField] private List<StateSelectElement> stateSelectElements;
        [SerializeField] private StateUIPlaceElement statePlaceElementPrefab;
        [SerializeField] private List<TransitionSelectElement> transitionSelectElements;
        [SerializeField] private TransitionPlaceElement transitionPlaceElementPrefab;
        [SerializeField] private StateUIElement startStateElement;
        [SerializeField] private StateUIData startStateData;
        [SerializeField] private StateChartUIScaler stateChartUIScaler;

        private StateChartManager _stateChartManager;
        private bool _isInPlacement;
        private bool _setupUIOnEnable;
        private StateUIPlaceElement _statePlaceElement;
        private TransitionPlaceElement _transitionPlaceElement;
        private List<StateChartManager.StateAction> _availableActions;
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
        }

        public void SetupUI(List<StateChartManager.StateAction> availableActions,
            List<StateChartManager.TransitionCondition> availableTransitionConditions)
        {
            ClearStateChartUI();
            _availableActions = availableActions;
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
            stateSelectElements.ForEach(x => x.gameObject.SetActive(_availableActions.Contains(x.Action)));
            transitionSelectElements.ForEach(x =>
                x.gameObject.SetActive(_availableTransitionConditions.Contains(x.Condition)));
        }
    

        private void ClearStateChartUI()
        {
            var deleteCounter = transform.childCount - 1;
            while (deleteCounter > 0)
            {
                var childObject = transform.GetChild(deleteCounter).gameObject;
                if (childObject.TryGetComponent(out StateUIPlaceElement placeElement))
                {
                    Destroy(childObject);
                }

                deleteCounter--;
            }
            
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

        public void HandleStatePlaceElementClicked(StateUIPlaceElement placeElement)
        {
            if (_isInPlacement)
            {
                return;
            }

            _statePlaceElement = placeElement;
            _isInPlacement = true;
            RemoveStatePlaceElement(placeElement);
        }


        private void RemoveStatePlaceElement(StateUIPlaceElement placeElement)
        {
            for (int i = 0; i < _connectedTransitions.Count; i++)
            {
                var connectedTransition = _connectedTransitions.ElementAt(i);
                if (connectedTransition.Key.Item2 != placeElement && connectedTransition.Key.Item1 != placeElement)
                    continue;
                
                var plug = connectedTransition.Value;
                RemoveTransitionByPlug(plug);
                i--;
            }

            _stateChartManager.RemoveState(_statePlaceElement.AssignedId);
            _statePlaceElement.AssignedId = -1;
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
            if (!HelperFunctions.CheckIfMouseIsOverObjectWithTag("DropZone"))
            {
                return false;
            }

            int assignedId = _stateChartManager.AddState(stateUIData.action);
            _statePlaceElement.AssignedId = assignedId;
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
                _stateChartManager.AddDefaultTransition(state1.AssignedId, state2.AssignedId);
            }
            else
            {
                _stateChartManager.AddTransition(plug.transitionCondition, state1.AssignedId,
                    state2.AssignedId);
            }
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