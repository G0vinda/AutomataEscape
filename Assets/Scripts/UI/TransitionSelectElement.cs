using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class TransitionSelectElement : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private TransitionUIData data;
        
        public StateChartManager.TransitionCondition Condition { get; private set; }
        
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.color = data.color;
            Condition = data.condition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            GameManager.Instance.GetStateChartUIManager().HandleTransitionSelectElementClicked(data);
        }
    }
}