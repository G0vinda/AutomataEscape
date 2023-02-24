using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StateGridLayout : LayoutGroup
    {
        [SerializeField] private Vector2 padding;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private RectTransform stateElementTransform;

        public float availableGroupWidth;
        public int maxColumns;
        public float stateSize;

        public override void SetLayoutHorizontal()
        {
            availableGroupWidth = rectTransform.sizeDelta.x - 2 * padding.x;
            stateSize = stateElementTransform.sizeDelta.x;
            maxColumns = Mathf.FloorToInt(availableGroupWidth / (stateSize + spacing.x));
            int maxRows = Mathf.CeilToInt(rectChildren.Count / (float)maxColumns);

            PositionChildren(maxColumns, maxRows, stateSize);
        }

        private void PositionChildren(int maxColumns, int maxRows, float stateSize)
        {
            float xPos = 0f;
            float yPos = 0f;
            
            int childCounter = 0;
            for (int y = 0; y < maxRows; y++)
            {
                yPos = y * (stateSize + spacing.y) + padding.y;
                for (int x = 0; x < maxColumns; x++)
                {
                    var item = rectChildren[childCounter];

                    xPos = x * (stateSize + spacing.x) + padding.x;

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
            
        }

        public override void SetLayoutVertical()
        {
            
        }
    }
}
