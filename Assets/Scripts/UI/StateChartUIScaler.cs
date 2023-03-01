using UnityEngine;

namespace UI
{
    public class StateChartScaler : MonoBehaviour
    {
        [SerializeField] private float yPadding;
        private RectTransform _rectTransform;
        private bool _chartIsScaled;
        private float _scaleFactor;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private float ScaleChart()
        {
            if (!_chartIsScaled)
            {
                _chartIsScaled = true;
                var scaledHeight =
                    GameManager.Instance.GetStateChartUIManager().DownscaleFloat(_rectTransform.sizeDelta.y + 2 * yPadding);

                if (Screen.height < scaledHeight)
                {
                    _scaleFactor = Screen.height / scaledHeight;
                    _rectTransform.sizeDelta *= _scaleFactor;
                }
            }

            return _scaleFactor;
        }
    }
}
