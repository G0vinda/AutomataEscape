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

        public static event Action<TransitionSelectElement> TransitionSelectElementSelected;
        public TransitionSelectElement CurrentSelected { get; private set; }

        public void Setup(List<StateChartManager.TransitionCondition> availableTransitionConditions)
        {
            foreach (var element in selectElements)
            {
                if (availableTransitionConditions.Contains(element.Condition))
                {
                    element.enabled = true;
                    element.gameObject.SetActive(true);
                    element.HideSelectionMarking();
                }
                else
                {
                    element.enabled = false;
                    element.gameObject.SetActive(false);
                }
            }
            
            TrySetActive(true);
            
            foreach (var transitionSelectElement in selectElements)
            {

                transitionSelectElement.HideSelectionMarking();
            }
        
            SelectTransitionCondition(StateChartManager.TransitionCondition.Default, true);
        }

        public void SelectTransitionCondition(StateChartManager.TransitionCondition condition, bool isSetup = false)
        {
            if(CurrentSelected != null)
                CurrentSelected.HideSelectionMarking();
            
            var selectElement = selectElements.First(element => element.Condition == condition);
            selectElement.ShowSelectionMarking();
            CurrentSelected = selectElement;
            lineImage.color = selectElement.GetColor();

            TransitionLineDrawer.CurrentTransitionCondition = condition;
            
            if(!isSetup)
                TransitionSelectElementSelected?.Invoke(selectElement);
        }

        public void TrySetActive(bool value)
        {
            var activeSelectElements = selectElements.Count(element => element.gameObject.activeSelf);
            gameObject.SetActive(value && activeSelectElements > 1);
        }
    }
}