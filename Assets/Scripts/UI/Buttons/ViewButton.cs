using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Buttons
{
    public class ViewButton : MonoBehaviour, IButtonResettable
    {
        [SerializeField] private Sprite programViewSprite;
        [SerializeField] private Sprite levelViewSprite;
        [SerializeField] private float robotClickWobbleTime;
        [SerializeField] private float robotClickWobbleStrength;

        private Image _image;
        private Button _button;
        private Tween _robotClickWobble;
        private bool _listensToRobotClickEvent;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            UIManager.ViewStateChanged += ChangeImage;
            GameManager.RobotStateChanged += SetButtonToInteractable;
            ListenToRobotGotClickedEvent(true);
        }
        
        private void OnDisable()
        {
            UIManager.ViewStateChanged -= ChangeImage;
            GameManager.RobotStateChanged -= SetButtonToInteractable;
            ListenToRobotGotClickedEvent(false);
        }

        private void ChangeImage(bool programmingViewActive)
        {
            if (programmingViewActive)
            {
                _image.sprite = levelViewSprite;
                ListenToRobotGotClickedEvent(false);
            }
            else
            {
                _image.sprite = programViewSprite;
                ListenToRobotGotClickedEvent(true);
            }
        }

        private void ListenToRobotGotClickedEvent(bool listen)
        {
            if(listen == _listensToRobotClickEvent)
                return;

            if (listen)
            {
                Robot.Robot.RobotClicked += HandleRobotClicked;
                _listensToRobotClickEvent = true;
            }
            else
            {
                Robot.Robot.RobotClicked -= HandleRobotClicked;
                _listensToRobotClickEvent = false;
            }
        }

        private void SetButtonToInteractable(bool isRunning)
        {
            _button.interactable = !isRunning;
        }
        
        private void HandleRobotClicked()
        {
            if(_robotClickWobble != null)
                return;
            
            _robotClickWobble = transform.DOPunchScale(Vector2.one * robotClickWobbleStrength, robotClickWobbleTime, 5, 0f)
                .OnComplete(() =>
                {
                    _robotClickWobble = null;
                });
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Reset()
        {
            gameObject.SetActive(true);
            _button.interactable = true;
            _image.sprite = programViewSprite;
        }
    }
}