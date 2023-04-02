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
        StandsOnKey,
        StandsOnOrange,
        StandsOnPurple
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

        public bool TryGetTransition(TransitionCondition condition, out Transition transition)
        {
            transition = null;
            if (Transitions.All(t => t.Condition != condition))
                return false;
            
            transition = Transitions.First(t => t.Condition == condition);
            return true;
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
                var stateId = activeState.StateId;
                if (!CheckIfStateIsConnected(stateId))
                    stateIdsWithError.Add(activeState.StateId);
            }
            
            stateIdsWithError = stateIdsWithError.Distinct().ToList();
            return stateIdsWithError.Count == 0;
        }

        public bool CheckIfStateIsConnected(int stateId)
        {
            var state = GetStateById(stateId);

            if (state.DefaultTransitionDestinationId < 0)
                return false;

            if (stateId == 0)
                return true;
            
            var isDestinationOfTransition = false;
            foreach (var sourceState in _activeStates)
            {
                if (state == sourceState)
                    continue;

                if (sourceState.Transitions.Any(transition => transition.DestinationId == stateId) ||
                    sourceState.DefaultTransitionDestinationId == stateId)
                {
                    isDestinationOfTransition = true;
                    break;
                }
            }

            return isDestinationOfTransition;
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
