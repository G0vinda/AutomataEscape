using Robot;
using UI.Grid;
using UI.UIData;
using UnityEngine;

namespace UI.State
{
    public class StateUIPlaceElement : MonoBehaviour
    {
        public bool Draggable { get; set; }
        
        private StateUIData _data;
        private StateUIElement _uiElement;

        public void Initialize(StateUIData stateUIData)
        {
            _uiElement = GetComponent<StateUIElement>();
            _uiElement.Initialize(-1);
            _data = stateUIData;
            _uiElement.image.sprite = _data.inactiveSprite;
        }

        public int GetAssignedId()
        {
            return _uiElement.AssignedId;
        }

        public StateChartManager.StateAction GetAction()
        {
            return _data.action;
        }

        public void SetAssignedId(int newId)
        {
            _uiElement.AssignedId = newId;
        }

        private void SetColorToTransparent()
        {
            var imageColor = _uiElement.image.color;
            imageColor.a = 0.6f;
            _uiElement.image.color = imageColor;
        }

        private void SetColorToDefault()
        {
            var imageColor = _uiElement.image.color;
            imageColor.a = 1;
            _uiElement.image.color = imageColor;
        }

        public void HighlightAsTransitionDestination()
        {
            _uiElement.SetSizeToHighlight();
        }

        public void RemoveHighlight()
        {
            _uiElement.UpdateScaling();
        }

        public void SetImageToActive(bool active)
        {
            _uiElement.image.sprite = active ? _data.sprite : _data.inactiveSprite;
        }

        public void PlaceOnCell(StateChartCell cellToPlaceOn, Transform newParent)
        {
            transform.SetParent(newParent);
            _uiElement.ConnectedCell = cellToPlaceOn;
        }

        public void SwitchAppearanceToOnGrid()
        {
            _uiElement.UpdateScaling();
            SetColorToDefault();
        }

        public void SwitchAppearanceToOffGrid()
        {
            _uiElement.SetSizeToDefault();
            SetColorToTransparent();
        }
    }
}
