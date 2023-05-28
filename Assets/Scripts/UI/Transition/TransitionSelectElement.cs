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
        
        public StateChartManager.TransitionCondition Condition { get; private set; }

        private void Awake()
        {
            background.color = data.color;
            if (data.icon != null)
            {
                icon.sprite = data.icon;   
            }
            else
            {
                Destroy(icon);
            }

            Condition = data.condition;
        }

        public Color GetColor()
        {
            return background.color;
        }

        public void ShowSelectionMarking()
        {
            selectionMarking.gameObject.SetActive(true);
        }
        
        public void HideSelectionMarking()
        {
            selectionMarking.gameObject.SetActive(false);
        }
    }
}