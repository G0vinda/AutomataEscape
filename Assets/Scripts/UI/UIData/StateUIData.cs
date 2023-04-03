using UnityEngine;
using static Robot.StateChartManager;

namespace UI.UIData
{
    [CreateAssetMenu(fileName = "newStateUIData", menuName = "StateUIData")]
    public class StateUIData : ScriptableObject
    {
        public Sprite sprite;
        public Sprite inactiveSprite;
        public StateAction action;
    }
}
