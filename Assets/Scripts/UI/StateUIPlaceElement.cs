using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class StateUIPlaceElement : MonoBehaviour, IPointerDownHandler
    {
        private enum Mode
        {
            IsInSelectionUnavailable,
            IsInSelectionAvailable,
            IsBeingDragged,
            IsPlaced
        }
        
        private Mode _currentMode;
        private StateUIData _data;
        private StateUIElement _uiElement;

        public void Initialize(StateUIData stateUIData)
        {
            _uiElement = GetComponent<StateUIElement>();
            _uiElement.Initialize(-1);
            _data = stateUIData;
            _uiElement.SetText(_data.text);
            _uiElement.SetImageColor(_data.color);
            SetToUnavailable();
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

        public void SetToUnavailable()
        {
            _currentMode = Mode.IsInSelectionUnavailable;
        }
        
        public void SetToAvailable()
        {
            _currentMode = Mode.IsInSelectionAvailable;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if(_currentMode == Mode.IsInSelectionUnavailable)
                return;
            
            _currentMode = Mode.IsBeingDragged;
        }

        private void SetColorsToDisabled()
        {
            var imageColor = _uiElement.image.color;
            var textColor = _uiElement.textElement.color;
            imageColor.a = 0.6f;
            textColor.a = 0.6f;
            _uiElement.image.color = imageColor;
            _uiElement.textElement.color = textColor;
        }

        private void SetColorsToDefault()
        {
            var imageColor = _uiElement.image.color;
            var textColor = _uiElement.textElement.color;
            imageColor.a = 1;
            textColor.a = 1;
            _uiElement.image.color = imageColor;
            _uiElement.textElement.color = textColor;
        }
        
        public void PlaceState(StateChartCell cellToPlaceOn, Transform newParent)
        {
            transform.SetParent(newParent);
            _uiElement.ConnectedCell = cellToPlaceOn;
            //_uiElement.AddDefaultTransitionPlugToState();
        }

        public void SwitchAppearanceToOnGrid()
        {
            _uiElement.UpdateScaling();
            SetColorsToDefault();
        }

        public void SwitchAppearanceToOffGrid()
        {
            _uiElement.SetSizeToDefault();
            SetColorsToDisabled();
        }

        public void SetSizeToBig()
        {
            _uiElement.image.rectTransform.localScale = new Vector2(1.15f, 1.15f);
        }
    }
}
