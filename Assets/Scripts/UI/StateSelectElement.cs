using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class StateSelectElement : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private StateUIData data;
        [SerializeField] private TextMeshProUGUI textElement;
        public StateChartManager.StateAction Action { get; private set; }

        private StateChartUIManager _uiManager;

        private void Awake()
        {
            var image = GetComponent<Image>();
            
            textElement.text = data.text;
            //image.sprite = data.image;
            image.color = data.color;
            Action = data.action;
        }

        private void Start()
        {
            _uiManager = FindObjectOfType<StateChartUIManager>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _uiManager.HandleStateSelectElementClicked(data);
        }
    }
}
