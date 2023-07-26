using System.Collections.Generic;
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

        private Image _image;
        private Dictionary<StateChartManager.StateAction, Sprite> _stateSprites;
        private bool _playMusicOnNextState;

        private void Awake()
        {
            _image = GetComponent<Image>();
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
        }

        private void HandleStateChartStopped()
        {
            gameObject.SetActive(false);
        }
    }
}
