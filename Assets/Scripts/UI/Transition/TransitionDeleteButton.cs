using System;
using DG.Tweening;
using UnityEngine;

namespace UI.Transition
{
    public class TransitionDeleteButton : MonoBehaviour
    {
        [SerializeField] private float initiationDuration;
        public static event Action ButtonPressed;

        public void Initialize(Vector2 destination)
        {
            transform.localScale = Vector3.one * 0.2f;
            
            var instantiationSequence = DOTween.Sequence();
            instantiationSequence.Append(transform.DOMove(destination, initiationDuration));
            instantiationSequence.Join(transform.DOScale(1f, initiationDuration));
            instantiationSequence.SetEase(Ease.InCirc);
        }

        public void PressButton()
        {
            ButtonPressed?.Invoke();
        }
    }
}
