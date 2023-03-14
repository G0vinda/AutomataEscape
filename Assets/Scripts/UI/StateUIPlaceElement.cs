using Helper;
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

        private StateChartCell _connectedCell;
        private Mode _currentMode;
        private StateUIData _data;
        private StateUIElement _uiElement;

        public void Initialize(StateUIData stateUIData, float gridScaleFactor)
        {
            _uiElement = GetComponent<StateUIElement>();
            _uiElement.Initialize(gridScaleFactor, -1);
            _data = stateUIData;
            _uiElement.SetupEmptySlots();
            _uiElement.SetText(_data.text);
            _uiElement.SetImageColor(_data.color);
            SetToUnavailable();
        }

        public int GetAssignedId()
        {
            return _uiElement.AssignedId;
        }

        public StateChartCell GetConnectedCell()
        {
            return _connectedCell;
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
            _connectedCell = cellToPlaceOn;
            transform.SetParent(newParent);
            _uiElement.SetupEmptySlots();
            //_uiElement.AddDefaultTransitionPlugToState();
        }

        public int IsPositionInRangeOfEmptySlot(Vector3 position)
        {
            return _uiElement.IsPositionInRangeOfEmptySlot(position);
        }

        public void AddTransitionPlugToState(TransitionUIData data)
        {
            var transitionSlotTransforms = _uiElement.transitionSlotTransforms;
            var emptySlotIds = _uiElement.emptySlotIds;
            for (int i = transitionSlotTransforms.Length - 1; i >= 0; i--)
            {
                if (!emptySlotIds.Contains(i))
                    continue;

                var newPlug = _uiElement.InstantiateTransitionPlug(transitionSlotTransforms[i].position,
                    transitionSlotTransforms[i].rotation, i);
                
                newPlug.GetLineTransform().rotation = Quaternion.identity;
                emptySlotIds.Remove(i);
                
                newPlug.Initialize(data);
                
                return;
            }
        }
        
        private void RemoveAllTransitionPlugs()
        {
            foreach (var plug in _uiElement.connectedTransitionPlugs)
            {
                if (plug != null)
                {
                    _uiElement.RemoveTransitionPlugFromSlot(plug);
                    Destroy(plug.gameObject);
                }
            }
        }

        public void SwitchAppearanceToOnGrid(float zoomFactor)
        {
            _uiElement.SetSizeToCellSize(zoomFactor);
            SetColorsToDefault();
        }

        public void SwitchAppearanceToOffGrid()
        {
            _uiElement.SetSizeToDefault();
            SetColorsToDisabled();
        }

        public bool IsPositionInRangeOfState(Vector3 pos)
        {
            return Vector3.Distance(pos, transform.position) < _uiElement.stateBufferSpace;
        }
        
        public void SetSlotToOccupied(int index)
        {
            _uiElement.emptySlotIds.Remove(index);
        }

        public void SetSlotToEmpty(int index)
        {
            _uiElement.emptySlotIds.Add(index);
        }
        
        public Vector3 GetSlotPosition(int slotId)
        {
            return _uiElement.transitionSlotTransforms[slotId].position;
        }

        public Vector2 GetSlotDirection(int slotId)
        {
            return _uiElement.transitionSlotTransforms[slotId].ZRotToDir();
        }

        public void SetSizeToBig()
        {
            _uiElement.image.rectTransform.localScale = new Vector2(1.15f, 1.15f);
        }
    }
}
