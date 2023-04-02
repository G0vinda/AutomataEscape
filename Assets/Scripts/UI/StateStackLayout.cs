using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StateStackLayout : LayoutGroup
    {
        [SerializeField] private Vector2 padding;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private RectTransform panelTransform;
        [SerializeField] private RectTransform availableStateStackTransform;

        private float _availablePanelWidth;
        private float _availableStateStackWidth;
        private float _availableStateStackHeight;

        public override void SetLayoutHorizontal()
        {
            _availablePanelWidth = panelTransform.sizeDelta.x - 2 * padding.x;
            _availableStateStackHeight = availableStateStackTransform.sizeDelta.y;
            _availableStateStackWidth = availableStateStackTransform.sizeDelta.x;
            var maxColumns = Mathf.FloorToInt(_availablePanelWidth / (_availableStateStackWidth + spacing.x));
            var maxRows = Mathf.CeilToInt(rectChildren.Count / (float)maxColumns);

            PositionChildren(maxColumns, maxRows);
        }

        private void PositionChildren(int maxColumns, int maxRows)
        {
            var childCounter = 0;
            for (var y = 0; y < maxRows; y++)
            {
                var yPos = y * (_availableStateStackHeight + spacing.y) + padding.y;
                for (var x = 0; x < maxColumns; x++)
                {
                    var item = rectChildren[childCounter];

                    var xPos = x * (_availableStateStackWidth + spacing.x) + padding.x;

                    SetChildAlongAxis(item, 0, xPos);
                    SetChildAlongAxis(item, 1, yPos);
                    childCounter++;

                    if (childCounter == rectChildren.Count)
                        return;
                }
            }
        }
        
        public override void CalculateLayoutInputVertical()
        {
            // Meant to be empty
        }

        public override void SetLayoutVertical()
        {
            // Meant to be empty
        }
    }
}
