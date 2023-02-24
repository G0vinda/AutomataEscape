using UnityEngine;

namespace UI
{
    public class StateChartScaler : MonoBehaviour
    {
        [SerializeField] private float yPadding;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        void Start()
        {
            var scaledHeight =
                StateChartUIManager.Instance.DownscaleFloat(_rectTransform.sizeDelta.y + 2 * yPadding);

            if (Screen.height < scaledHeight)
            {
                var newScaleFactor = Screen.height / scaledHeight;
                _rectTransform.sizeDelta *= newScaleFactor;
            }
        }
    }
}
