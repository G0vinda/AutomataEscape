using Helper;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class StateUIPlaceElement : MonoBehaviour, IPointerDownHandler
    {
        public enum Mode
        {
            IsInSelection,
            IsBeingDragged,
            IsPlaced
        }

        private Mode _currentMode;
        private Vector3 _dragZOffset = new (0f, 0f, 2f);
        private StateUIData _data;
        private StateChartUIManager _uiManager;
        private StateUIElement _uiElement;

        public void Initialize(StateUIData stateUIData)
        {
            _uiElement = GetComponent<StateUIElement>();
            _uiElement.AssignedId = -1;
            _currentMode = Mode.IsInSelection;
            _uiManager = GameManager.Instance.GetStateChartUIManager();
            _data = stateUIData;
            _uiElement.SetupEmptySlots();
            _uiElement.SetText(_data.text);
            _uiElement.SetImageColor(_data.color);
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
        
        public void OnPointerDown(PointerEventData eventData)
        {
            switch (_currentMode)
            {
                case Mode.IsPlaced:
                    _uiManager.HandleStatePlaceElementClicked(this, true);
                    RemoveAllTransitionPlugs();
                    break;
                case Mode.IsInSelection:
                    _uiManager.HandleStatePlaceElementClicked(this, false);
                    break;
            }

            _currentMode = Mode.IsBeingDragged;
        }
        
        private void Update()
        {
            if (_currentMode == Mode.IsBeingDragged)
            {
                transform.position = Input.mousePosition + _dragZOffset;
                if (Input.GetMouseButtonUp(0))
                {
                    if (_uiManager.HandleStatePlaceElementReleased(_data))
                    {
                        PlaceState();
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
        
        public void PlaceState()
        {
            _currentMode = Mode.IsPlaced;
            var placePosition = transform.position;
            placePosition.z= 1f;
            transform.position = placePosition;
            _uiElement.SetupEmptySlots();
            _uiElement.AddDefaultTransitionPlugToState();
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

        public void SetSizeToDefault() 
        { 
            _uiElement.image.rectTransform.localScale = new Vector2(1f, 1f);
        }
    }
}
