using Robot;
using UnityEngine;

namespace UI.UIData
{
    [CreateAssetMenu(fileName = "newTransitionUIData", menuName = "TransitionUIData")]
    public class TransitionUIData : ScriptableObject
    {
        public Sprite icon;
        public Color color;
        public StateChartManager.TransitionCondition condition;
    }
}