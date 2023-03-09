using System;
using UnityEngine;

namespace UI
{
    public class StateChartPanel : MonoBehaviour
    {
        [SerializeField] private float yMargin;
        [SerializeField] private float padding;

        private UIManager _uiManager;
        private RectTransform _rectTransform;
        private StateChartUIGrid _stateChartUIGrid;
        private Vector2 _defaultScale;
        private float _scaleFactor;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _stateChartUIGrid = GetComponent<StateChartUIGrid>();
        }

        public void Initialize()
        {
            _uiManager = GameManager.Instance.GetUIManager();
            ScaleChartToFitScreen();
            Debug.Log($"StateChartPanelsPosition is {_rectTransform.position}");
            var gridHeight = _uiManager.ScaleFloat(_rectTransform.sizeDelta.y - 2 * padding);
            var topLeftGridPosition = (Vector2)transform.position +
                                      new Vector2(_uiManager.ScaleFloat(padding), -gridHeight * 0.5f);
            _stateChartUIGrid.Initialize(gridHeight, topLeftGridPosition);
        }

        public float GetScaleFactor()
        {
            return _scaleFactor;
        }

        private void ScaleChartToFitScreen()
        {
            var scaledHeight = _uiManager.ScaleFloat(_rectTransform.sizeDelta.y + 2 * yMargin);
            _scaleFactor = Screen.height < scaledHeight ? Screen.height / scaledHeight : 1f;
            
            _rectTransform.sizeDelta *= _scaleFactor;
            _defaultScale = _rectTransform.sizeDelta;
        }

        public void ZoomChart(float zoomFactor, float zoomDelta, Vector2 zoomCenter)
        {
            Vector2 panelCenterOffset = new Vector2(_uiManager.ScaleFloat(_rectTransform.sizeDelta.x * 0.5f), 0);
            Vector2 panelCenter = (Vector2)_rectTransform.position + panelCenterOffset;
            var zoomCenterDifference = zoomCenter - panelCenter;
            var zoomOffset = - zoomCenterDifference * zoomDelta;
            _rectTransform.position += (Vector3)zoomOffset;
            _rectTransform.sizeDelta = _defaultScale * zoomFactor;
        }
    }
}