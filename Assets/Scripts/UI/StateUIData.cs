using UnityEngine;
using static StateChartManager;

namespace UI
{
    [CreateAssetMenu(fileName = "newStateUIData", menuName = "StateUIData")]
    public class StateUIData : ScriptableObject
    {
        public string text;
        public Sprite image;
        public Color color;
        public StateAction action;
    }
}
