using System;
using DG.Tweening;
using Robot;
using UI.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace UI.State
{
    public class StartStateUIElement : MonoBehaviour
    {
        [Header("Fade values:")]
        [SerializeField] private float imageFadeMax;
        [SerializeField] private float imageFadeMin;
        [SerializeField] private float imageFadeTime;
        [SerializeField] private Image frontImage;
        
        private StateUIElement _uiElement;
        private Tween _fadeTween;
        private Color _frontImageColor;
        private RectTransform _frontImageTransform;

        private void Awake()
        {
            _uiElement = GetComponent<StateUIElement>();   
        }

        #region OnEnable/OnDisable

        private void OnEnable()
        {
            _uiElement.TransitionLineAdded += StopFadeTween;
            _uiElement.LastTransitionLineRemoved += StartFadeTween;
        }

        private void OnDisable()
        {
            _uiElement.TransitionLineAdded -= StopFadeTween;
            _uiElement.LastTransitionLineRemoved -= StartFadeTween;
        }
        
        #endregion

        public void Initialize(StateChartCell connectedCell)
        {
            _uiElement.Initialize(0);
            _uiElement.ConnectedCell = connectedCell;
            _frontImageColor = frontImage.color;
            _frontImageTransform = frontImage.GetComponent<RectTransform>();
            UpdateFrontImagScaling();

            StartFadeTween();
        }

        public void UpdateFrontImagScaling()
        {
            _frontImageTransform.sizeDelta = StateUIElement.StateSizeAttributes.StateSize;
        }

        public void PlayErrorEffect(float effectTime)
        {
            StopFadeTween();
            DOVirtual.Color(Color.red, _frontImageColor, effectTime, newColor => frontImage.color = newColor).SetEase(Ease.OutQuad)
                .OnComplete(
                    () =>
                    {
                        if (_uiElement.GetNumberOfOutgoingTransitions() > 0)
                            StopFadeTween();
                        else
                            StartFadeTween();
                    });
        }

        private void StartFadeTween()
        {
            _fadeTween?.Kill();
            _fadeTween = DOVirtual.Float(imageFadeMax, imageFadeMin, imageFadeTime, newAlpha =>
            {
                _frontImageColor.a = newAlpha;
                frontImage.color = _frontImageColor;
            }).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void StopFadeTween()
        {
            _fadeTween?.Kill();
            _frontImageColor.a = 1f;
            frontImage.color = _frontImageColor;
        }

        public void RemoveDefaultTransitionLine()
        {
            if (_uiElement != null && _uiElement.GetNumberOfOutgoingTransitions() > 0)
                _uiElement.RemoveTransitionByCondition(StateChartManager.TransitionCondition.Default);
        }
    }
}
