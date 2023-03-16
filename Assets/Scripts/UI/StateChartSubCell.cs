using UnityEngine;

namespace UI
{
    public class StateChartSubCell
    {
        public bool BlockedOnHorizontal { get; set; }
        public bool BlockedOnVertical { get; set; }
        
        private StateChartCell _parent;
        private Vector2Int _subCoordinates;

        public StateChartSubCell(StateChartCell parent, Vector2Int subCoordinates)
        {
            _parent = parent;
            _subCoordinates = subCoordinates;
        }
    }
}