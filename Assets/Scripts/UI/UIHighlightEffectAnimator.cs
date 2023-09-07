using System;
using System.Collections.Generic;
using UnityEngine;
using Coffee;
using Coffee.UIEffects;
using DG.Tweening;

namespace UI
{
    public class UIHighlightEffectAnimator : MonoBehaviour
    {
        [SerializeField] protected List<UIEffect> uiEffects;
        [SerializeField] protected float effectPeriodTime;
        [SerializeField] protected float effectMaxValue;
        
        protected Tween _effectTween;
        
        public virtual void PlayEffect()
        {
            _effectTween = DOVirtual.Float(0, effectMaxValue, effectPeriodTime, value => uiEffects.ForEach(effect => effect.colorFactor = value))
                .SetEase(Ease.OutSine).SetLoops(-1, LoopType.Yoyo);
        }

        public virtual void StopEffect()
        {
            _effectTween.Kill();
            uiEffects.ForEach(effect => effect.colorFactor = 0);
        }
    }
}
