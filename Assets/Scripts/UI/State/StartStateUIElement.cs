using Robot;
using UI.Grid;
using UnityEngine;

namespace UI.State
{
    public class StartStateUIElement : MonoBehaviour
    {
        private StateUIElement _uiElement;

        public void Initialize(StateChartCell connectedCell)
        {
            _uiElement = GetComponent<StateUIElement>();
            _uiElement.Initialize(0);
            _uiElement.ConnectedCell = connectedCell;
        }

        public void RemoveDefaultTransitionLine()
        {
            if(_uiElement != null && _uiElement.GetNumberOfOutgoingTransitions() > 0)
                _uiElement.RemoveTransitionByCondition(StateChartManager.TransitionCondition.Default);
        }
    }
}
