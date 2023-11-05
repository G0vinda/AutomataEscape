using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LevelGrid;
using Robot.States;
using TMPro;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot
{
    public class Robot : MonoBehaviour
    {
        public static event Action<StateChartManager.StateAction> NextStateStarts;
        public static event Action StateChartStopped;
        public static event Action RobotClicked;
        public bool IsRunning { get; private set; }
        
        private SpriteChanger _spriteChanger;
        private Vector2Int _currentCoordinates;
        private Direction _currentDirection;
        private StateChartManager _stateChartManager;
        private WaitForSeconds _stateWait = new (0.8f);
        private Coroutine _currentRun;
        private List<Enemy.Enemy> _activeEnemies;
        private Tween _currentAnimation;
        private StateChartManager.StateAction _lastStateAction;

        private void Awake()
        {
            _stateChartManager = GetComponent<StateChartManager>();
            _spriteChanger = GetComponent<SpriteChanger>();
        }

        private void Start()
        {
            _spriteChanger.SetHeadToClosed();
        }

        public void ResetPosition(Vector2Int coordinates, Direction direction, List<Enemy.Enemy> enemies)
        {
            Initialize(coordinates, direction, enemies);
            _spriteChanger.SetHeadToOpen();
        }

        public void Initialize(Vector2Int coordinates, Direction direction, List<Enemy.Enemy> enemies)
        {
            _currentCoordinates = coordinates;
            _currentDirection = direction;
            _activeEnemies = enemies;
            KeyHandleState.ResetGrabbedKeyType();
            _spriteChanger.SetCarryKeyType(LevelGridManager.KeyType.None);
            _spriteChanger.SetDirection(direction);
            _spriteChanger.SetSpriteSortingOrder(LevelGridManager.GetSpriteSortingOrderFromCoordinates(coordinates));
            _spriteChanger.Initialize();
            _lastStateAction = StateChartManager.StateAction.GoForward; // As startState is always the first state this is fine
        }

        public void SetCoordinates(Vector2Int newCoordinates)
        {
            _currentCoordinates = newCoordinates;
            _spriteChanger.SetSpriteSortingOrder(LevelGridManager.GetSpriteSortingOrderFromCoordinates(newCoordinates));
        }

        public Vector2Int GetCoordinates()
        {
            return _currentCoordinates;
        }

        public void StartRun()
        {
            IsRunning = true;
            _currentRun = StartCoroutine(Run(_stateChartManager.StartState));
        }

        public void StopRun()
        {
            IsRunning = false;
            _spriteChanger.SetHeadSpriteToOff();
            StopCoroutine(_currentRun);
            _currentAnimation?.Kill();
        }

        private IEnumerator Run(StartState startState)
        {
            RobotState currentState = startState;
            var currentStatus = RobotState.Status.Running;
            do
            {
                _activeEnemies.ForEach(enemy => enemy.Move());
                
                if (currentState == null)
                {
                    yield return _stateWait;
                    continue;
                }

                if (GetStateActionIfNew(currentState, out var newStateAction))
                {
                    _lastStateAction = newStateAction;
                    NextStateStarts?.Invoke(newStateAction);
                }

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
                
                yield return _stateWait;

                if (currentStatus == RobotState.Status.Running)
                {
                    var nextStateId = currentState.DetermineNextStateId(_currentCoordinates, _currentDirection);
                    if (nextStateId < 0)
                    {
                        currentState = null;
                        StateChartStopped?.Invoke();
                        _spriteChanger.ShutDown();
                        SoundPlayer.Instance.PlayRobotShutdown();
                        SoundPlayer.Instance.PlayMusicLevel();
                    }
                    else
                    {
                        currentState = _stateChartManager.GetStateById(nextStateId);   
                    }
                }
                
            } while (true);

            _spriteChanger.CloseHead();
            yield return _stateWait;
            GameManager.Instance.ReachGoal();
        }

        private bool GetStateActionIfNew(RobotState robotState, out StateChartManager.StateAction newStateAction)
        {
            newStateAction = robotState switch
            {
                GoForwardState => StateChartManager.StateAction.GoForward,
                TurnRightState => StateChartManager.StateAction.TurnRight,
                TurnLeftState => StateChartManager.StateAction.TurnLeft,
                GrabState => StateChartManager.StateAction.Grab,
                DropState => StateChartManager.StateAction.Drop,
                StartState => StateChartManager.StateAction.Start,
                _ => throw new ArgumentException()
            };

            return newStateAction != _lastStateAction;
        }

        private void OnMouseDown()
        {
            if(IsRunning)
                return;
            
            RobotClicked?.Invoke();
        }
    }
}