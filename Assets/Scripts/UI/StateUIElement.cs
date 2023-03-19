using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StateUIElement : MonoBehaviour
    {
        public struct SizeAttributes
        {
            public Vector2 StateSize { get; private set; }
            public float LineElementLength { get; private set; }
            public float FirstLineElementLength { get; private set;}
            public float LineWidth { get; private set;}
            public Vector2 DefaultStateSize { get; private set; }
            
            private float _defaultLineElementLength;
            private float _defaultFirstLineElementLength;
            private float _defaultLineWidth;

            public void SetDefaults(float sateSize, float lineWidth)
            {
                DefaultStateSize = sateSize * Vector2.one;
                _defaultLineElementLength = sateSize / 3;
                _defaultLineWidth = lineWidth;
                _defaultFirstLineElementLength = (_defaultLineElementLength + _defaultLineWidth) * 0.5f;
                SetScaling(1);
            }

            public void SetScaling(float scaleFactor)
            {
                StateSize = DefaultStateSize * scaleFactor;
                LineElementLength = _defaultLineElementLength * scaleFactor;
                FirstLineElementLength = _defaultFirstLineElementLength * scaleFactor;
                LineWidth = _defaultLineWidth * scaleFactor;
            }
        }
        
        [SerializeField] public TransitionPlug defaultTransitionPlugPrefab;
        [SerializeField] public Image image;
        [SerializeField] public TextMeshProUGUI textElement;
        [SerializeField] public float stateBufferSpace;
        [SerializeField] private TransitionLine transitionLinePrefab;
        
        public static SizeAttributes StateSizeAttributes;
        
        public int AssignedId { get; set; }
        public StateChartCell ConnectedCell { get; set; }
        
        [HideInInspector]
        public List<int> emptySlotIds;
        [HideInInspector]
        public TransitionPlug[] connectedTransitionPlugs;
        
        private RectTransform _imageTransform;
        private List<TransitionLine> _outgoingTransitionLines = new ();

        public void Initialize(int assignedId)
        {
            _imageTransform = image.GetComponent<RectTransform>();
            _imageTransform.sizeDelta = StateSizeAttributes.StateSize;

            AssignedId = assignedId;
        }

        public void SetSizeToDefault() 
        {
            _imageTransform.sizeDelta = StateSizeAttributes.DefaultStateSize;
        }

        public void UpdateScaling()
        {
            var scaleDelta = StateSizeAttributes.StateSize.x / _imageTransform.sizeDelta.x;
            UpdateTransitionLines(scaleDelta);
            _imageTransform.sizeDelta = StateSizeAttributes.StateSize;
        }
        
        private void UpdateTransitionLines(float scaleDelta)
        {
            foreach (var outgoingTransitionLine in _outgoingTransitionLines)
            {
                //Calculate new position
                var positionDelta = outgoingTransitionLine.transform.position - transform.position;
                positionDelta *= scaleDelta;
                outgoingTransitionLine.transform.position = transform.position + positionDelta;//new Vector3(StateSizeAttributes.StateSize.x * 0.5f, 0, 0);
                
                outgoingTransitionLine.UpdateSize(
                    StateSizeAttributes.FirstLineElementLength, 
                    StateSizeAttributes.LineElementLength, 
                    StateSizeAttributes.LineWidth);
            }
        }
        
        public void SetupEmptySlots()
        {
            connectedTransitionPlugs = new TransitionPlug[12];
            emptySlotIds = new List<int>();
            for (var i = 0; i < 12; i++)
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

        public TransitionLine DrawFirstTransitionLine(Vector2 position, Direction direction, Color lineColor)
        {
            var newTransitionLine = Instantiate(transitionLinePrefab, position, Quaternion.identity, transform);
            newTransitionLine.Initialize(
                StateSizeAttributes.FirstLineElementLength,
                StateSizeAttributes.LineElementLength, 
                StateSizeAttributes.LineWidth, 
                lineColor,
                direction);
            _outgoingTransitionLines.Add(newTransitionLine);

            return newTransitionLine;
        }

        public TransitionPlug InstantiateTransitionPlug(Vector3 position, Quaternion rotation, int slotId)
        {
            var newPlug = Instantiate(defaultTransitionPlugPrefab, position, rotation, transform);
            connectedTransitionPlugs[slotId] = newPlug;
            return newPlug;
        }
        
        public void MoveTransitionPlugToSlot(TransitionPlug plug, int newSlotId)
        {
            // var plugTransform = plug.GetComponent<RectTransform>();
            // plugTransform.position = transitionSlotTransforms[newSlotId].position;
            // plugTransform.rotation = transitionSlotTransforms[newSlotId].rotation;
            // plug.GetLineTransform().rotation = Quaternion.identity;
            //
            // RemoveTransitionPlugFromSlot(plug);
            // connectedTransitionPlugs[newSlotId] = plug;
            // emptySlotIds.Remove(newSlotId);
        }

        public void RemoveTransitionPlugFromSlot(TransitionPlug plug)
        {
            var oldSlotId = Array.IndexOf(connectedTransitionPlugs, plug);
            connectedTransitionPlugs[oldSlotId] = null;
            emptySlotIds.Add(oldSlotId);
        }

        public int IsPositionInRangeOfEmptySlot(Vector3 pos)
        {
            // foreach (var emptySlotId in emptySlotIds)
            // {
            //     var emptySlotTransform = transitionSlotTransforms[emptySlotId];
            //     var emptySlotPos = emptySlotTransform.position;
            //     if (Vector3.Distance(pos, emptySlotPos) > slotAreaWidth + slotAreaHeight)
            //         continue;
            //
            //     var emptySlotDir = emptySlotTransform.ZRotToDir();
            //
            //     if (emptySlotDir.Equals(Vector2.zero))
            //     {
            //         Debug.LogError("Couldn't read direction of empty slot.");
            //         return -1;
            //     }
            //
            //     float xMin, xMax, yMin, yMax;
            //     if (emptySlotDir.Equals(Vector2.left))
            //     {
            //         xMin = emptySlotPos.x - _scaledSlotAreaHeight;
            //         xMax = emptySlotPos.x;
            //         yMin = emptySlotPos.y - _scaledSlotAreaWidth * 0.5f;
            //         yMax = emptySlotPos.y + _scaledSlotAreaWidth * 0.5f;
            //     } else if (emptySlotDir.Equals(Vector2.right))
            //     {
            //         xMin = emptySlotPos.x;
            //         xMax = emptySlotPos.x + _scaledSlotAreaHeight;
            //         yMin = emptySlotPos.y - _scaledSlotAreaWidth * 0.5f;
            //         yMax = emptySlotPos.y + _scaledSlotAreaWidth * 0.5f;
            //     } else if (emptySlotDir.Equals(Vector2.down))
            //     {
            //         xMin = emptySlotPos.x - _scaledSlotAreaWidth * 0.5f;
            //         xMax = emptySlotPos.x + _scaledSlotAreaWidth * 0.5f;
            //         yMin = emptySlotPos.y - _scaledSlotAreaHeight;
            //         yMax = emptySlotPos.y;
            //     } else // Vector2.up
            //     {
            //         xMin = emptySlotPos.x - _scaledSlotAreaWidth * 0.5f;
            //         xMax = emptySlotPos.x + _scaledSlotAreaWidth * 0.5f;
            //         yMin = emptySlotPos.y;
            //         yMax = emptySlotPos.y + _scaledSlotAreaHeight;;
            //     }
            //
            //     if (IsPosInSquare(pos, xMin, yMin, xMax, yMax))
            //         return emptySlotId;
            // }

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
    }
}
