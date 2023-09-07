using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PlasticGui.Help;
using Robot;
using UI.Transition;
using UnityEngine;

namespace UI
{
    public class UIHighlighter : MonoBehaviour
    {
        [SerializeField] private UIHighlightEffectAnimator viewButtonAnimator;

        private bool _viewButtonHighlighted;
        private List<StateChartManager.TransitionCondition> _highlightedTransitionConditions;
        private List<TransitionSelectElement> _currentlyHighlightedTransitionSelectElement = new ();

        #region OnEnable/OnDisable

        private void OnEnable()
        {
            UIManager.ViewStateChanged += RemoveViewButtonHighlight;
            TransitionSelectElement.TransitionSelectElementEnabled += HighlightTransitionSelectElementIfNotAlready;
            TransitionSelection.TransitionSelectElementSelected += HandleTransitionSelectElementSelected;
        }
        
        private void OnDisable()
        {
            UIManager.ViewStateChanged -= RemoveViewButtonHighlight;
            TransitionSelectElement.TransitionSelectElementEnabled -= HighlightTransitionSelectElementIfNotAlready;
            TransitionSelection.TransitionSelectElementSelected -= HandleTransitionSelectElementSelected;
        }

        #endregion
        
        private void Start()
        {
            var highLightedTransitionPrefsString = PlayerPrefs.GetString("highlightedTransitionConditions", "");
            var highlightedTransitionPrefs = JsonUtility.FromJson<TransitionConditionHighlightedPrefs>(highLightedTransitionPrefsString);

            if (highlightedTransitionPrefs == null || !highlightedTransitionPrefs.GetHighlightedTransitionConditions().Any())
            {
                _highlightedTransitionConditions = new List<StateChartManager.TransitionCondition>()
                    { StateChartManager.TransitionCondition.Default };

                var transitionConditionHighlightPrefs = new TransitionConditionHighlightedPrefs();
                var prefString = transitionConditionHighlightPrefs.GetJson(_highlightedTransitionConditions);
                PlayerPrefs.SetString("highlightedTransitionConditions", prefString);
            }
            else
            {
                _highlightedTransitionConditions = highlightedTransitionPrefs.GetHighlightedTransitionConditions();
            }

            _viewButtonHighlighted = PlayerPrefs.GetInt("viewButtonHighlighted", 0) > 0;

            if(!_viewButtonHighlighted)
            {
                viewButtonAnimator.PlayEffect();
                _viewButtonHighlighted = true;
                PlayerPrefs.SetInt("viewButtonHighlighted", 1);
            }
        }

        private void HighlightTransitionSelectElementIfNotAlready(TransitionSelectElement selectElement)
        {
            var condition = selectElement.Condition;
            if(_highlightedTransitionConditions.Contains(condition))
                return;
            
            var highlightEffectAnimator = selectElement.GetComponent<UIHighlightEffectAnimator>();
            highlightEffectAnimator.PlayEffect();
            
            _currentlyHighlightedTransitionSelectElement.Add(selectElement);
            _highlightedTransitionConditions.Add(condition);
            
            var transitionConditionHighlightPrefs = new TransitionConditionHighlightedPrefs();
            var prefString = transitionConditionHighlightPrefs.GetJson(_highlightedTransitionConditions);
            PlayerPrefs.SetString("highlightedTransitionConditions", prefString);
        }

        private void HandleTransitionSelectElementSelected(TransitionSelectElement selectElement)
        {
            if (!_currentlyHighlightedTransitionSelectElement.Contains(selectElement))
                return;
            
            var highlightEffectAnimator = selectElement.GetComponent<UIHighlightEffectAnimator>();
            highlightEffectAnimator.StopEffect();
            _currentlyHighlightedTransitionSelectElement.Remove(selectElement);
        }

        private void RemoveViewButtonHighlight(bool _)
        {
            if(!_viewButtonHighlighted)
                return;
            
            viewButtonAnimator.StopEffect();
        }
        
        [Serializable]
        private class TransitionConditionHighlightedPrefs
        {
            public List<int> highlightedTransitionIds;
            
            public string GetJson(List<StateChartManager.TransitionCondition> highlightedTransitionConditions)
            {
                SetHighlightedTransitionIds(highlightedTransitionConditions);
                return JsonUtility.ToJson(this);
            }

            public void SetHighlightedTransitionIds(List<StateChartManager.TransitionCondition> highlightedTransitionConditions)
            {
                highlightedTransitionIds = highlightedTransitionConditions.Select(condition => (int)condition).ToList();
            }

            public List<StateChartManager.TransitionCondition> GetHighlightedTransitionConditions()
            {
                return highlightedTransitionIds.Select(id => (StateChartManager.TransitionCondition)id).ToList();
            }
        }
    }
}
