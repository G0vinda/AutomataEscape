using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tiles;
using UI;
using UnityEngine;
using TileType = Tiles.Tile.TileType;

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

    public readonly Quaternion Up = Quaternion.Euler(new Vector3(0, 0, 180f));
    public readonly Quaternion Down = Quaternion.Euler(new Vector3(0, 0, 0));
    public readonly Quaternion Right = Quaternion.Euler(new Vector3(0, 0, 90));
    public readonly Quaternion Left = Quaternion.Euler(new Vector3(0, 0, -90));

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
        _levels = new[]
        {
            new LevelData(
                (0, -1),
                Up,
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
            ),
            new LevelWithKeyData(
                (4, 0),
                (3, -2),
                Down,
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
            _stateChartRunner.StartRun(stateChartManager.GetStateChart());
            StateChartRunnerStateChanged?.Invoke(true);   
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
        Quaternion robotStartRotation = level.RobotStartRotation;
        _stateChartRunner = Instantiate(robotStateChartRunnerPrefab, robotStartPositionOnGrid, robotStartRotation);
        _stateChartRunner.SetStartCoordinates(level.RobotStartPosition);
        if (level is LevelWithKeyData)
        {
            var keyCoordinates = ((LevelWithKeyData)level).KeyPosition;
            Vector3 keyPositionOnGrid = gridManager.GetTilePosition(keyCoordinates);
            var key = Instantiate(keyObject, keyPositionOnGrid, Quaternion.identity).GetComponent<Key>();
            key.Coordinates = keyCoordinates;
        }
    }
}