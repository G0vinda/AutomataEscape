using System;
using System.Collections.Generic;
using System.Linq;
using LevelGrid;
using Robot.States;
using Robot.Transitions;
using UnityEngine;
using Random = System.Random;


namespace Robot
{
    public class StateChartManager : MonoBehaviour
    {
        public enum StateAction
        {
            GoForward,
            TurnLeft,
            TurnRight,
            Grab,
            Drop,
            Start
        }
    
        [Serializable]
        public enum TransitionCondition
        {
            Default,
            IsInFrontOfWall,
            StandsOnKey,
            StandsOnOrange,
            StandsOnPurple
        }

        public static event Action<int> StateIsActive;
        public static event Action<int> StateIsInactive;
        public StartState StartState => (StartState)_stateChart.ElementAt(0);
        
        private LevelGridManager _levelGridManager;
        private SpriteChanger _spriteChanger;
        private List<RobotState> _stateChart;
        private Random _randomId = new ();

        private void Awake()
        {
            _levelGridManager = GameManager.Instance.GetLevelGridManager();
            _spriteChanger = GetComponent<SpriteChanger>();
            _stateChart = new List<RobotState> { new StartState(_spriteChanger) };
        }
    
        public int AddState(StateAction stateAction)
        {
            RobotState newState = stateAction switch
            {
                StateAction.GoForward => new GoForwardState(_levelGridManager, _spriteChanger, transform),
                StateAction.TurnLeft => new TurnLeftState(_levelGridManager, _spriteChanger),
                StateAction.TurnRight => new TurnRightState(_levelGridManager, _spriteChanger),
                StateAction.Grab => new GrabState(_levelGridManager, _spriteChanger),
                StateAction.Drop => new DropState(_levelGridManager, _spriteChanger),
                _ => throw new ArgumentOutOfRangeException()
            };
        
            newState.Id = GenerateStateId();
            _stateChart.Add(newState);
            return newState.Id;
        }

        public void AddTransition(TransitionCondition transitionCondition, int fromStateId, int toStateId)
        {
            var fromState = GetStateById(fromStateId);
            RobotTransition newTransition = transitionCondition switch
            {
                TransitionCondition.Default => new DefaultTransition(toStateId),
                TransitionCondition.IsInFrontOfWall => new IsInFrontOfWallTransition(toStateId),
                TransitionCondition.StandsOnKey => new StandsOnKeyTransition(toStateId),
                TransitionCondition.StandsOnOrange => new StandsOnOrangeTransition(toStateId),
                TransitionCondition.StandsOnPurple => new StandsOnPurpleTransition(toStateId),
                _ => throw new ArgumentOutOfRangeException()
            };
            fromState.AddTransition(newTransition);
            CheckForConnectedStates();
        }
    
        private int GenerateStateId()
        {
            int id;
            do
            {
                id = _randomId.Next(1, int.MaxValue);
            } while (_stateChart.Any(state => state.Id == id));

            return id;
        }
    
        public RobotState GetStateById(int id)
        {
            return _stateChart.FirstOrDefault(state => state.Id == id);
        }

        public void RemoveTransition(TransitionCondition transitionCondition, int fromStateId)
        {
            var typeToDelete = transitionCondition switch
            {
                TransitionCondition.Default => typeof(DefaultTransition),
                TransitionCondition.IsInFrontOfWall => typeof(IsInFrontOfWallTransition),
                TransitionCondition.StandsOnKey => typeof(StandsOnKeyTransition),
                TransitionCondition.StandsOnOrange => typeof(StandsOnOrangeTransition),
                TransitionCondition.StandsOnPurple => typeof(StandsOnPurpleTransition),
                _ => throw new ArgumentOutOfRangeException()
            };

            var fromState = GetStateById(fromStateId);
            var transitionToDelete =
                fromState.Transitions.First(transition => transition.GetType() == typeToDelete);
            
            fromState.RemoveTransition(transitionToDelete);
            CheckForConnectedStates();
        }
    
        public void RemoveStateById(int id)
        {
            _stateChart.Remove(GetStateById(id));
        }

        public bool CheckIfStartStateIsConnected()
        {
            return _stateChart[0].Transitions.Length > 0;
        }

        private void CheckForConnectedStates()
        {
            List<RobotState> statesToCheck = new () { _stateChart[0] }; // StartState
            List<RobotState> checkedStates = new();
            List<RobotState> activeStates = new();

            while (statesToCheck.Count > 0)
            {
                var statesInCheck = new List<RobotState>(statesToCheck);
                foreach (var state in statesInCheck)
                {
                    if (state.Transitions.Any(transition => transition is DefaultTransition))
                    {
                        activeStates.Add(state);
                        statesToCheck.AddRange(state.Transitions.Select(transition => GetStateById(transition.DestinationId)));
                    }
                    checkedStates.Add(state);
                }

                statesToCheck = statesToCheck.Except(checkedStates).ToList();
            }

            activeStates.Where(state => state.Id != 0).ToList().ForEach(state => StateIsActive?.Invoke(state.Id));

            var inactiveStates = _stateChart.Except(activeStates).ToList();
            
            inactiveStates.Where(state => state.Id != 0).ToList().ForEach(state => StateIsInactive?.Invoke(state.Id));
        }
    }
}
