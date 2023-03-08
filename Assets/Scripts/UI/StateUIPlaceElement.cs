using Helper;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class StateUIPlaceElement : MonoBehaviour, IPointerDownHandler
    {
        public enum Mode
        {
            IsInSelectionUnavailable,
            IsInSelectionAvailable,
            IsBeingDragged,
            IsPlaced
        }

        [SerializeField] private GameObject transitionLineBlocker;
        [SerializeField] private float placeAreaSize;

        private Mode _currentMode;
        private Vector3 _dragOffset = new (0f, 75f, 2f);
        private StateUIData _data;
        private StateChartUIManager _uiManager;
        private StateUIElement _uiElement;
        private bool _canBePlaced;
        private float _scaledPlaceAreaSize;

        public void Initialize(StateUIData stateUIData)
        {
            _uiElement = GetComponent<StateUIElement>();
            _uiElement.AssignedId = -1;
            SetToUnavailable();
            _uiManager = GameManager.Instance.GetStateChartUIManager();
            _scaledPlaceAreaSize = _uiManager.DownscaleFloat(placeAreaSize);
            _dragOffset = new Vector3(0f, _uiManager.DownscaleFloat(_dragOffset.y), 2f);
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
            switch (_currentMode)
            {
                case Mode.IsPlaced:
                    _uiManager.HandleStatePlaceElementClicked(this, true);
                    RemoveAllTransitionPlugs();
                    break;
                case Mode.IsInSelectionAvailable:
                    _uiManager.HandleStatePlaceElementClicked(this, false);
                    break;
                case Mode.IsInSelectionUnavailable:
                    return;
            }

            SetColorsToDisabled();
            _canBePlaced = false;
            transitionLineBlocker.SetActive(false);
            _currentMode = Mode.IsBeingDragged;
        }
        
        private void Update()
        {
            if (_currentMode == Mode.IsBeingDragged)
            {
                transform.position = Input.mousePosition + _dragOffset;
                CheckIfCanBePlaced();
                if (Input.GetMouseButtonUp(0))
                {
                    _uiManager.HandleStatePlaceElementReleased(_data, _canBePlaced);
                    if (_canBePlaced)
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

        private void CheckIfCanBePlaced()
        {
            if (HelperFunctions.CheckIfMouseAreaIsOverFreeAreaWithTag(_scaledPlaceAreaSize, "StateChartDropZone", "Blocker"))
            {
                if(!_canBePlaced)
                    SetColorsToDefault();
                _canBePlaced = true;
            }
            else
            {
                if(_canBePlaced)
                    SetColorsToDisabled();
                _canBePlaced = false;
            }
            Debug.Log($"Can be placed: {_canBePlaced}, size {_scaledPlaceAreaSize}");
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
        
        public void PlaceState()
        {
            _currentMode = Mode.IsPlaced;
            var placePosition = transform.position;
            placePosition.z= 1f;
            transform.position = placePosition;
            transitionLineBlocker.SetActive(true);
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
