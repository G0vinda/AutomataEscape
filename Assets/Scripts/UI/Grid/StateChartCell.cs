using UI.State;
using UI.Transition;
using UnityEngine;

namespace UI.Grid
{
    public class StateChartCell
    {
        public struct SubCell
        {
            public TransitionLine PlacedHorizontalLine { get; set; }
            public TransitionLine PlacedVerticalLine { get; set; }
        }
        
        /* "SubCells" is representing the sub cells in following pattern:
         * 6 7 8
         * 3 4 5
         * 0 1 2
         */
        public SubCell[] SubCells { get; }
        public StateUIElement PlacedStateElement { get; private set; }
        public bool IsEmpty => PlacedStateElement == null;

        public StateChartCell()
        {
            SubCells = new SubCell[9];

            for (var i = 0; i < SubCells.Length; i++)
            {
                SubCells[i] = new SubCell();
            }
        }

        public ref SubCell GetSubCellOnCoordinates(Vector2Int subCoordinates)
        {
            var subCellId = subCoordinates.y * 3 + subCoordinates.x;
            return ref SubCells[subCellId];
        }

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