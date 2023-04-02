using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ViewButton : MonoBehaviour
    {
        [SerializeField] private Sprite programViewSprite;
        [SerializeField] private Sprite levelViewSprite;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            UIManager.ViewStateChanged += ChangeImage;   
        }

        private void OnDisable()
        {
            UIManager.ViewStateChanged -= ChangeImage;
        }

        private void ChangeImage(bool programmingViewActive)
        {
            _image.sprite = programmingViewActive ? levelViewSprite : programViewSprite;
        }
    }
}