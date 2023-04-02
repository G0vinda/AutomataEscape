using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RunButton : MonoBehaviour
    {
        [SerializeField] private Sprite runSprite;
        [SerializeField] private Sprite stopSprite;
        
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            GameManager.Instance.StateChartRunnerStateChanged += ChangeImage;   
        }

        private void OnDisable()
        {
            GameManager.Instance.StateChartRunnerStateChanged -= ChangeImage;
        }

        private void ChangeImage(bool isRunning)
        {
            _image.sprite = isRunning ? stopSprite : runSprite;
        }
    }
}
