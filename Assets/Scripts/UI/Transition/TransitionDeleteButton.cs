using System;
using UnityEngine;

namespace UI.Transition
{
    public class TransitionDeleteButton : MonoBehaviour
    {
        public static event Action ButtonPressed;

        public void PressButton()
        {
            ButtonPressed?.Invoke();
        }
    }
}
