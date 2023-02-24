using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static StateChartManager;


public class StateChartRunner : MonoBehaviour
{
    [SerializeField] private GameObject keySprite;
    
    public bool IsRunning { get; private set; }
    
    private GridManager _gridManager;
    private (int, int) _currentCoordinates;
    private bool isCarryingKey = false;

    public void SetStartCoordinates((int, int) coordinates)
    {
        _currentCoordinates = coordinates;
        _gridManager = FindObjectOfType<GridManager>();
    }

    private Vector2Int GetCurrentDirection()
    {
        var dirAngle = (int)transform.rotation.eulerAngles.z;
        dirAngle = dirAngle > 180 ? dirAngle - 360 : dirAngle;
        
        switch(dirAngle)
        {
            case 0:
                return new Vector2Int(0, -1);
            case 90:
                return new Vector2Int(1, 0);
            case -90:
                return new Vector2Int(-1, 0);
            case 180:
                return new Vector2Int(0, 1);
            default:
                return new Vector2Int(0, 0); // Invalid case
        }
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
        switch (state.Action)
        {
            case StateAction.None:
                break;
            case StateAction.MoveForward:
                Move(GetCurrentDirection());
                break;
            case StateAction.TurnLeft:
                Turn(90);
                break;
            case StateAction.TurnRight:
                Turn(-90);
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

    private void Move(Vector2Int direction)
    {
        if(_gridManager.CheckIfWayIsBlocked(_currentCoordinates, direction))
            return;
        
        var newCoordinates = (_currentCoordinates.Item1 + direction.x, _currentCoordinates.Item2 + direction.y);
        
        transform.position = _gridManager.Grid[newCoordinates].transform.position;
        _currentCoordinates = newCoordinates;
        
        if (_gridManager.CheckIfTileIsGoal(_currentCoordinates))
        {
            IsRunning = false;
            GameManager.Instance.LoadNextLevel();
        }
    }

    private void Turn(int degree)
    {
        var currentAngle = transform.rotation.eulerAngles.z;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, currentAngle + degree));
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
        return _gridManager.CheckIfWayIsBlocked(_currentCoordinates, GetCurrentDirection());
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
