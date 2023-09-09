using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DG.Tweening;
using Robot;
using UI.Grid;
using UI.Transition;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace UI.State
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

        [SerializeField] private float connectionGrowEffectSize;
        [SerializeField] private float connectionGrowEffectTime;

        public static SizeAttributes StateSizeAttributes;
        
        public Image image;
        public int AssignedId { get; set; }
        public StateChartCell ConnectedCell { get; set; }

        public event Action TransitionLineAdded;
        public event Action LastTransitionLineRemoved;

        private const float SelectedHighlightFactor = 1.08f;
        private const float DragHighlightFactor = 1.15f; 
        private RectTransform _imageTransform;
        private List<TransitionLine> _outgoingTransitionLines = new ();

        public void Initialize(int assignedId)
        {
            _imageTransform = image.GetComponent<RectTransform>();
            SetSizeToDefault();

            AssignedId = assignedId;
        }

        public void PlayConnectionEffect(TweenCallback onCompleteAction)
        {
            _imageTransform.localScale = Vector2.one * connectionGrowEffectSize;
            _imageTransform.DOScale(Vector2.one, connectionGrowEffectTime).SetEase(Ease.InBounce)
                .OnComplete(onCompleteAction);
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
            _imageTransform.localScale = Vector2.one;
        }

        public void SetSizeToSelectedHighlight()
        {
            _imageTransform.localScale = Vector2.one * SelectedHighlightFactor;
        }

        public void SetSizeToDragHighlight()
        {
            _imageTransform.localScale = Vector2.one * DragHighlightFactor;
        }
        
        private void UpdateTransitionLines(float scaleDelta)
        {
            foreach (var outgoingTransitionLine in _outgoingTransitionLines)
            {
                var positionDelta = outgoingTransitionLine.transform.position - transform.position;
                positionDelta *= scaleDelta;
                outgoingTransitionLine.transform.position = transform.position + positionDelta;
                
                outgoingTransitionLine.UpdateSize(
                    StateSizeAttributes.FirstLineElementLength, 
                    StateSizeAttributes.LineElementLength, 
                    StateSizeAttributes.LineWidth);
            }
        }

        public void AddTransitionLine(TransitionLine newLine)
        {
            _outgoingTransitionLines.Add(newLine);
            TransitionLineAdded?.Invoke();
        }

        public void RemoveTransitionByCondition(StateChartManager.TransitionCondition condition)
        {
            var transitionMatch = _outgoingTransitionLines.First(transition => transition.Condition == condition);
            RemoveTransition(transitionMatch);
        }

        public void RemoveTransition(TransitionLine transitionLine)
        {
            _outgoingTransitionLines.Remove(transitionLine);
            transitionLine.FadeColorToDestroy();
            if(_outgoingTransitionLines.Count == 0)
                LastTransitionLineRemoved?.Invoke();
        }
    }
}
