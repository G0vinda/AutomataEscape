using System;
using Helper;
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
    [SerializeField] private GameObject keyObject;
    [SerializeField] private CurrentStateIndicator currentStateIndicator;
    [SerializeField] private bool resetSaveSystemOnStart;

    public static GameManager Instance { get; private set; }
    public Action<bool> StateChartRunnerStateChanged;
    
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
        if(resetSaveSystemOnStart)
            PlayerPrefs.SetInt("CurrentLevelId", 0);
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

    public void ToggleStateChartView()
    {
        uiManager.gameObject.SetActive(!uiManager.gameObject.activeSelf);
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
            }
            else
            {
                currentStateIndicator.gameObject.SetActive(true);
                _stateChartRunner.StartRun(stateChartManager.GetStateChart());
                StateChartRunnerStateChanged?.Invoke(true);   
            }
        }
    }

    private void ReloadLevel()
    {
        Destroy(_stateChartRunner.gameObject);
        LoadLevelGrid(LevelStorage.GetLevelData(_currentLevelId));
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
        var level = LevelStorage.GetLevelData(levelId);
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