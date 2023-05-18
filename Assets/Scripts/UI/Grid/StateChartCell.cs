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

        public void PlaceStateElement(StateUIElement stateUIElement)
        {
            PlacedStateElement = stateUIElement;
        }
    }
}