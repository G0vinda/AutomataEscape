using System;
using Helper;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class StateUIPlaceElement : StateUIElement, IPointerDownHandler
    {
        [SerializeField] protected Image image;
        [SerializeField] protected TextMeshProUGUI textElement;
        [SerializeField] private float stateBufferSpace;

        private Vector3 _dragZOffset = new (0f, 0f, 2f);
        private TransitionPlug[] _connectedTransitionPlugs;
        private bool _isBeingDragged;
        private StateUIData _data;
        
        public void Initialize(StateUIData stateUIData)
        {
            AssignedId = -1;
            UIManager = GameManager.Instance.GetStateChartUIManager();
            _data = stateUIData;
            _connectedTransitionPlugs = new TransitionPlug[12];
            SetupEmptySlotIds();
            textElement.text = _data.text;
            image.color = _data.color;
            _isBeingDragged = true;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            UIManager.HandleStatePlaceElementClicked(this);
            RemoveAllTransitionPlugs();
            _isBeingDragged = true;
        }
        
        private void Update()
        {
            if (_isBeingDragged)
            {
                transform.position = Input.mousePosition + _dragZOffset;
                if (Input.GetMouseButtonUp(0))
                {
                    if (UIManager.HandleStatePlaceElementReleased(_data))
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
            _isBeingDragged = false;
            var placePosition = transform.position;
            placePosition.z= 1f;
            transform.position = placePosition;
            SetupEmptySlotIds();
            AddDefaultTransitionPlugToState();
        }

        protected override void AddDefaultTransitionPlugToState()
        {
            AddTransitionPlugToState(null);
        }
        
        public void AddTransitionPlugToState(TransitionUIData data)
        {
            for (int i = transitionSlotTransforms.Length - 1; i >= 0; i--)
            {
                if (!_emptySlotIds.Contains(i))
                    continue;
                
                var newPlug = Instantiate(
                    defaultTransitionPlugPrefab, 
                    transitionSlotTransforms[i].position,
                    transitionSlotTransforms[i].rotation,
                    transform);
                _connectedTransitionPlugs[i] = newPlug.GetComponent<TransitionPlug>();
                newPlug.GetLineTransform().rotation = Quaternion.identity;
                _emptySlotIds.Remove(i);

                if (data != null)
                {
                    newPlug.Initialize(data);
                }
                
                return;
            }
        }
        
        private void RemoveAllTransitionPlugs()
        {
            foreach (var plug in _connectedTransitionPlugs)
            {
                if(plug != null)
                    RemoveTransitionPlug(plug);
            }
        }
        
        public void RemoveTransitionPlug(TransitionPlug plug)
        {
            int index = Array.IndexOf(_connectedTransitionPlugs, plug);
            Destroy(plug.gameObject);
            _connectedTransitionPlugs[index] = null;
            _emptySlotIds.Add(index);
        }
        
        public bool IsPositionInRangeOfState(Vector3 pos)
        {
            return Vector3.Distance(pos, transform.position) < stateBufferSpace;
        }
        
        public void SetSlotToOccupied(int index)
        {
            _emptySlotIds.Remove(index);
        }

        public void SetSlotToEmpty(int index)
        {
            _emptySlotIds.Add(index);
        }
        
        public Vector3 GetSlotPosition(int slotId)
        {
            return transitionSlotTransforms[slotId].position;
        }

        public Vector2 GetSlotDirection(int slotId)
        {
            return transitionSlotTransforms[slotId].ZRotToDir();
        }

        public void SetSizeToBig()
        {
            image.rectTransform.localScale = new Vector2(1.15f, 1.15f);
        }

        public void SetSizeToDefault() 
        { 
            image.rectTransform.localScale = new Vector2(1f, 1f);
        }
    }
}
