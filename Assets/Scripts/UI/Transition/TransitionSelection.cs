using System;
using System.Collections.Generic;
using System.Linq;
using Robot;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Transition
{
    public class TransitionSelection : MonoBehaviour
    {
        [SerializeField] private List<TransitionSelectElement> selectElements;
        [SerializeField] private Image lineImage;
        
        public TransitionSelectElement CurrentSelected { get; private set; }

        public void Setup(List<StateChartManager.TransitionCondition> availableTransitionConditions)
        {
            foreach (var element in selectElements)
            {
                if (availableTransitionConditions.Contains(element.Condition))
                {
                    element.gameObject.SetActive(true);
                    element.HideSelectionMarking();
                }
                else
                {
                    element.gameObject.SetActive(false);
                }
            }
            
            TrySetActive(true);
            
            foreach (var transitionSelectElement in selectElements)
            {

                transitionSelectElement.HideSelectionMarking();
            }
        
            SelectTransitionCondition(StateChartManager.TransitionCondition.Default);
        }

        public void SelectTransitionCondition(StateChartManager.TransitionCondition condition)
        {
            if(CurrentSelected != null)
                CurrentSelected.HideSelectionMarking();
            
            var selectElement = selectElements.First(element => element.Condition == condition);
            selectElement.ShowSelectionMarking();
            CurrentSelected = selectElement;
            lineImage.color = selectElement.GetColor();
            
            TransitionLineDrawer.CurrentTransitionCondition = condition;
        }

        public void TrySetActive(bool value)
        {
            var activeSelectElements = selectElements.Count(element => element.gameObject.activeSelf);
            gameObject.SetActive(value && activeSelectElements > 1);
        }
    }
}