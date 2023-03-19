using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class StateChartManager : MonoBehaviour
{
    public enum StateAction
    {
        MoveForward,
        TurnLeft,
        TurnRight,
        Grab,
        Drop,
        None
    }
    
    public enum TransitionCondition
    {
        Default,
        IsInFrontOfWall,
        StandsOnKey
    }
        
    public class Transition
    {
        public TransitionCondition Condition;
        public int DestinationId;

        public Transition(TransitionCondition condition, int destinationId)
        {
            Condition = condition;
            DestinationId = destinationId;
        }
    }

    public class StateData
    {
        public int StateId;
        public StateAction Action;
        public List<Transition> Transitions;
        public int DefaultTransitionDestinationId;

        public StateData(StateAction action)
        {
            StateId = 0;
            Action = action;
            Transitions = new List<Transition>();
            DefaultTransitionDestinationId = -1;
        }

        public void RemoveTransitionByCondition(TransitionCondition condition)
        {
            foreach (var transition in Transitions)
            {
                if (transition.Condition == condition)
                {
                    Transitions.Remove(transition);
                    break;
                }    
            }
        }
    }

    public class StateChart
    {
        public StateData StartState;
        private List<StateData> _activeStates;

        public StateChart()
        {
            _activeStates = new List<StateData>();
            StartState = new StateData(StateAction.None);
            _activeStates.Add(StartState);
        }

        public void AddState(StateData newState)
        {
            newState.StateId = GenerateStateId();
            _activeStates.Add(newState);
        }

        private int GenerateStateId()
        {
            int id;
            bool idIsUnique;
            
            do
            {
                id = Random.Range(1, 1000);
                idIsUnique = true;
                foreach (var activeState in _activeStates)
                {
                    if (activeState.StateId == id)
                    {
                        idIsUnique = false;
                        break;
                    }
                }
            } while (!idIsUnique);

            return id;
        }

        public void RemoveStateById(int id)
        {
            _activeStates.Remove(GetStateById(id));
        Debug.Log($"State with id {id} removed");
        }

        public StateData GetStateById(int id)
        {
            foreach (var state in _activeStates)
            {
                if (state.StateId == id)
                {
                    return state;
                }
            }

            return null;
        }

        public bool CheckIfChartIsExecutable(out List<int> stateIdsWithError)
        {
            stateIdsWithError = new List<int>();
            if (_activeStates.Count <= 1)
                return false;

            foreach (var activeState in _activeStates)
            {
                if (activeState.DefaultTransitionDestinationId < 0)
                    stateIdsWithError.Add(activeState.StateId);
            }

            foreach (var destinationState in _activeStates)
            {
                var destinationId = destinationState.StateId;
                if(destinationId == 0)
                    continue;
                
                var isDestinationOfTransition = false;
                foreach (var sourceState in _activeStates)
                {
                    if (destinationState == sourceState)
                        continue;

                    if (sourceState.Transitions.Any(transition => transition.DestinationId == destinationId) ||
                        sourceState.DefaultTransitionDestinationId == destinationId)
                    {
                        isDestinationOfTransition = true;
                        break;
                    }
                }

                if (!isDestinationOfTransition)
                    stateIdsWithError.Add(destinationState.StateId);
            }

            stateIdsWithError = stateIdsWithError.Distinct().ToList();
            return stateIdsWithError.Count == 0;
        }
    }

    private StateChart _currentStateChart;

    public void ResetStateChart()
    {
        _currentStateChart = new StateChart();
    }

    public StateChart GetStateChart()
    {
        return _currentStateChart;
    }

    public int AddState(StateAction stateAction)
    {
        StateData newState = new StateData(stateAction);
        _currentStateChart.AddState(newState);
        return newState.StateId;
    }

    public void AddTransition(TransitionCondition transitionCondition, int fromStateId, int toStateId)
    {
        var fromState = _currentStateChart.GetStateById(fromStateId);
        var newTransition = new Transition(transitionCondition, toStateId);
        fromState.Transitions.Add(newTransition);
    }

    public void AddDefaultTransition(int fromStateId, int toStateId)
    {
        var fromState = _currentStateChart.GetStateById(fromStateId);
        fromState.DefaultTransitionDestinationId = toStateId;
    }

    public void RemoveState(int stateId)
    {
        _currentStateChart.RemoveStateById(stateId);
    }

    public void RemoveTransition(TransitionCondition transitionCondition, int fromStateId)
    {
        var state = _currentStateChart.GetStateById(fromStateId);
        state.RemoveTransitionByCondition(transitionCondition);   
    }

    public void RemoveDefaultTransition(int stateId)
    {
        var state = _currentStateChart.GetStateById(stateId);
        state.DefaultTransitionDestinationId = -1;
    }
    
}
