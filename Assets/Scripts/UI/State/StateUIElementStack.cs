using System;
using System.Collections.Generic;
using Robot;
using UI.UIData;
using UnityEngine;

namespace UI.State
{
    public class StateUIElementStack : MonoBehaviour
    {
        [SerializeField] private StateUIData stateData;
        [SerializeField] private StateUIPlaceElement stateElementPrefab;
        [SerializeField] private float stateElementPlaceOffset;
    
        private List<StateUIPlaceElement> _stateElements;
        private Vector2 _originalSize;
        private float _originalPlaceOffset;

        private void Awake()
        {
            _originalSize = GetComponent<RectTransform>().sizeDelta;
            _originalPlaceOffset = stateElementPlaceOffset;
        }

        public void Setup(int numberOfStates, float gridScaleFactor)
        {
            _stateElements = new List<StateUIPlaceElement>();
            GetComponent<RectTransform>().sizeDelta = _originalSize * gridScaleFactor;
            stateElementPlaceOffset = _originalPlaceOffset * gridScaleFactor;
            DestroyStates();
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
            newStateTransform.SetAsLastSibling();

            if(_stateElements.Count > 0)
                _stateElements[^1].Draggable = false;
            
            newState.Draggable = true;

            var newStatePosition = newStateTransform.localPosition;
            newStatePosition.y = stateElementPlaceOffset * _stateElements.Count - newStateTransform.sizeDelta.y * 0.5f;
            newStateTransform.localPosition = newStatePosition;
            _stateElements.Add(newState);
        }

        public void RemoveState(StateUIPlaceElement stateUIPlaceElement)
        {
            _stateElements.Remove(stateUIPlaceElement);
            if(_stateElements.Count > 0)
                _stateElements[^1].Draggable = true;
        }

        private void DestroyStates()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
