using UnityEngine;
using UnityEngine.PlayerLoop;

namespace UI
{
    public class StateChartCell
    {
        public struct SubCell
        {
            public TransitionLine PlacedHorizontalLine { get; set; }
            public TransitionLine PlacedVerticalLine { get; set; }
        }
        
        public StateUIElement PlacedStateElement { get; private set; }

        public SubCell[] SubCells { get; }

        public StateChartCell()
        {
            /* The SubCells are positioned like this:
             * 6 7 8
             * 3 4 5
             * 0 1 2
             */
            SubCells = new SubCell[9];

            for (var i = 0; i < SubCells.Length; i++)
            {
                SubCells[i] = new SubCell();
            }
        }

        public ref SubCell GetSubCellOnCoordinates(Vector2Int subCoordinates)
        {
            var subCellId = (subCoordinates.y + 1) * 3 + (subCoordinates.x + 1);
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