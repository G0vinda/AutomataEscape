using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RunButton : MonoBehaviour
    {
        [SerializeField] private Sprite runSprite;
        [SerializeField] private Sprite stopSprite;

        [SerializeField] private float errorEffectTime; 
        
        private Image _image;
        private Color _defaultColor;
        private Vector3 _defaultPosition;
        private bool _errorEffectShowing;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _defaultColor = _image.color;
            _defaultPosition = transform.position;
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
            Debug.Log("Invalid event was triggered");
            var errorSequence = DOTween.Sequence();
            errorSequence.Append(transform.DOShakePosition(errorEffectTime, 30f));
            errorSequence.Join(
                DOVirtual.Color(Color.red, _defaultColor, errorEffectTime, value => _image.color = value));
            errorSequence.SetEase(Ease.OutQuad).OnComplete(() => { _errorEffectShowing = false; });
        }

        private void ChangeImage(bool isRunning)
        {
            _image.sprite = isRunning ? stopSprite : runSprite;
        }
    }
}
