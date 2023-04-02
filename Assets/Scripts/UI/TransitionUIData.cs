using UnityEngine;

namespace UI
{
    [CreateAssetMenu(fileName = "newTransitionUIData", menuName = "TransitionUIData")]
    public class TransitionUIData : ScriptableObject
    {
        public Sprite icon;
        public Color color;
        public StateChartManager.TransitionCondition condition;
    }
}