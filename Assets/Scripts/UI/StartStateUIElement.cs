using System.Linq;
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
            _uiElement.AddDefaultTransitionPlugToState();
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
