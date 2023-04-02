using UnityEngine;
using static StateChartManager;

namespace UI
{
    [CreateAssetMenu(fileName = "newStateUIData", menuName = "StateUIData")]
    public class StateUIData : ScriptableObject
    {
        public Sprite sprite;
        public Sprite inactiveSprite;
        public Sprite pinSprite;
        public StateAction action;
    }
}
