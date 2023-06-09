using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LevelGrid;
using Robot.States;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot
{
    public class Robot : MonoBehaviour
    {
        public static event Action<StateChartManager.StateAction> NextStateStarts;
        public bool IsRunning { get; private set; }
        
        private SpriteChanger _spriteChanger;
        private Vector2Int _currentCoordinates;
        private Direction _currentDirection;
        private StateChartManager _stateChartManager;
        private WaitForSeconds _stateWait = new (0.8f);
        private Coroutine _currentRun;
        private Tween _currentAnimation;

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

        public void SetCoordinates(Vector2Int newCoordinates)
        {
            _currentCoordinates = newCoordinates;
            _spriteChanger.SetSpriteSortingOrder(LevelGridManager.GetSpriteSortingOrderFromCoordinates(newCoordinates));
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
            _currentAnimation?.Kill();
        }

        private IEnumerator Run(StartState startState)
        {
            RobotState currentState = startState;
            var currentStatus = RobotState.Status.Running;
            do
            {
                NextStateStarts?.Invoke(DetermineStateAction(currentState));

                if (currentStatus == RobotState.Status.Pause)
                {
                    currentStatus = RobotState.Status.Running;
                }
                else
                {
                    currentStatus = currentState.ProcessState(ref _currentCoordinates, ref _currentDirection,
                        out _currentAnimation);   
                }

                if (currentStatus == RobotState.Status.ReachedGoal)
                {
                    break;
                }

                if (currentStatus == RobotState.Status.Running)
                {
                    var nextStateId = currentState.DetermineNextStateId(_currentCoordinates, _currentDirection);
                    currentState = _stateChartManager.GetStateById(nextStateId);    
                }
                
                yield return _stateWait;
            } while (true);

            yield return _stateWait;
            GameManager.Instance.ReachGoal();
        }

        private StateChartManager.StateAction DetermineStateAction(RobotState robotState)
        {
            return robotState switch
            {
                GoForwardState => StateChartManager.StateAction.GoForward,
                TurnRightState => StateChartManager.StateAction.TurnRight,
                TurnLeftState => StateChartManager.StateAction.TurnLeft,
                GrabState => StateChartManager.StateAction.Grab,
                DropState => StateChartManager.StateAction.Drop,
                StartState => StateChartManager.StateAction.Start,
                _ => throw new ArgumentException()
            };
        }
    }
}