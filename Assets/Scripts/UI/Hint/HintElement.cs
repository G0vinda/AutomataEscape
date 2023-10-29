using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hint
{
    public class HintElement : MonoBehaviour
    {
        [SerializeField] private float minAlpha;
        [SerializeField] private float maxAlpha;
        [SerializeField] private float minSizeFactor;
        [SerializeField] private float spawnTime;

        private Image _image;
        private Tween _animation;
        private Color _elementColor;
        private Vector3 _spawnSize;
        
        private void Start()
        {
            _image = GetComponent<Image>();
            _elementColor = _image.color;
            _elementColor.a = 0.8f;
            _image.color = _elementColor;
            
            _spawnSize = Vector3.one * minSizeFactor;
            _animation = DOVirtual.Float(0, 1, spawnTime, ProcessAnimationStep).SetEase(Ease.InSine);
        }

        private void ProcessAnimationStep(float t)
        {
            _elementColor.a = Mathf.Lerp(minAlpha, maxAlpha, t);
            _image.color = _elementColor;
            transform.localScale = Vector3.Lerp(_spawnSize, Vector3.one, t);
        }
        
        public void StartYoyoEffect(float yoyoTime)
        {
            _animation?.Kill();
            _animation = DOVirtual.Float(0, 1, yoyoTime, ProcessAnimationStep).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }
    }
}
