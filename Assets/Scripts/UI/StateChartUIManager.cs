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
        [SerializeField] private List<TransitionSelectElement> transitionSelectElements;
        [SerializeField] private StatePlaceElement statePlaceElementPrefab;
        [SerializeField] private StatePlaceElement startStateElement;
        [SerializeField] private TransitionPlaceElement transitionPlaceElementPrefab;
        [SerializeField] private StateUIData startStateData;

        public static StateChartUIManager Instance;

        private bool _isInPlacement;
        private StatePlaceElement _statePlaceElement;
        private TransitionPlaceElement _transitionPlaceElement;

        private Dictionary<(StatePlaceElement, StatePlaceElement), TransitionPlug> _connectedTransitions = new();
        private Canvas _canvas;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _canvas = GetComponent<Canvas>();
            }
            else
            {
                Destroy(this);
            }
        }

        public void SetupUI(List<StateChartManager.StateAction> availableActions,
            List<StateChartManager.TransitionCondition> availableTransitionConditions)
        {
            ClearChart();
            
            // Setup start state
            startStateElement.Initialize(this, startStateData);
            startStateElement.PlaceState();
            startStateElement.AssignedId = 0;

            // Enabled UI Elements available for that level
            stateSelectElements.ForEach(x => x.gameObject.SetActive(availableActions.Contains(x.Action)));
            transitionSelectElements.ForEach(x =>
                x.gameObject.SetActive(availableTransitionConditions.Contains(x.Condition)));
        }

        private void ClearChart()
        {
            var deleteCounter = transform.childCount - 1;
            while (deleteCounter > 0)
            {
                var childObject = transform.GetChild(deleteCounter).gameObject;
                if (childObject.TryGetComponent(out StatePlaceElement placeElement))
                {
                    RemoveStatePlaceElement(placeElement);
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
            _statePlaceElement.Initialize(this, stateUIData);
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

        public void HandleStatePlaceElementClicked(StatePlaceElement placeElement)
        {
            if (_isInPlacement)
            {
                return;
            }

            _statePlaceElement = placeElement;
            _isInPlacement = true;
            RemoveStatePlaceElement(placeElement);
        }


        private void RemoveStatePlaceElement(StatePlaceElement placeElement)
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

            StateChartManager.Instance.RemoveState(_statePlaceElement.AssignedId);
            _statePlaceElement.AssignedId = -1;
        }

        public void RemoveTransitionByPlug(TransitionPlug plug, bool destinationStateWillBeRemoved = false)
        {
            var transitionCondition = plug.transitionCondition;
            var fromState = plug.connectedState;
            if (transitionCondition == StateChartManager.TransitionCondition.Default)
            {
                StateChartManager.Instance.RemoveDefaultTransition(fromState.AssignedId);
            }
            else
            {
                StateChartManager.Instance.RemoveTransition(plug.transitionCondition, fromState.AssignedId);
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

            int assignedId = StateChartManager.Instance.AddState(stateUIData.action);
            _statePlaceElement.AssignedId = assignedId;
            return true;
        }

        public void HandleTransitionPlaceElementReleased(TransitionUIData transitionUIData)
        {
            _isInPlacement = false;
            var mouseOverState = HelperFunctions.CheckIfMouseIsOverObjectWithComponent<StatePlaceElement>();
            if (mouseOverState == null)
            {
                return;
            }

            mouseOverState.AddTransitionPlugToState(transitionUIData);
        }

        public void HandleNewTransitionConnected(StatePlaceElement state1, StatePlaceElement state2,
            TransitionPlug plug)
        {
            _connectedTransitions.Add((state1, state2), plug);
            if (plug.transitionCondition == StateChartManager.TransitionCondition.Default)
            {
                StateChartManager.Instance.AddDefaultTransition(state1.AssignedId, state2.AssignedId);
            }
            else
            {
                StateChartManager.Instance.AddTransition(plug.transitionCondition, state1.AssignedId,
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