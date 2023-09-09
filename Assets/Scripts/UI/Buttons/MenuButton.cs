using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Buttons
{
    public class MenuButton : MonoBehaviour
    {
        [SerializeField] private int menuSceneIndex;

        private Button _button;
        private Image _image;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
        }

        #region OnEnable/OnDisable

        private void OnEnable()
        {
            UIManager.ViewStateChanged += OnViewStateChanged;
        }

        private void OnDisable()
        {
            UIManager.ViewStateChanged -= OnViewStateChanged;
        }

        #endregion

        public void ReturnToMenu()
        {
            SceneManager.LoadScene(menuSceneIndex);
        }

        private void OnViewStateChanged(bool programmingViewActive)
        {
            _button.interactable = !programmingViewActive;
            _image.enabled = !programmingViewActive;
        }
    }
}
