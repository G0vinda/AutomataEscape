using System;
using Robot;
using UI.UIData;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace UI.Transition
{
    public class TransitionSelectElement : MonoBehaviour
    {
        [SerializeField] private TransitionUIData data;
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        [SerializeField] private Image selectionMarking;
        [SerializeField] private Image transitionLine;
        [SerializeField] private Transform containerTransform;
        
        public StateChartManager.TransitionCondition Condition { get; private set; }
        public static event Action<TransitionSelectElement> TransitionSelectElementEnabled;

        private void Awake()
        {
            background.color = data.color;
            transitionLine.color = data.color;
            if (data.icon != null)
            {
                icon.sprite = data.icon;   
            }
            else
            {
                Destroy(icon.gameObject);
            }

            Condition = data.condition;
        }

        private void OnEnable()
        {
            background.color = data.color;
            TransitionSelectElementEnabled?.Invoke(this);
        }

        public void ShowSelectionMarking()
        {
            selectionMarking.gameObject.SetActive(true);
            containerTransform.localPosition = new Vector3(100f, 0, 0);
        }
        
        public void HideSelectionMarking()
        {
            selectionMarking.gameObject.SetActive(false);
            containerTransform.localPosition = Vector3.zero;
        }
    }
}