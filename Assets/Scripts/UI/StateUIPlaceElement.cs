using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class StateUIPlaceElement : MonoBehaviour
    {
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

        public void PlaceState(StateChartCell cellToPlaceOn, Transform newParent)
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
