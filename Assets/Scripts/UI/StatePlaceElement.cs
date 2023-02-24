using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Helper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class StatePlaceElement : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private TransitionPlug defaultTransitionPlugPrefab;
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI textElement;
        [SerializeField] private RectTransform[] transitionSlotTransforms;
        [SerializeField] private float maxSlotMouseAreaWidth;
        [SerializeField] private float maxSlotMouseAreaHeight;
        [SerializeField] private float stateBufferSpace;
        
        public int AssignedId { get; set; }

        private Vector3 _dragZOffset = new (0f, 0f, 2f);
        private List<int> _emptySlotIds;
        private TransitionPlug[] _connectedTransitionPlugs;
        private StateChartUIManager _uiManager;
        private bool _isBeingDragged;
        private StateUIData _data;
        public void Initialize(StateChartUIManager uiManager, StateUIData stateUIData)
        {
            AssignedId = -1;
            _uiManager = uiManager;
            _data = stateUIData;
            _connectedTransitionPlugs = new TransitionPlug[12];
            SetupEmptySlotIds();
            textElement.text = _data.text;
            image.color = _data.color;
            _isBeingDragged = true;
        }

        private void SetupEmptySlotIds()
        {
            _emptySlotIds = new List<int>();
            for (int i = 0; i < 12; i++)
            {
                _emptySlotIds.Add(i);    
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _uiManager.HandleStatePlaceElementClicked(this);
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
            _isBeingDragged = false;
            var placePosition = transform.position;
            placePosition.z= 1f;
            transform.position = placePosition;
            SetupEmptySlotIds();
            AddDefaultTransitionPlugToState();
        }

        private void AddDefaultTransitionPlugToState()
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

        public void MoveTransitionPlugToSlot(TransitionPlug plug, int newSlotId)
        {
            int oldSlotId = Array.IndexOf(_connectedTransitionPlugs, plug);
            
            var plugTransform = plug.GetComponent<RectTransform>();
            plugTransform.position = transitionSlotTransforms[newSlotId].position;
            plugTransform.rotation = transitionSlotTransforms[newSlotId].rotation;
            plug.GetLineTransform().rotation = Quaternion.identity;

            _connectedTransitionPlugs[oldSlotId] = null;
            _connectedTransitionPlugs[newSlotId] = plug;
            _emptySlotIds.Remove(newSlotId);
            _emptySlotIds.Add(oldSlotId);
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

        public bool IsPosInRangeOfState(Vector3 pos)
        {
            return Vector3.Distance(pos, transform.position) < stateBufferSpace;
        }

        public int IsPosInRangeOfEmptySlot(Vector3 pos)
        {
            foreach (var emptySlotId in _emptySlotIds)
            {
                var emptySlotTransform = transitionSlotTransforms[emptySlotId];
                var emptySlotPos = emptySlotTransform.position;
                if (Vector3.Distance(pos, emptySlotPos) > maxSlotMouseAreaWidth + maxSlotMouseAreaHeight)
                    continue;

                var emptySlotDir = emptySlotTransform.ZRotToDir();

                if (emptySlotDir.Equals(Vector2.zero))
                {
                    Debug.LogError("Couldn't read direction of empty slot.");
                    return -1;
                }

                float xMin, xMax, yMin, yMax;
                if (emptySlotDir.Equals(Vector2.left))
                {
                    xMin = emptySlotPos.x - maxSlotMouseAreaHeight;
                    xMax = emptySlotPos.x;
                    yMin = emptySlotPos.y - maxSlotMouseAreaWidth * 0.5f;
                    yMax = emptySlotPos.y + maxSlotMouseAreaWidth * 0.5f;
                } else if (emptySlotDir.Equals(Vector2.right))
                {
                    xMin = emptySlotPos.x;
                    xMax = emptySlotPos.x + maxSlotMouseAreaHeight;
                    yMin = emptySlotPos.y - maxSlotMouseAreaWidth * 0.5f;
                    yMax = emptySlotPos.y + maxSlotMouseAreaWidth * 0.5f;
                } else if (emptySlotDir.Equals(Vector2.down))
                {
                    xMin = emptySlotPos.x - maxSlotMouseAreaWidth * 0.5f;
                    xMax = emptySlotPos.x + maxSlotMouseAreaWidth * 0.5f;
                    yMin = emptySlotPos.y - maxSlotMouseAreaHeight;
                    yMax = emptySlotPos.y;
                } else // Vector2.up
                {
                    xMin = emptySlotPos.x - maxSlotMouseAreaWidth * 0.5f;
                    xMax = emptySlotPos.x + maxSlotMouseAreaWidth * 0.5f;
                    yMin = emptySlotPos.y;
                    yMax = emptySlotPos.y + maxSlotMouseAreaHeight;;
                }

                if (IsPosInSquare(pos, xMin, yMin, xMax, yMax))
                    return emptySlotId;
            }

            return -1;
        }

        public void SetSizeToBig()
        {
            image.rectTransform.localScale = new Vector2(1.15f, 1.15f);
        }

        public void SetSizeToDefault()
        {
            image.rectTransform.localScale = new Vector2(1f, 1f);
        }

        public Vector3 GetSlotPos(int slotId)
        {
            return transitionSlotTransforms[slotId].position;
        }

        public Vector2 GetSlotDir(int slotId)
        {
            return transitionSlotTransforms[slotId].ZRotToDir();
        }

        private bool IsPosInSquare(Vector3 pos, float xMin, float yMin, float xMax, float yMax)
        {
            var x = pos.x;
            var y = pos.y;
            if (x >= xMin && x <= xMax)
            {
                if (y >= yMin && y <= yMax)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetSlotToOccupied(int index)
        {
            _emptySlotIds.Remove(index);
        }

        public void SetSlotToEmpty(int index)
        {
            _emptySlotIds.Add(index);
        }
    }
}
