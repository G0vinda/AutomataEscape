using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CurrentStateIndicator : MonoBehaviour
    {
        [SerializeField] private Color startColor;
        [SerializeField] private Color forwardColor;
        [SerializeField] private Color turnLeftColor;
        [SerializeField] private Color turnRightColor;
        [SerializeField] private Color grabColor;
        [SerializeField] private Color dropColor;

        private Image _image;
        
        private void Awake()
        {
            _image = GetComponent<Image>();
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
            _image.color = action switch
            {
                StateChartManager.StateAction.MoveForward => forwardColor,
                StateChartManager.StateAction.TurnLeft => turnLeftColor,
                StateChartManager.StateAction.TurnRight => turnRightColor,
                StateChartManager.StateAction.Grab => grabColor,
                StateChartManager.StateAction.Drop => dropColor,
                StateChartManager.StateAction.None => startColor,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
