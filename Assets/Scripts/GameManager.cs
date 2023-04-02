using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

[DefaultExecutionOrder(-1)] // Game Manager will be executed before all other scripts
public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private StateChartManager stateChartManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private StateChartRunner robotStateChartRunnerPrefab;
    [SerializeField] private GameObject redKeyPrefab;
    [SerializeField] private GameObject blueKeyPrefab;
    [SerializeField] private CurrentStateIndicator currentStateIndicator;
    [SerializeField] private bool resetSaveSystemOnStart;

    public static GameManager Instance { get; private set; }
    public Action<bool> StateChartRunnerStateChanged;
    
    private StateChartRunner _stateChartRunner;
    private Dictionary<Vector2Int, (GridManager.KeyType, GameObject)> _currentKeyObjectData = new ();
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
        if(resetSaveSystemOnStart)
            PlayerPrefs.SetInt("CurrentLevelId", 4);
        _currentLevelId = PlayerPrefs.GetInt("CurrentLevelId", 0);
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

    public bool IsKeyOnCoordinates(Vector2Int coordinates)
    {
        return _currentKeyObjectData.ContainsKey(coordinates);
    }

    public void DropKeyOnCoordinates(Vector2Int coordinates, GridManager.KeyType keyType)
    {
        if(gridManager.UnlockGateWithKeyIfPossible(coordinates, keyType))
            return;
        
        GameObject newKey;   
        var keyPosition = gridManager.GetTilePosition(coordinates);
        if (keyType == GridManager.KeyType.Blue)
        {
            newKey = Instantiate(blueKeyPrefab, keyPosition, Quaternion.identity);
        }
        else if (keyType == GridManager.KeyType.Red)
        {
            newKey = Instantiate(redKeyPrefab, keyPosition, Quaternion.identity);
        }
        else
        {
            throw new ArgumentException();
        }
        
        _currentKeyObjectData.Add(coordinates, (keyType, newKey));
    }

    public GridManager.KeyType GrabKeyOnCoordinates(Vector2Int coordinates)
    {
        var (keyType, keyObject) = _currentKeyObjectData[coordinates];
        Destroy(keyObject);
        _currentKeyObjectData.Remove(coordinates);
        return keyType;
    }

    public void ToggleStateChartRunState()
    {
        if (_stateChartRunner.IsRunning)
        {
            currentStateIndicator.gameObject.SetActive(false);
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
                uiManager.SwitchToProgramView();
            }
            else
            {
                currentStateIndicator.gameObject.SetActive(true);
                _stateChartRunner.StartRun(stateChartManager.GetStateChart());
                StateChartRunnerStateChanged?.Invoke(true);
                uiManager.SwitchLevelView();
            }
        }
    }

    private void ReloadLevel()
    {
        Destroy(_stateChartRunner.gameObject);
        LoadLevelGrid(LevelDataStorage.GetLevelData(_currentLevelId));
    }

    public void LoadNextLevel()
    {
        Destroy(_stateChartRunner.gameObject);
        StateChartRunnerStateChanged?.Invoke(false);
        currentStateIndicator.gameObject.SetActive(false);
        _currentLevelId++;
        PlayerPrefs.SetInt("CurrentLevelId", _currentLevelId);
        LoadLevel(_currentLevelId);
    }

    private void LoadLevel(int levelId)
    {
        var level = LevelDataStorage.GetLevelData(levelId);
        LoadLevelGrid(level);

        // Setup StateChart
        stateChartManager.ResetStateChart();
        uiManager.SetupUIForLevel(level.AvailableActions, level.AvailableTransitionConditions);
    }

    private void LoadLevelGrid(LevelData level)
    {
        gridManager.CreateLevelBasedOnGrid(level.Grid);
        var tileRenderers = gridManager.GetTileObjectRenderers();
        cameraController.AlignCameraWithLevel(tileRenderers);
        var robotStartPositionOnGrid = gridManager.GetTilePosition(level.RobotStartPosition);
        _stateChartRunner = Instantiate(robotStateChartRunnerPrefab, robotStartPositionOnGrid, Quaternion.identity);
        _stateChartRunner.SetStartCoordinates(level.RobotStartPosition, level.RobotStartDirection);

        _currentKeyObjectData = new Dictionary<Vector2Int, (GridManager.KeyType, GameObject)>();
        foreach (var (keyCoordinates, keyType) in level.KeyData)
        {
            DropKeyOnCoordinates(keyCoordinates, keyType);
        }
    }
}