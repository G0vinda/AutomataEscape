using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class StateUIElementStack : MonoBehaviour
    {
        [SerializeField] private StateUIData stateData;
        [SerializeField] private StateUIPlaceElement stateElementPrefab;
        [SerializeField] private float stateElementPlaceOffset;
    
        private List<StateUIPlaceElement> _stateElements;

        public void Initialize(int numberOfStates, float gridScaleFactor)
        {
            _stateElements = new List<StateUIPlaceElement>();
            ((RectTransform)transform).sizeDelta *= gridScaleFactor;
            stateElementPlaceOffset *= gridScaleFactor;
            Debug.Log($"StatePlaceElementOffset: {stateElementPlaceOffset}");
            for (var i = 0; i < numberOfStates; i++)
            {
                AddState(gridScaleFactor);
            }
        }

        public StateChartManager.StateAction GetAction()
        {
            return stateData.action;
        }

        public void AddState(float gridScaleFactor)
        {
            var newState = Instantiate(stateElementPrefab, transform);
            newState.Initialize(stateData, gridScaleFactor);
            newState.SetToAvailable();
            var numOfStates = _stateElements.Count;
            if(numOfStates > 0)
                _stateElements[^1].SetToUnavailable();
            
            var newStateTransform = newState.GetComponent<RectTransform>();
            newStateTransform.SetSiblingIndex(numOfStates);

            var newStatePosition = newStateTransform.localPosition;
            newStatePosition.y = stateElementPlaceOffset * numOfStates - newStateTransform.sizeDelta.y * 0.5f;
            newStateTransform.localPosition = newStatePosition;
            _stateElements.Add(newState);
        }

        public void RemoveState(StateUIPlaceElement stateUIPlaceElement)
        {
            _stateElements.Remove(stateUIPlaceElement);
            if(_stateElements.Count > 0)
                _stateElements[^1].SetToAvailable();
        }

        public void DestroyStates()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
