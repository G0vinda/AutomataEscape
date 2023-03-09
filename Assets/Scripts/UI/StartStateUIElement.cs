using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UI
{
    public class StartStateUIElement : MonoBehaviour
    {
        private StateUIElement _uiElement;

        private void Start()
        {
            _uiElement = GetComponent<StateUIElement>();
            _uiElement.SetupEmptySlots();
            //_uiElement.AddDefaultTransitionPlugToState();
        }

        public void Initialize(float scaleFactor)
        {
            _uiElement.Initialize(scaleFactor, 0);
        }

        public void ClearDefaultStateLine()
        {
            foreach (var plug in _uiElement.connectedTransitionPlugs)
            {
                if (plug == null) continue;
                
                plug.DisconnectLine();
                return;
            }
        }
    }
}
