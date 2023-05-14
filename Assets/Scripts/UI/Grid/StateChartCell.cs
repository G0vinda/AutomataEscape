using UI.State;

namespace UI.Grid
{
    public class StateChartCell
    {
        public StateUIElement PlacedStateElement { get; private set; }
        public bool IsEmpty => PlacedStateElement == null;

        public void RemoveStateElement()
        {
            PlacedStateElement = null;
        }
        
        public void PlaceStateElement(StateUIPlaceElement stateElement)
        {
            PlacedStateElement = stateElement.GetComponent<StateUIElement>();
        }
        
        public void PlaceStateElement(StartStateUIElement stateElement)
        {
            PlacedStateElement = stateElement.GetComponent<StateUIElement>();
        }
    }
}