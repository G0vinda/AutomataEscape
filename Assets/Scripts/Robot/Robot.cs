using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Robot.States;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot
{
    public class Robot : MonoBehaviour
    {
        public static Action<StateChartManager.StateAction> NextStateStarts;
        public bool IsRunning { get; private set; }
        
        private SpriteChanger _spriteChanger;
        private Vector2Int _currentCoordinates;
        private Direction _currentDirection;
        private StateChartManager _stateChartManager;
        private LevelGridManager.KeyType _grabbedKeyType;
        private WaitForSeconds _waitForSecond = new (1f);
        private Coroutine _currentRun;

        private void Awake()
        {
            _stateChartManager = GetComponent<StateChartManager>();
            _spriteChanger = GetComponent<SpriteChanger>();
        }

        public void Initialize(Vector2Int coordinates, Direction direction)
        {
            _currentCoordinates = coordinates;
            _currentDirection = direction;
            _spriteChanger.SetCarryKeyType(LevelGridManager.KeyType.None);
            _spriteChanger.SetSpriteDirection(direction);
            _spriteChanger.SetSpriteSortingOrder(LevelGridManager.GetSpriteSortingOrderFromCoordinates(coordinates));
        }

        public ref LevelGridManager.KeyType GetGrabbedKeyReference()
        {
            return ref _grabbedKeyType;
        }
        
        public void StartRun()
        {
            IsRunning = true;
            _currentRun = StartCoroutine(Run(_stateChartManager.StartState));
        }

        public void StopRun()
        {
            IsRunning = false;
            StopCoroutine(_currentRun);
        }

        private IEnumerator Run(StartState startState)
        {
            RobotState currentState = startState;
            do
            {
                NextStateStarts?.Invoke(DetermineStateAction(currentState));
                yield return _waitForSecond;
                
                if (currentState.ProcessState(ref _currentCoordinates, ref _currentDirection))
                    break;
                var nextStateId = currentState.DetermineNextStateId(_currentCoordinates, _currentDirection);
                currentState = _stateChartManager.GetStateById(nextStateId);
            } while (true);
            
            GameManager.Instance.LoadNextLevel();
        }

        private StateChartManager.StateAction DetermineStateAction(RobotState robotState)
        {
            switch (robotState)
            {
                case GoForwardState:
                    return StateChartManager.StateAction.GoForward;
                case TurnRightState:
                    return StateChartManager.StateAction.TurnRight;
                case TurnLeftState:
                    return StateChartManager.StateAction.TurnLeft;
                case GrabState:
                    return StateChartManager.StateAction.Grab;
                case DropState:
                    return StateChartManager.StateAction.Drop;
                case StartState:
                    return StateChartManager.StateAction.Start;
                default:
                    throw new ArgumentException();
            }
        }
    }
}