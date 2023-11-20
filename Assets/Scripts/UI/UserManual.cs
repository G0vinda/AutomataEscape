using System;
using System.Collections;
using System.Collections.Generic;
using Robot;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace UI
{
    public class UserManual : MonoBehaviour
    {
        [SerializeField] private RectTransform viewport;
        [SerializeField] private float topBottomMargin;
        [SerializeField] private RectTransform contentTransform;
        
        [Header("StateEntries")] 
        [SerializeField] private GameObject goForwardStateEntry;
        [SerializeField] private GameObject turnRightStateEntry;
        [SerializeField] private GameObject turnLeftStateEntry;
        [SerializeField] private GameObject grabStateEntry;
        [SerializeField] private GameObject dropStateEntry;

        [Header("TransitionEntries")] 
        [SerializeField] private GameObject defaultTransitionEntry;
        [SerializeField] private GameObject isInFrontOfWallTransitionEntry;
        [SerializeField] private GameObject standsOnOrangeTransitionEntry;
        [SerializeField] private GameObject standsOnPurpleTransitionEntry;
        [SerializeField] private GameObject standsOnKeyTransitionEntry;

        private ScrollRect _scrollRect;
        private Dictionary<StateChartManager.StateAction, GameObject> _stateEntries = new();
        private Dictionary<StateChartManager.TransitionCondition, GameObject> _transitionEntries = new();
        private bool _dirtyState; // is true when rerender is needed
        private List<StateChartManager.StateAction> _statesToEnable;
        private List<StateChartManager.TransitionCondition> _transitionsToEnable;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            
            _stateEntries = new Dictionary<StateChartManager.StateAction, GameObject>()
            {
                { StateChartManager.StateAction.GoForward, goForwardStateEntry },
                { StateChartManager.StateAction.TurnRight, turnRightStateEntry },
                { StateChartManager.StateAction.TurnLeft, turnLeftStateEntry },
                { StateChartManager.StateAction.Grab, grabStateEntry },
                { StateChartManager.StateAction.Drop, dropStateEntry }
            };
         
            foreach (var (_, entry) in _stateEntries)
            {
                entry.SetActive(false);
            }

            _transitionEntries = new Dictionary<StateChartManager.TransitionCondition, GameObject>()
            {
                { StateChartManager.TransitionCondition.Default, defaultTransitionEntry },
                { StateChartManager.TransitionCondition.IsInFrontOfWall, isInFrontOfWallTransitionEntry },
                { StateChartManager.TransitionCondition.StandsOnOrange, standsOnOrangeTransitionEntry },
                { StateChartManager.TransitionCondition.StandsOnPurple, standsOnPurpleTransitionEntry },
                { StateChartManager.TransitionCondition.StandsOnKey, standsOnKeyTransitionEntry },
            };
            
            foreach (var (_, entry) in _transitionEntries)
            {
                entry.SetActive(false);
            }
        }

        #region OnEnable/OnDisable

        private void OnEnable()
        {
            _scrollRect.verticalNormalizedPosition = 1f;
            GameManager.RobotStateChanged += HandleRobotOrViewStateChanged;
            UIManager.ViewStateChanged += HandleRobotOrViewStateChanged;
            if (_dirtyState)
            {
                _dirtyState = false;
                StartCoroutine(DelayedStateSetup());
            }
        }
        private void OnDisable()
        {
            GameManager.RobotStateChanged -= HandleRobotOrViewStateChanged;
            UIManager.ViewStateChanged -= HandleRobotOrViewStateChanged;
        }

        #endregion

        private void Start()
        {
            var uiManager = GameManager.Instance.GetUIManager();
            var viewportHeight = uiManager.UnscaleFloat(Screen.height - 2 * topBottomMargin);
            var viewPortSize = viewport.sizeDelta;
            viewPortSize.y = viewportHeight;
            viewport.sizeDelta = viewPortSize;
        }
        
        private void HandleRobotOrViewStateChanged(bool robotOrViewState)
        {
            if(robotOrViewState) // is true if robot is running or stateChartPanel opened
                gameObject.SetActive(false);
        }

        public void ToggleUserManual()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void EnableManualEntries(List<StateChartManager.StateAction> stateActions, List<StateChartManager.TransitionCondition> transitionConditions)
        {
            _dirtyState = true;
            _statesToEnable = stateActions;
            _transitionsToEnable = transitionConditions;
        }

        private IEnumerator DelayedStateSetup() // This delayed setup is necessary to render the layout correctly
        {
            foreach (var (action, entry) in _stateEntries)
            {
                if (_statesToEnable.Contains(action))
                {
                    entry.SetActive(true);
                }
            }
            
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
            _scrollRect.verticalNormalizedPosition = 1f;

            if (_transitionsToEnable.Count > 1)
            {
                foreach (var (condition, entry) in _transitionEntries)
                {
                    if (_transitionsToEnable.Contains(condition))
                    {
                        entry.SetActive(true);
                    }
                }
            }
            
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
            
            yield return null;
            _scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
