using UnityEngine;

namespace UI
{
    public class StateChartUIScaler : MonoBehaviour
    {
        [SerializeField] private float yPadding;
        private RectTransform _rectTransform;
        private bool _chartIsScaled;
        private float _scaleFactor;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public float ScaleChart()
        {
            if (!_chartIsScaled)
            {
                _chartIsScaled = true;
                var scaledHeight =
                    GameManager.Instance.GetStateChartUIManager().DownscaleFloat(_rectTransform.sizeDelta.y + 2 * yPadding);
                
                _scaleFactor = Screen.height < scaledHeight ? Screen.height / scaledHeight : 1f;
                _rectTransform.sizeDelta *= _scaleFactor;
            }
            
            return _scaleFactor;
        }
    }
}
