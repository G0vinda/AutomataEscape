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
        private Vector2 _defaultSize;
        private float _scaleFactor;
        private float _gridHeight;
        private float _zoomFactor;
        private Vector2 _bottomLeftGridPosition;
        private float _scaledPadding;
        private MovementBoundaries _movementBoundaries;

        private float _defaultStateSize = 130;
        private float _defaultLineWidth = 20;

        public void Initialize(float availableHorizontalSpace)
        {
            _rectTransform = GetComponent<RectTransform>();
            _stateChartUIGrid = GetComponent<StateChartUIGrid>();
            _uiManager = GameManager.Instance.GetUIManager();
            ScaleChartToFitScreen(availableHorizontalSpace);
            _zoomFactor = 1f;
            _scaledPadding = _uiManager.ScaleFloat(padding) * _scaleFactor;
            CalculateGridValues();
            _stateChartUIGrid.Initialize(_gridHeight, _bottomLeftGridPosition);
        }
        
        public float GetScaleFactor()
        {
            return _scaleFactor;
        }

        public void MoveByVector(Vector3 moveVector)
        {
            var newPosition = transform.position + moveVector;
            transform.position = _movementBoundaries.ClampVector2(newPosition);
            CalculateGridValues();
            _stateChartUIGrid.UpdateGrid(_gridHeight, _bottomLeftGridPosition);
        }

        private void CalculateGridValues()
        {
            _gridHeight = _uiManager.ScaleFloat(_rectTransform.sizeDelta.y) - 2 * _scaledPadding;
            _bottomLeftGridPosition = (Vector2)transform.position +
                                      new Vector2(-_gridHeight * 0.5f, -_gridHeight * 0.5f);
        }

        private void ScaleChartToFitScreen(float availableHorizontalSpace)
        {
            var scaledHeight = _uiManager.ScaleFloat(_rectTransform.sizeDelta.y + 2 * yMargin);
            _scaleFactor = Screen.height < scaledHeight ? Screen.height / scaledHeight : 1f;
            
            _rectTransform.sizeDelta *= _scaleFactor;
            _defaultSize = _rectTransform.sizeDelta;
            transform.position = new Vector2(availableHorizontalSpace * 0.5f, transform.position.y);
            _movementBoundaries = new MovementBoundaries(transform.position);
            StateUIElement.StateSizeAttributes.SetDefaults(
                _defaultStateSize * _scaleFactor,
                _defaultLineWidth * _scaleFactor);
        }

        public void ZoomChart(float zoomFactor, float zoomDelta, Vector2 zoomCenter)
        {
            _zoomFactor = zoomFactor;
            _rectTransform.sizeDelta = _defaultSize * _zoomFactor;
            _scaledPadding = _uiManager.ScaleFloat(padding) * _scaleFactor * _zoomFactor; 
            _movementBoundaries.SetBoundaries((_rectTransform.sizeDelta.x - _defaultSize.x)*0.5f);
            var panelPosition = (Vector2)_rectTransform.position;
            var zoomCenterDifference = zoomCenter - panelPosition;
            var newPosition = panelPosition - zoomCenterDifference * zoomDelta;
            _rectTransform.position = _movementBoundaries.ClampVector2(newPosition);
            CalculateGridValues();
            StateUIElement.StateSizeAttributes.SetScaling(zoomFactor);
            _stateChartUIGrid.UpdateGrid(_gridHeight,_bottomLeftGridPosition);
        }

        public struct MovementBoundaries
        {
            private Vector2 _center;
            private float _xMin, _yMin, _xMax, _yMax;

            public MovementBoundaries(Vector2 center)
            {
                _center = center;
                _xMin = _center.x;
                _yMin = _center.y;
                _xMax = _center.x;
                _yMax = _center.y;
            }

            public void SetBoundaries(float boundarySize)
            {
                _xMin = _center.x - boundarySize;
                _yMin = _center.y - boundarySize;
                _xMax = _center.x + boundarySize;
                _yMax = _center.y + boundarySize;
            }

            public Vector2 ClampVector2(Vector2 vector)
            {
                return new Vector2(Mathf.Clamp(vector.x, _xMin, _xMax), Mathf.Clamp(vector.y, _yMin, _yMax));
            }
        }
    }
}