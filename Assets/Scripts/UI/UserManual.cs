using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    public class UserManual : MonoBehaviour
    {
        [SerializeField] private RectTransform viewport;
        [SerializeField] private float topBottomMargin;

        private ScrollRect _scrollRect;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        #region OnEnable/OnDisable

        private void OnEnable()
        {
            _scrollRect.verticalNormalizedPosition = 1f;
            GameManager.RobotStateChanged += HandleRobotOrViewStateChanged;
            UIManager.ViewStateChanged += HandleRobotOrViewStateChanged;
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
    }
}
