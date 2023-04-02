using System;
using System.Collections.Generic;
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

        private void Awake()
        {
            _image = GetComponent<Image>();
            _stateSprites = new Dictionary<StateChartManager.StateAction, Sprite>();
            _stateSprites.Add(StateChartManager.StateAction.None, startStateSprite);
            foreach (var data in stateUIData)
            {
                _stateSprites.Add(data.action, data.sprite);
            }
        }

        private void OnEnable()
        {
            StateChartRunner.NextStateStarts += OnNextState;
        }

        private void OnDisable()
        {
            StateChartRunner.NextStateStarts -= OnNextState;
        }

        private void OnNextState(StateChartManager.StateAction action)
        {
            _image.sprite = _stateSprites[action];
        }
    }
}
