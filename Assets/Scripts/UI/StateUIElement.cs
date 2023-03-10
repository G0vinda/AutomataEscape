using System;
using System.Collections.Generic;
using Helper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StateUIElement : MonoBehaviour
    {
        [SerializeField] public TransitionPlug defaultTransitionPlugPrefab;
        [SerializeField] public RectTransform[] transitionSlotTransforms;
        [SerializeField] public Image image;
        [SerializeField] public TextMeshProUGUI textElement;
        [SerializeField] public float slotAreaWidth;
        [SerializeField] public float slotAreaHeight;
        [SerializeField] public float stateBufferSpace;

        public int AssignedId { get; set; }
        
        [HideInInspector]
        public List<int> emptySlotIds;
        [HideInInspector]
        public TransitionPlug[] connectedTransitionPlugs;

        private float _scaledSlotAreaWidth;
        private float _scaledSlotAreaHeight;

        private RectTransform _rectTransform;
        private Vector2 _defaultSize;
        private Vector3 _defaultImageScale;

        private void Start()
        {
            var uiManager = GameManager.Instance.GetUIManager();
            _scaledSlotAreaWidth = uiManager.ScaleFloat(slotAreaWidth);
            _scaledSlotAreaHeight = uiManager.ScaleFloat(slotAreaHeight);
        }

        public void Initialize(float scaleFactor, int assignedId)
        {
            _rectTransform = GetComponent<RectTransform>();
            _defaultSize = _rectTransform.sizeDelta * scaleFactor;
            Debug.Log($"Default size is: {GameManager.Instance.GetUIManager().ScaleFloat(_defaultSize.x)}");
            _rectTransform.sizeDelta = _defaultSize; 
            _defaultImageScale = image.rectTransform.localScale * scaleFactor;
            //Debug.Log($"Default imageSize is: {GameManager.Instance.GetUIManager().ScaleFloat(_defaultSize.x)}");
            image.rectTransform.localScale = _defaultImageScale;

            AssignedId = assignedId;
        }

        public void SetSizeToDefault() 
        {
            _rectTransform.sizeDelta = _defaultSize;
            image.rectTransform.localScale = _defaultImageScale;
        }

        public void SetSizeToCellSize(float zoomFactor)
        {
            _rectTransform.sizeDelta = _defaultSize * zoomFactor;
            image.rectTransform.localScale = _defaultImageScale * zoomFactor;
        }
        
        public void SetupEmptySlots()
        {
            connectedTransitionPlugs = new TransitionPlug[12];
            emptySlotIds = new List<int>();
            for (int i = 0; i < 12; i++)
            {
                emptySlotIds.Add(i);    
            }
        }

        public void SetText(string text)
        {
            textElement.text = text;
        }

        public void SetImageColor(Color color)
        {
            image.color = color;
        }

        public void AddDefaultTransitionPlugToState()
        {
            var defaultSlotId = transitionSlotTransforms.Length - 1;
            var defaultPlug = InstantiateTransitionPlug(transitionSlotTransforms[defaultSlotId].position,
                transitionSlotTransforms[defaultSlotId].rotation, defaultSlotId);
            defaultPlug.GetLineTransform().rotation = Quaternion.identity;
            emptySlotIds.Remove(defaultSlotId);
        }

        public TransitionPlug InstantiateTransitionPlug(Vector3 position, Quaternion rotation, int slotId)
        {
            var newPlug = Instantiate(defaultTransitionPlugPrefab, position, rotation, transform);
            connectedTransitionPlugs[slotId] = newPlug;
            return newPlug;
        }
        
        public void MoveTransitionPlugToSlot(TransitionPlug plug, int newSlotId)
        {
            var plugTransform = plug.GetComponent<RectTransform>();
            plugTransform.position = transitionSlotTransforms[newSlotId].position;
            plugTransform.rotation = transitionSlotTransforms[newSlotId].rotation;
            plug.GetLineTransform().rotation = Quaternion.identity;

            RemoveTransitionPlugFromSlot(plug);
            connectedTransitionPlugs[newSlotId] = plug;
            emptySlotIds.Remove(newSlotId);
        }

        public void RemoveTransitionPlugFromSlot(TransitionPlug plug)
        {
            var oldSlotId = Array.IndexOf(connectedTransitionPlugs, plug);
            connectedTransitionPlugs[oldSlotId] = null;
            emptySlotIds.Add(oldSlotId);
        }

        public int IsPositionInRangeOfEmptySlot(Vector3 pos)
        {
            foreach (var emptySlotId in emptySlotIds)
            {
                var emptySlotTransform = transitionSlotTransforms[emptySlotId];
                var emptySlotPos = emptySlotTransform.position;
                if (Vector3.Distance(pos, emptySlotPos) > slotAreaWidth + slotAreaHeight)
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
                    xMin = emptySlotPos.x - _scaledSlotAreaHeight;
                    xMax = emptySlotPos.x;
                    yMin = emptySlotPos.y - _scaledSlotAreaWidth * 0.5f;
                    yMax = emptySlotPos.y + _scaledSlotAreaWidth * 0.5f;
                } else if (emptySlotDir.Equals(Vector2.right))
                {
                    xMin = emptySlotPos.x;
                    xMax = emptySlotPos.x + _scaledSlotAreaHeight;
                    yMin = emptySlotPos.y - _scaledSlotAreaWidth * 0.5f;
                    yMax = emptySlotPos.y + _scaledSlotAreaWidth * 0.5f;
                } else if (emptySlotDir.Equals(Vector2.down))
                {
                    xMin = emptySlotPos.x - _scaledSlotAreaWidth * 0.5f;
                    xMax = emptySlotPos.x + _scaledSlotAreaWidth * 0.5f;
                    yMin = emptySlotPos.y - _scaledSlotAreaHeight;
                    yMax = emptySlotPos.y;
                } else // Vector2.up
                {
                    xMin = emptySlotPos.x - _scaledSlotAreaWidth * 0.5f;
                    xMax = emptySlotPos.x + _scaledSlotAreaWidth * 0.5f;
                    yMin = emptySlotPos.y;
                    yMax = emptySlotPos.y + _scaledSlotAreaHeight;;
                }

                if (IsPosInSquare(pos, xMin, yMin, xMax, yMax))
                    return emptySlotId;
            }

            return -1;
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

        private void OnDrawGizmos()
        {
            // Draw top slotAreas
            Gizmos.color = Color.red;
            for (var i = 0; i < 3; i++)
            {
                var slotPos = transitionSlotTransforms[i].position;
                Gizmos.DrawLine(
                    slotPos + new Vector3(-_scaledSlotAreaWidth * 0.5f, 0, 0),
                    slotPos + new Vector3(-_scaledSlotAreaWidth * 0.5f, _scaledSlotAreaHeight, 0));
                Gizmos.DrawLine(
                    slotPos + new Vector3(-_scaledSlotAreaWidth * 0.5f, _scaledSlotAreaHeight, 0),
                    slotPos + new Vector3(_scaledSlotAreaWidth * 0.5f, _scaledSlotAreaHeight, 0));
                Gizmos.DrawLine(
                    slotPos + new Vector3(_scaledSlotAreaWidth * 0.5f, _scaledSlotAreaHeight, 0),
                    slotPos + new Vector3(_scaledSlotAreaWidth * 0.5f, 0, 0));
            }
            
            // Draw bottom slotAreas
            Gizmos.color = Color.red;
            for (var i = 6; i < 9; i++)
            {
                var slotPos = transitionSlotTransforms[i].position;
                Gizmos.DrawLine(
                    slotPos + new Vector3(-_scaledSlotAreaWidth * 0.5f, 0, 0),
                    slotPos + new Vector3(-_scaledSlotAreaWidth * 0.5f, -_scaledSlotAreaHeight, 0));
                Gizmos.DrawLine(
                    slotPos + new Vector3(-_scaledSlotAreaWidth * 0.5f, -_scaledSlotAreaHeight, 0),
                    slotPos + new Vector3(_scaledSlotAreaWidth * 0.5f, -_scaledSlotAreaHeight, 0));
                Gizmos.DrawLine(
                    slotPos + new Vector3(_scaledSlotAreaWidth * 0.5f, -_scaledSlotAreaHeight, 0),
                    slotPos + new Vector3(_scaledSlotAreaWidth * 0.5f, 0, 0));
            }
        }
    }
}
