using System;
using System.Collections.Generic;
using Robot;
using UI.UIData;
using UnityEngine;

namespace UI.State
{
    public class StateUIElementFactory : MonoBehaviour
    {
        [SerializeField] private StateUIData goForwardStateData;
        [SerializeField] private StateUIData turnRightStateData;
        [SerializeField] private StateUIData turnLeftStateData;
        [SerializeField] private StateUIData grabStateData;
        [SerializeField] private StateUIData dropStateData;

        [SerializeField] private StateUIPlaceElement stateUIElementPrefab;

        private Dictionary<StateChartManager.StateAction, StateUIData> _stateData;

        public StateUIPlaceElement CreateStateUIElement(StateChartManager.StateAction action)
        {
            var stateData = _stateData[action];
            if (stateData == null)
                throw new ArgumentException("StateAction to instantiate is not registered.");

            var newState = Instantiate(stateUIElementPrefab);
            newState.Initialize(stateData);
            return newState;
        }
    }
}