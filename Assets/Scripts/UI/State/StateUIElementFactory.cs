using System;
using System.Collections.Generic;
using Robot;
using UI.UIData;
using UnityEngine;

namespace UI.State
{
    public class StateUIElementFactory : MonoBehaviour
    {
        [Header("StateData")]
        [SerializeField] private StateUIData goForwardStateData;
        [SerializeField] private StateUIData turnRightStateData;
        [SerializeField] private StateUIData turnLeftStateData;
        [SerializeField] private StateUIData grabStateData;
        [SerializeField] private StateUIData dropStateData;

        [Header("References")]
        [SerializeField] private StateUIPlaceElement stateUIElementPrefab;
        [SerializeField] private Transform stateChartPanelTransform;

        private Dictionary<StateChartManager.StateAction, StateUIData> _stateData;

        private void Awake()
        {
            _stateData = new Dictionary<StateChartManager.StateAction, StateUIData>
            {
                { StateChartManager.StateAction.GoForward, goForwardStateData },
                { StateChartManager.StateAction.TurnRight, turnRightStateData },
                { StateChartManager.StateAction.TurnLeft, turnLeftStateData },
                { StateChartManager.StateAction.Grab, grabStateData },
                { StateChartManager.StateAction.Drop, dropStateData }
            };
        }

        public StateUIPlaceElement CreateStateUIElement(StateChartManager.StateAction action)
        {
            var stateData = _stateData[action];
            if (stateData == null)
                throw new ArgumentException("StateAction to instantiate is not registered.");

            var newState = Instantiate(stateUIElementPrefab, stateChartPanelTransform);
            newState.Initialize(stateData);
            return newState;
        }
    }
}