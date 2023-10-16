using System;
using System.Collections;
using UI.State;
using UnityEngine;

namespace UI.Grid
{
    [RequireComponent(typeof(UIGridManager))]
    public class HintCreator : MonoBehaviour
    {
        [SerializeField] private float createLoopHintWaitTime;
        [SerializeField] private CreateLoopHint createLoopHintPrefab;
        
        private UIGridManager _gridManager;
        private float _timeSpentInProgramViewWithOneConnection;
        private bool _loopCreated;
        private bool _firstConnectionCreated;
        private bool _isInProgramView;
        private StateUIPlaceElement _hintStateUIPlaceElement;
        private CreateLoopHint _createLoopHint;

        #region OnEnable/OnDisable

        private void OnEnable()
        {
            UIManager.TransitionCreated += HandleTransitionCreated;
            UIManager.TransitionRemoved += HandleTransitionRemoved;
            UIManager.ViewStateChanged += HandleViewStateChanged;
        }

        private void OnDisable()
        {
            UIManager.TransitionCreated -= HandleTransitionCreated;
            UIManager.TransitionRemoved -= HandleTransitionRemoved;
            UIManager.ViewStateChanged -= HandleViewStateChanged;
        }

        #endregion

        private void Awake()
        {
            _gridManager = GetComponent<UIGridManager>();
        }
        
        private void Start()
        {
            if (GameManager.Instance.GetCurrentLevelId() == 0)
            {
                StartCoroutine(WaitForCreateLoopHint());
            }
            else
            {
                Destroy(this);
            }
        }

        private void HandleTransitionCreated((StateUIElement, StateUIPlaceElement) connectedStates)
        {
            var sourceState = connectedStates.Item1;
            var destinationState = connectedStates.Item2;

            if (sourceState.GetComponent<StartStateUIElement>() != null)
            {
                _firstConnectionCreated = true;
                _hintStateUIPlaceElement = destinationState;
            }
            else if (sourceState == destinationState.GetComponent<StateUIElement>())
            {
                _loopCreated = true;
                Destroy(_createLoopHint.gameObject);
                Destroy(this);
            }
        }
        
        private void HandleTransitionRemoved((StateUIElement, StateUIPlaceElement) disconnectedStates)
        {
            var sourceState = disconnectedStates.Item1;
            var destinationState = disconnectedStates.Item2;

            if (sourceState.GetComponent<StartStateUIElement>() != null)
            {
                _firstConnectionCreated = false;
            }
        }

        private void HandleViewStateChanged(bool newViewState)
        {
            _isInProgramView = newViewState;
        }

        private IEnumerator WaitForCreateLoopHint()
        {
            while (true)
            {
                if(_loopCreated)
                    break;

                if (_isInProgramView && _firstConnectionCreated)
                    _timeSpentInProgramViewWithOneConnection += Time.deltaTime;

                if (_timeSpentInProgramViewWithOneConnection > createLoopHintWaitTime)
                {
                    _createLoopHint = Instantiate(createLoopHintPrefab, transform);
                    var stateCell = _hintStateUIPlaceElement.GetComponent<StateUIElement>().ConnectedCell;
                    _createLoopHint.StartDraw(
                        _gridManager.GetSubCellSize(), 
                        _hintStateUIPlaceElement.transform.position, 
                        _gridManager.GetCoordinatesFromCell(stateCell).y < 6);
                    break;
                }
                
                yield return null;
            }
        }
    }
}
