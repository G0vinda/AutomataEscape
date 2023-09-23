using System.Collections.Generic;
using DG.Tweening;
using Robot;
using UI.UIData;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CurrentStateIndicator : MonoBehaviour
    {
        [SerializeField] private Sprite startStateSprite;
        [SerializeField] private List<StateUIData> stateUIData;
        [Header("OnChange Animation Values")]
        [SerializeField] private float onChangeSizeAnimationMax;
        [SerializeField] private float onChangeSizeAnimationTime;
        [Header("Idle Animation Values")]
        [SerializeField] private float idleSizeAnimationPeriodTime;
        [SerializeField] private float idleSizeAnimationMax;

        private Image _image;
        private UIHighlightEffectAnimator _highlightEffectAnimator;
        private Dictionary<StateChartManager.StateAction, Sprite> _stateSprites;
        private bool _playMusicOnNextState;
        private Tween _idleAnimation;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _highlightEffectAnimator = GetComponent<UIHighlightEffectAnimator>();
            _stateSprites = new Dictionary<StateChartManager.StateAction, Sprite>();
            _stateSprites.Add(StateChartManager.StateAction.Start, startStateSprite);
            foreach (var data in stateUIData)
            {
                _stateSprites.Add(data.action, data.sprite);
            }
        }

        private void OnEnable()
        {
            Robot.Robot.NextStateStarts += OnNextState;
            Robot.Robot.StateChartStopped += HandleStateChartStopped;
        }

        private void OnDisable()
        {
            StopIdleAnimation();
            Robot.Robot.NextStateStarts -= OnNextState;
            Robot.Robot.StateChartStopped -= HandleStateChartStopped;
        }

        private void OnNextState(StateChartManager.StateAction action)
        {
            _image.sprite = _stateSprites[action];
            if (action == StateChartManager.StateAction.Start)
            {
                SoundPlayer.Instance.PlayRobotStartUp();
                _playMusicOnNextState = true;
            }
            else if (_playMusicOnNextState)
            {
                SoundPlayer.Instance.PlayMusicWalking();
                _playMusicOnNextState = false;
            }

            if (action != StateChartManager.StateAction.Start)
            {
                SoundPlayer.Instance.PlayRunStateChange();
            }

            StopIdleAnimation();
            _image.transform.DOScale(Vector3.one * onChangeSizeAnimationMax, onChangeSizeAnimationTime).SetEase(Ease.OutCirc).SetLoops(2, LoopType.Yoyo)
                .OnComplete(StartIdleAnimation);
        }

        private void StartIdleAnimation()
        {
            Debug.Log("StartIdleAnimation called");
            _idleAnimation = _image.transform.DOScale(Vector3.one * idleSizeAnimationMax, idleSizeAnimationPeriodTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            _highlightEffectAnimator.PlayEffect();
        }

        private void StopIdleAnimation()
        {
            _idleAnimation?.Kill();
            _highlightEffectAnimator.StopEffect();
        }

        private void HandleStateChartStopped()
        {
            gameObject.SetActive(false);
        }
    }
}
