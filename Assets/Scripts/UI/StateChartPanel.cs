using UI.Grid;
using UI.State;
using UnityEngine;

namespace UI
{
    public class StateChartPanel : MonoBehaviour
    {
        [SerializeField] private float yMargin;
        [SerializeField] private float padding;

        private UIManager _uiManager;
        private RectTransform _rectTransform;
        private UIGridManager _uiGridManager;
        
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
            _uiGridManager = GetComponent<UIGridManager>();
            _uiManager = GameManager.Instance.GetUIManager();
            
            _zoomFactor = 1f;
            ScaleChartToFitScreen(availableHorizontalSpace);
            _scaledPadding = _uiManager.ScaleFloat(padding) * _scaleFactor;
            CalculateGridValues();
            _uiGridManager.Initialize(_gridHeight, _bottomLeftGridPosition);
        }
        
        public float GetScaleFactor()
        {
            return _scaleFactor;
        }

        public void MoveByVector(Vector2 moveVector)
        {
            var newPosition = (Vector2)transform.position + moveVector;
            transform.position = _movementBoundaries.ClampVector2(newPosition);
            CalculateGridValues();
            _uiGridManager.UpdateGrid(_gridHeight, _bottomLeftGridPosition);
        }

        private void CalculateGridValues()
        {
            _gridHeight = _uiManager.ScaleFloat(_rectTransform.sizeDelta.y) - 2 * _scaledPadding;
            _bottomLeftGridPosition = (Vector2)transform.position + Vector2.one * -_gridHeight * 0.5f;
        }

        private void ScaleChartToFitScreen(float availableHorizontalSpace)
        {
            var scaledHeight = _uiManager.ScaleFloat(_rectTransform.sizeDelta.y + 2 * yMargin);
            _scaleFactor = Screen.height < scaledHeight ? Screen.height / scaledHeight : 1f; // If the screen height isn't big enough _scaleFactor will be < 0
            
            _rectTransform.sizeDelta *= _scaleFactor;
            _defaultSize = _rectTransform.sizeDelta;
            transform.position = new Vector2(availableHorizontalSpace * 0.5f, transform.position.y); // Position panel at center in available space
            _movementBoundaries = new MovementBoundaries(transform.position);
            
            // The StateUIElement size has to scale with uiGrid
            StateUIElement.StateSizeAttributes.SetDefaults(
                _defaultStateSize * _scaleFactor,
                _defaultLineWidth * _scaleFactor);
        }

        public void ZoomChart(float zoomFactor, float zoomDelta, Vector2 zoomCenter)
        {
            _zoomFactor = zoomFactor;
            _rectTransform.sizeDelta = _defaultSize * _zoomFactor;
            _scaledPadding = _uiManager.ScaleFloat(padding) * _scaleFactor * _zoomFactor; 
            
            var panelPosition = (Vector2)_rectTransform.position;
            var zoomCenterDifference = zoomCenter - panelPosition;
            var newPosition = panelPosition - zoomCenterDifference * zoomDelta;
            
            _movementBoundaries.SetBoundaries((_rectTransform.sizeDelta.x - _defaultSize.x)*0.5f);
            _rectTransform.position = _movementBoundaries.ClampVector2(newPosition);
            
            CalculateGridValues();
            StateUIElement.StateSizeAttributes.SetScaling(zoomFactor);
            _uiGridManager.UpdateGrid(_gridHeight,_bottomLeftGridPosition);
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