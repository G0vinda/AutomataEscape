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

        private void OnDisable()
        {
            _uiElement.TransitionLineAdded -= StopFadeTween;
            _uiElement.LastTransitionLineRemoved -= StartFadeTween;
        }

        public void Initialize(StateChartCell connectedCell)
        {
            _uiElement = GetComponent<StateUIElement>();
            _uiElement.Initialize(0);
            _uiElement.ConnectedCell = connectedCell;
            _uiElement.TransitionLineAdded += StopFadeTween;
            _uiElement.LastTransitionLineRemoved += StartFadeTween;
            _frontImageColor = frontImage.color;

            StartFadeTween();
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
