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
    private Vector2Int _currentCoordinates;
    private Direction _currentDirection;
    private bool isCarryingKey;

    public void SetStartCoordinates(Vector2Int coordinates, Direction direction)
    {
        _currentCoordinates = coordinates;
        _currentDirection = direction;
        _gridManager = FindObjectOfType<GridManager>();
    }

    public void StartRun(StateChart stateChart)
    {
        StartCoroutine(Run(stateChart));
    }

    public IEnumerator Run(StateChart stateChart)
    {
        StateData currentState = stateChart.StartState;
        IsRunning = true;
        do
        {
            Debug.Log($"New Step in StateChartRunner starts: {currentState.StateId} ID");
            int nextStateId = ProcessState(currentState);
            Debug.Log($"Next Id should be {nextStateId}");
            currentState = stateChart.GetStateById(nextStateId);
            
            yield return new WaitForSeconds(1f);
        } while (IsRunning);
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

        foreach (var transition in state.Transitions)
        {
            switch (transition.Condition)
            {
                case TransitionCondition.IsInFrontOfWall:
                    if (CheckIfWayIsBlocked())
                    {
                        return transition.DestinationId;
                    }
                    break;
                case TransitionCondition.StandsOnKey:
                    if (CheckIfOnKey())
                    {
                        return transition.DestinationId;
                    }
                    break;
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
        transform.rotation = _currentDirection.ToZRotation();
    }

    private void Grab()
    {
        if (!CheckIfOnKey() || isCarryingKey)
        {
            return;
        }

        var key = GameObject.FindGameObjectWithTag("Key");
        Destroy(key);
        isCarryingKey = true;
        keySprite.SetActive(true);
    }

    private void Drop()
    {
        if(!isCarryingKey)
            return;
        
        _gridManager.DropKey(_currentCoordinates);
        isCarryingKey = false;
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
