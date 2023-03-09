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
        private float _gridHeight;
        private Vector2 _bottomLeftGridPosition;

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
            CalculateGridValues();
            _stateChartUIGrid.Initialize(_gridHeight, _bottomLeftGridPosition);
        }

        public float GetScaleFactor()
        {
            return _scaleFactor;
        }

        private void CalculateGridValues()
        {
            _gridHeight = _uiManager.ScaleFloat(_rectTransform.sizeDelta.y - 2 * padding);
            _bottomLeftGridPosition = (Vector2)transform.position +
                                      new Vector2(_uiManager.ScaleFloat(padding), -_gridHeight * 0.5f);
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
            var zoomCenterDifference = zoomCenter - (Vector2)_rectTransform.position;
            var zoomOffset = - zoomCenterDifference * zoomDelta;
            _rectTransform.position += (Vector3)zoomOffset;
            _rectTransform.sizeDelta = _defaultScale * zoomFactor;
            CalculateGridValues();
            _stateChartUIGrid.UpdateGrid(_gridHeight,_bottomLeftGridPosition,zoomFactor);
        }
    }
}