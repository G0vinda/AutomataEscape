using System.Collections.Generic;
using System.Linq;
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
        
        [SerializeField] public Image image;
        [SerializeField] public TextMeshProUGUI textElement;
        [SerializeField] public float stateBufferSpace;
        [SerializeField] private TransitionLine transitionLinePrefab;
        
        public static SizeAttributes StateSizeAttributes;
        
        public int AssignedId { get; set; }
        public StateChartCell ConnectedCell { get; set; }

        private RectTransform _imageTransform;
        private List<TransitionLine> _outgoingTransitionLines = new ();

        public void Initialize(int assignedId)
        {
            _imageTransform = image.GetComponent<RectTransform>();
            _imageTransform.sizeDelta = StateSizeAttributes.DefaultStateSize;

            AssignedId = assignedId;
        }

        public int GetNumberOfOutgoingTransitions()
        {
            return _outgoingTransitionLines.Count;
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

        public void SetText(string text)
        {
            textElement.text = text;
        }

        public void SetImageColor(Color color)
        {
            image.color = color;
        }

        public TransitionLine DrawFirstTransitionLine(Vector2 position, Direction direction, Color lineColor, StateChartManager.TransitionCondition condition)
        {
            var newTransitionLine = Instantiate(transitionLinePrefab, position, Quaternion.identity, transform);
            newTransitionLine.Initialize(
                StateSizeAttributes.FirstLineElementLength,
                StateSizeAttributes.LineElementLength, 
                StateSizeAttributes.LineWidth, 
                lineColor,
                direction,
                condition);
            _outgoingTransitionLines.Add(newTransitionLine);

            return newTransitionLine;
        }

        public void RemoveTransitionByCondition(StateChartManager.TransitionCondition condition)
        {
            var transitionMatch = _outgoingTransitionLines.First(transition => transition.Condition == condition);
            RemoveTransition(transitionMatch);
        }

        public void RemoveTransition(TransitionLine transitionLine)
        {
            _outgoingTransitionLines.Remove(transitionLine);
            Destroy(transitionLine.gameObject);
        }
    }
}
