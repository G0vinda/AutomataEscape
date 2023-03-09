namespace UI
{
    public class StateChartSubCell
    {
        private StateChartCell _parent;
        private ByteCoordinates _subCoordinates;

        public StateChartSubCell(StateChartCell parent, ByteCoordinates subCoordinates)
        {
            _parent = parent;
            _subCoordinates = subCoordinates;
        }
    }
}