using System;
using DG.Tweening;
using TMPro;
using UI.Buttons;
using UI.State;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RunButton : MonoBehaviour, IButtonResettable
    {
        [SerializeField] private Sprite runSprite;
        [SerializeField] private Sprite stopSprite;
        [SerializeField] private StartStateUIElement startStateUIElement;
        [Header("Error effect values")]
        [SerializeField] private float errorEffectTime;
        [SerializeField] private float errorEffectShakeStrength;
         
        private Image _image;
        private Color _defaultColor;
        private bool _errorEffectShowing;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _defaultColor = _image.color;
        }

        private void OnEnable()
        {
            GameManager.RobotStateChanged += ChangeImage;
            GameManager.InvalidRunPress += PlayInvalidEffect;
        }

        private void OnDisable()
        {
            GameManager.RobotStateChanged -= ChangeImage;
            GameManager.InvalidRunPress -= PlayInvalidEffect;
        }

        private void PlayInvalidEffect()
        {
            if(_errorEffectShowing)
                return;
            
            _errorEffectShowing = true;
            var errorSequence = DOTween.Sequence();
            errorSequence.Append(transform.DOShakePosition(errorEffectTime, errorEffectShakeStrength));
            errorSequence.Join(
                DOVirtual.Color(Color.red, _defaultColor, errorEffectTime, value => _image.color = value));
            errorSequence.SetEase(Ease.OutQuad).OnComplete(() => { _errorEffectShowing = false; });

            startStateUIElement.PlayErrorEffect(errorEffectTime);
        }

        private void ChangeImage(bool isRunning)
        {
            _image.sprite = isRunning ? stopSprite : runSprite;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Reset()
        {
            gameObject.SetActive(true);
            _image.sprite = runSprite;
        }
    }
}
