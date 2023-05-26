using UnityEngine;
using UnityEngine.UI;

namespace UI.Buttons
{
    public class ViewButton : MonoBehaviour
    {
        [SerializeField] private Sprite programViewSprite;
        [SerializeField] private Sprite levelViewSprite;

        private Image _image;
        private Button _button;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            UIManager.ViewStateChanged += ChangeImage;
            GameManager.RobotStateChanged += SetButtonToInteractable;

        }

        private void OnDisable()
        {
            UIManager.ViewStateChanged -= ChangeImage;
            GameManager.RobotStateChanged -= SetButtonToInteractable;
        }

        private void ChangeImage(bool programmingViewActive)
        {
            _image.sprite = programmingViewActive ? levelViewSprite : programViewSprite;
        }

        private void SetButtonToInteractable(bool isRunning)
        {
            _button.interactable = !isRunning;
        }
    }
}