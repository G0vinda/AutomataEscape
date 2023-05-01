using System;
using System.Collections.Generic;
using LevelGrid;
using Robot;
using UI;
using UI.Transition;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)] // Game Manager will be executed before all other scripts
public class GameManager : MonoBehaviour
{
    [SerializeField] private LevelGridManager levelGridManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Robot.Robot robotPrefab;
    [SerializeField] private GameObject redKeyPrefab;
    [SerializeField] private GameObject blueKeyPrefab;
    [SerializeField] private CurrentStateIndicator currentStateIndicator;

    public static GameManager Instance { get; private set; }
    public Action<bool> RobotStateChanged;
    
    private const int FinishSceneIndex = 2;
    
    private Dictionary<Vector2Int, (LevelGridManager.KeyType, GameObject)> _currentKeyObjectData = new ();
    private int _currentLevelId;
    private Robot.Robot _robot;
    private StateChartManager _stateChartManager;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        uiManager.Initialize();
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

    public LevelGridManager GetLevelGridManager()
    {
        return levelGridManager;
    }

    public bool IsKeyOnCoordinates(Vector2Int coordinates)
    {
        return _currentKeyObjectData.ContainsKey(coordinates);
    }

    public void DropKeyOnCoordinates(Vector2Int coordinates, LevelGridManager.KeyType keyType)
    {
        if(levelGridManager.UnlockGateWithKeyIfPossible(coordinates, keyType))
            return;
        
        GameObject newKey;   
        var keyPosition = levelGridManager.GetTilePosition(coordinates);
        if (keyType == LevelGridManager.KeyType.Blue)
        {
            newKey = Instantiate(blueKeyPrefab, keyPosition, Quaternion.identity);
        }
        else if (keyType == LevelGridManager.KeyType.Red)
        {
            newKey = Instantiate(redKeyPrefab, keyPosition, Quaternion.identity);
        }
        else
        {
            throw new ArgumentException();
        }
        
        _currentKeyObjectData.Add(coordinates, (keyType, newKey));
    }

    public LevelGridManager.KeyType GrabKeyOnCoordinates(Vector2Int coordinates)
    {
        var (keyType, keyObject) = _currentKeyObjectData[coordinates];
        Destroy(keyObject);
        _currentKeyObjectData.Remove(coordinates);
        return keyType;
    }

    public void ToggleRobotRunState()
    {
        if (_robot.IsRunning)
        {
            currentStateIndicator.gameObject.SetActive(false);
            _robot.StopRun();
            RobotStateChanged?.Invoke(false);
            ReloadLevel(); 
        }
        else
        {
            if (!_stateChartManager.CheckIfStatesAreConnected())
            {
                uiManager.SwitchToProgramView();
            }
            else
            {
                currentStateIndicator.gameObject.SetActive(true);
                _robot.StartRun();
                RobotStateChanged?.Invoke(true);
                uiManager.SwitchToLevelView();
            }
        }
    }

    private void ReloadLevel()
    {
        foreach (var keyValuePair in _currentKeyObjectData)
        {
            var keyObject = keyValuePair.Value.Item2;
            Destroy(keyObject);
        }

        LoadLevelGrid(LevelDataStorage.GetLevelData(_currentLevelId));
    }

    public void LoadNextLevel()
    {
        Destroy(_robot.gameObject);
        RobotStateChanged?.Invoke(false);
        currentStateIndicator.gameObject.SetActive(false);
        
        _currentLevelId++;
        if (_currentLevelId >= LevelDataStorage.LevelCount)
            SceneManager.LoadScene(FinishSceneIndex);
        
        PlayerPrefs.SetInt("CurrentLevelId", _currentLevelId);
        LoadLevel(_currentLevelId);
    }

    private void LoadLevel(int levelId)
    {
        _robot = Instantiate(robotPrefab);
        _stateChartManager = _robot.GetComponent<StateChartManager>();
        SoundPlayer.Instance.PlayMusicLevel();
        
        var level = LevelDataStorage.GetLevelData(levelId);
        LoadLevelGrid(level);

        TransitionLineDrawer.ResetColors();
        uiManager.SetupUIForLevel(level.AvailableActions, level.AvailableTransitionConditions, _stateChartManager);
    }

    private void LoadLevelGrid(LevelData level)
    {
        levelGridManager.CreateLevelGrid(level.Grid);
        var tileRenderers = levelGridManager.GetTileSpriteRenderers();
        cameraController.AlignCameraWithLevel(tileRenderers);
        
        var robotStartPositionOnGrid = levelGridManager.GetTilePosition(level.RobotStartPosition);
        _robot.transform.position = robotStartPositionOnGrid;
        _robot.Initialize(level.RobotStartPosition, level.RobotStartDirection);

        _currentKeyObjectData = new Dictionary<Vector2Int, (LevelGridManager.KeyType, GameObject)>();
        foreach (var (keyCoordinates, keyType) in level.KeyData)
        {
            DropKeyOnCoordinates(keyCoordinates, keyType);
        }
    }
}