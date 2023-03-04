using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class StateUIElementStack : MonoBehaviour
    {
        [SerializeField] private StateUIData stateData;
        [SerializeField] private StateUIPlaceElement stateElementPrefab;
        [SerializeField] private float stateElementPlaceOffset;
    
        private List<RectTransform> _stateElementTransforms;

        public void Initialize(int numberOfStates)
        {
            _stateElementTransforms = new List<RectTransform>();
            for (var i = 0; i < numberOfStates; i++)
            {
                AddState();
            }
        }

        public StateChartManager.StateAction GetAction()
        {
            return stateData.action;
        }

        public void AddState()
        {
            var newState = Instantiate(stateElementPrefab, transform);
            newState.Initialize(stateData);
            var newStateTransform = newState.GetComponent<RectTransform>();
            var newStateIndex = _stateElementTransforms.Count;
            newStateTransform.SetSiblingIndex(newStateIndex);
            var newStatePosition = newStateTransform.localPosition;
            newStatePosition.y = stateElementPlaceOffset * newStateIndex;
            newStateTransform.localPosition = newStatePosition;
            _stateElementTransforms.Add(newStateTransform);
        }

        public void RemoveState(StateUIPlaceElement stateUIPlaceElement)
        {
            RemoveState(stateUIPlaceElement.GetComponent<RectTransform>());
        }

        public void RemoveState(RectTransform stateElementTransform)
        {
            _stateElementTransforms.Remove(stateElementTransform);
        }
    }
}
