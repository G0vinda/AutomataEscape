using System;
using System.Collections;
using Helper;
using UI;
using UnityEngine;
using static StateChartManager;


public class StateChartRunner : MonoBehaviour
{
    [SerializeField] private GameObject keySprite;

    public static Action<StateAction> NextStateStarts;  
    
    public bool IsRunning { get; private set; }
    
    private GridManager _gridManager;
    private RobotSpriteChanger _spriteChanger;
    private Vector2Int _currentCoordinates;
    private Direction _currentDirection;
    private bool _isCarryingKey;

    public void SetStartCoordinates(Vector2Int coordinates, Direction direction)
    {
        _currentCoordinates = coordinates;
        _currentDirection = direction;
        _gridManager = FindObjectOfType<GridManager>(); // Todo: Fetch from GameManager
        _spriteChanger = GetComponent<RobotSpriteChanger>();
        _spriteChanger.SetSprite(direction);
    }

    public void StartRun(StateChart stateChart)
    {
        StartCoroutine(Run(stateChart));
    }

    private IEnumerator Run(StateChart stateChart)
    {
        var currentState = stateChart.StartState;
        IsRunning = true;
        do
        {
            Debug.Log($"New Step in StateChartRunner starts: {currentState.StateId} ID");
            var nextStateId = ProcessState(currentState);
            if(!IsRunning)
                break;
            Debug.Log($"Next Id should be {nextStateId}");
            currentState = stateChart.GetStateById(nextStateId);
            
            yield return new WaitForSeconds(1f);
        } while (true);
    }

    private int ProcessState(StateData state)
    {
        NextStateStarts?.Invoke(state.Action);
        switch (state.Action)
        {
            case StateAction.None:
                break;
            case StateAction.MoveForward:
                Move(_currentDirection);
                if (!IsRunning)
                    return -1;
                break;
            case StateAction.TurnLeft:
                Turn(false);
                break;
            case StateAction.TurnRight:
                Turn(true);
                break;
            case StateAction.Grab:
                Grab();
                break;
            case StateAction.Drop:
                Drop();
                break;
        }

        Transition transition;
        if (state.TryGetTransition(TransitionCondition.IsInFrontOfWall, out transition))
        {
            if (CheckIfWayIsBlocked())
            {
                return transition.DestinationId;
            }
        }
        
        if (state.TryGetTransition(TransitionCondition.StandsOnKey, out transition))
        {
            if (CheckIfOnKey())
            {
                return transition.DestinationId;
            }
        }
        
        if (state.TryGetTransition(TransitionCondition.StandsOnOrange, out transition))
        {
            if (_gridManager.CheckIfTileIsOrange(_currentCoordinates))
            {
                return transition.DestinationId;
            }
        }
        
        if (state.TryGetTransition(TransitionCondition.StandsOnPurple, out transition))
        {
            if (_gridManager.CheckIfTileIsPurple(_currentCoordinates))
            {
                return transition.DestinationId;
            }
        }

        return state.DefaultTransitionDestinationId;
    }

    private void Move(Direction direction)
    {
        if(_gridManager.CheckIfWayIsBlocked(_currentCoordinates, direction))
            return;
        
        var newCoordinates = _currentCoordinates + direction.ToVector2Int();
        
        transform.position = _gridManager.Grid[newCoordinates].transform.position;
        _currentCoordinates = newCoordinates;
        
        if (_gridManager.CheckIfTileIsGoal(_currentCoordinates))
        {
            IsRunning = false;
            GameManager.Instance.LoadNextLevel();
        }
    }

    private void Turn(bool turnClockwise)
    {
        _currentDirection = _currentDirection.Turn(turnClockwise);
        _spriteChanger.SetSprite(_currentDirection);
    }

    private void Grab()
    {
        if (!CheckIfOnKey() || _isCarryingKey)
        {
            return;
        }

        var key = GameObject.FindGameObjectWithTag("Key");
        Destroy(key);
        _isCarryingKey = true;
        keySprite.SetActive(true);
    }

    private void Drop()
    {
        if(!_isCarryingKey)
            return;
        
        _gridManager.DropKey(_currentCoordinates);
        _isCarryingKey = false;
        keySprite.SetActive(false);
    }

    private bool CheckIfWayIsBlocked()
    {
        return _gridManager.CheckIfWayIsBlocked(_currentCoordinates, _currentDirection);
    }

    private bool CheckIfOnKey()
    {
        var keyInstance = GameObject.FindWithTag("Key");
        if (keyInstance == null)
        {
            return false;
        }
        
        var key = keyInstance.GetComponent<Key>();
        return key.Coordinates == _currentCoordinates;
    }
}
