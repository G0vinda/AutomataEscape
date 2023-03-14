using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UI
{
    public class StartStateUIElement : MonoBehaviour
    {
        private StateUIElement _uiElement;

        private void Awake()
        {
            _uiElement = GetComponent<StateUIElement>();
            //_uiElement.AddDefaultTransitionPlugToState();
        }

        public void Initialize(float scaleFactor)
        {
            _uiElement.Initialize(scaleFactor, 0);
            _uiElement.SetupEmptySlots();
        }

        public void ClearDefaultStateLine()
        {
            // TODO: causes error on mobile, but will be most probably removed on the line drawing rework 
            // foreach (var plug in _uiElement.connectedTransitionPlugs)
            // {
            //     if (plug == null) continue;
            //     
            //     plug.DisconnectLine();
            //     return;
            // }
        }
    }
}
