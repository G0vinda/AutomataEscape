using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.State
{
    public class StateStackLayout : LayoutGroup
    {
        [SerializeField] private Vector2 padding;
        [SerializeField] private float minXSpacing;
        [SerializeField] private RectTransform panelTransform;

        private float _availablePanelWidth;
        private float _stackWidth;
        private float _stackHeight;
        private Vector2 _spacing;

        public override void SetLayoutHorizontal()
        {
            RectTransform stackTransform = null;
            var stackCount = rectChildren.Count;
            for (var i = 0; i < stackCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    stackTransform = (RectTransform)transform.GetChild(i);
            }
            
            if(stackTransform == null)
                return;
            
            _availablePanelWidth = panelTransform.sizeDelta.x - 2 * padding.x;
            _stackHeight = stackTransform.sizeDelta.y;
            _stackWidth = stackTransform.sizeDelta.x;
            var rowCount = _availablePanelWidth >= stackCount * _stackWidth + minXSpacing * (stackCount - 1) ? 1 : 2;
            var columnCount = Mathf.CeilToInt((float)stackCount / rowCount);

            _spacing.x = columnCount > 1
                ? (_availablePanelWidth - columnCount * _stackWidth) / (columnCount - 1)
                : 0;

            PositionChildren(columnCount, rowCount);
        }

        private void PositionChildren(int maxColumns, int maxRows)
        {
            var childCounter = 0;
            for (var y = 0; y < maxRows; y++)
            {
                var yPos = y * (_stackHeight + _spacing.y) + padding.y;
                for (var x = 0; x < maxColumns; x++)
                {
                    var item = rectChildren[childCounter];

                    var xPos = x * (_stackWidth + _spacing.x) + padding.x;

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
