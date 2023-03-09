using UnityEngine;

namespace UI
{
    public class StateChartCell
    {
        public StateUIElement PlacedStateElement { get; private set; }

        private StateChartSubCell[] _subCells;

        public StateChartCell()
        {
            _subCells = new StateChartSubCell[9];
            
            for (var i = 0; i < 3; i++) // Rows
            {
                for (var j = 0; j < 3; j++) // Columns
                {
                    _subCells[(i + 1) * (j + 1) - 1] =
                        new StateChartSubCell(this, new ByteCoordinates((byte)(-1 + j), (byte)(-1 + i)));
                }
            }
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