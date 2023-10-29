using DG.Tweening;
using Robot;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TransitionSelectUIHighlightEffectAnimator : UIHighlightEffectAnimator
    {
        [SerializeField] Image transitionSelectBackground;
        [SerializeField] private float maxBackgroundScaleFactor;

        private Color _backgroundStartColor;
        private Transform _backgroundTransform;

        public override void PlayEffect()
        {
            _backgroundStartColor = transitionSelectBackground.color;
            _backgroundTransform = transitionSelectBackground.transform;
            var maxBackgroundScaleVector = maxBackgroundScaleFactor * Vector3.one;
            EffectTween = DOVirtual.Float(0, effectMaxValue, effectPeriodTime, value =>
            {
                uiEffects.ForEach(effect => effect.colorFactor = value);
                transitionSelectBackground.color = Color.Lerp(_backgroundStartColor, Color.white, value);
                _backgroundTransform.localScale =
                    Vector3.Lerp(Vector3.one, maxBackgroundScaleVector, value / effectMaxValue);
            }).SetEase(Ease.OutSine).SetLoops(-1, LoopType.Yoyo);
        }

        public override void StopEffect()
        {
            base.StopEffect();
            transitionSelectBackground.color = _backgroundStartColor;
            _backgroundTransform.localScale = Vector3.one;
        }
    }
}
