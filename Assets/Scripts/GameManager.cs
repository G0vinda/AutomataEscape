using System;
using System.Collections.Generic;
using Helper;
using UI;
using UnityEngine;
using TileType = Tiles.Tile.TileType;

[DefaultExecutionOrder(-1)] // Game Manager will be executed before all other scripts
public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private StateChartManager stateChartManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private StateChartRunner robotStateChartRunnerPrefab;
    [SerializeField] private GameObject keyObject;

    public static GameManager Instance { get; private set; }
    public Action<bool> StateChartRunnerStateChanged;

    private LevelData[] _levels;
    private StateChartRunner _stateChartRunner;
    private int _currentLevelId = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        uiManager.Initialize();
        _levels = new[]
        {
            new LevelData(
                new Vector2Int(0, -1),
                Direction.Up,
                new[,]
                {
                    { TileType.Floor, TileType.Floor, TileType.None, TileType.Floor },
                    { TileType.Floor, TileType.Floor, TileType.Floor, TileType.Goal }
                },
                new List<LevelData.AvailableStateInfo>()
                {
                    new (StateChartManager.StateAction.MoveForward, 2), 
                    new (StateChartManager.StateAction.TurnRight, 1)
                },
                new List<StateChartManager.TransitionCondition>()
                {
                    StateChartManager.TransitionCondition.Default
                }
            ),
            new LevelWithKeyData(
                new Vector2Int(4, 0),
                new Vector2Int(3, -2),
                Direction.Down,
                new[,]
                {
                    { TileType.Goal, TileType.Floor, TileType.None, TileType.Floor, TileType.Floor },
                    { TileType.Floor, TileType.None, TileType.None, TileType.Floor, TileType.Floor },
                    { TileType.Floor, TileType.Floor, TileType.GateLeft, TileType.Floor, TileType.Floor },
                    { TileType.Floor, TileType.None, TileType.None, TileType.Floor, TileType.Floor }
                },
                new List<LevelData.AvailableStateInfo>()
                {
                    new(StateChartManager.StateAction.MoveForward, 2), 
                    new(StateChartManager.StateAction.TurnRight, 1),
                    new(StateChartManager.StateAction.TurnLeft, 1), 
                    new(StateChartManager.StateAction.Grab, 1),
                    new(StateChartManager.StateAction.Drop, 1)
                },
                new List<StateChartManager.TransitionCondition>()
                {
                    StateChartManager.TransitionCondition.Default,
                    StateChartManager.TransitionCondition.IsInFrontOfWall,
                    StateChartManager.TransitionCondition.StandsOnKey
                }
            )
        };

        LoadLevel(_currentLevelId);
    }

    public UIManager GetUIManager()
    {
        return uiManager;
    }

    public InputManager GetInputManager()
    {
        return inputManager;
    }

    public StateChartManager GetStateChartManager()
    {
        return stateChartManager;
    }

    public void ToggleStateChartView()
    {
        uiManager.gameObject.SetActive(!uiManager.gameObject.activeSelf);
    }

    public void ToggleStateChartRunState()
    {
        if (_stateChartRunner.IsRunning)
        {
            ReloadLevel();
            StateChartRunnerStateChanged?.Invoke(false);
        }
        else
        {
            if (!stateChartManager.GetStateChart().CheckIfChartIsExecutable(out var errorStateIds))
            {
                Debug.Log("StateChart can't be executed, following states have errors:");
                foreach (var errorStateId in errorStateIds)
                {
                    Debug.Log($"State {errorStateId}");
                }
            }
            else
            {
                _stateChartRunner.StartRun(stateChartManager.GetStateChart());
                StateChartRunnerStateChanged?.Invoke(true);   
            }
        }
    }

    private void ReloadLevel()
    {
        Destroy(_stateChartRunner.gameObject);
        LoadLevelGrid(_levels[_currentLevelId]);
    }

    public void LoadNextLevel()
    {
        Destroy(_stateChartRunner.gameObject);
        StateChartRunnerStateChanged?.Invoke(false);
        _currentLevelId++;
        LoadLevel(_currentLevelId);
    }

    private void LoadLevel(int levelId)
    {
        var level = _levels[levelId];
        LoadLevelGrid(level);

        // Setup StateChart
        stateChartManager.ResetStateChart();
        uiManager.SetupUI(level.AvailableActions, level.AvailableTransitionConditions);
    }

    private void LoadLevelGrid(LevelData level)
    {
        gridManager.CreateLevelBasedOnGrid(level.Grid);
        var tileRenderers = gridManager.GetTileObjectRenderers();
        cameraController.AlignCameraWithLevel(tileRenderers);
        Vector3 robotStartPositionOnGrid = gridManager.GetTilePosition(level.RobotStartPosition);
        Quaternion robotStartRotation = level.RobotStartDirection.ToZRotation();
        _stateChartRunner = Instantiate(robotStateChartRunnerPrefab, robotStartPositionOnGrid, robotStartRotation);
        _stateChartRunner.SetStartCoordinates(level.RobotStartPosition, level.RobotStartDirection);
        if (level is LevelWithKeyData)
        {
            var keyCoordinates = ((LevelWithKeyData)level).KeyPosition;
            Vector3 keyPositionOnGrid = gridManager.GetTilePosition(keyCoordinates);
            var key = Instantiate(keyObject, keyPositionOnGrid, Quaternion.identity).GetComponent<Key>();
            key.Coordinates = keyCoordinates;
        }
    }
}