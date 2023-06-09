using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LevelGrid;
using log4net.Appender;
using Robot;
using UI;
using UI.Transition;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private Enemy.Enemy enemyPrefab;
    [SerializeField] private CurrentStateIndicator currentStateIndicator;
    [SerializeField] private Image levelFadeImage;
    [SerializeField] private Color levelFadeColor;
    [SerializeField] private float levelFadeTime;
    [SerializeField] private int startLevelId;
    [SerializeField] private bool resetLevelOnStart;
    [SerializeField] private float levelBeamTime;
    [SerializeField] private float portalBeamTime;

    public static GameManager Instance { get; private set; }
    public static event Action<bool> RobotStateChanged;
    public static event Action InvalidRunPress;
    public static event Action<float> BeamRobotIn;
    public static event Action<float> BeamRobotOut;
     
    
    private const int FinishSceneIndex = 2;
    
    private Dictionary<Vector2Int, (LevelGridManager.KeyType, GameObject)> _currentKeyObjectData = new ();
    private Vector2Int[] _currentPortalCoordinates;
    private int _currentLevelId;
    private Robot.Robot _robot;
    private List<Enemy.Enemy> _enemiesInLevel = new ();
    private StateChartManager _stateChartManager;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        uiManager.Initialize();
        if(resetLevelOnStart)
            PlayerPrefs.SetInt("CurrentLevelId", startLevelId);
        _currentLevelId = PlayerPrefs.GetInt("CurrentLevelId", 0);
        
        SoundPlayer.Instance.PlayAtmoLevel();
        SoundPlayer.Instance.PlayMusicLevel();
        StartCoroutine(LoadLevel(_currentLevelId));
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
            SoundPlayer.Instance.PlayMusicLevel();
        }
        else
        {
            if (!_stateChartManager.CheckIfStatesAreConnected())
            {
                uiManager.SwitchToProgramView();
                SoundPlayer.Instance.PlayRunError();
                InvalidRunPress?.Invoke();
            }
            else
            {
                SoundPlayer.Instance.PlayRunStart();
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

        var level = LevelDataStorage.GetLevelData(_currentLevelId);
        LoadLevelGrid(level);
        PositionEnemiesInLevel(level);
        PositionRobotInLevel(level);
    }

    private void LoadNextLevel()
    {
        Destroy(_robot.gameObject);
        RobotStateChanged?.Invoke(false);
        currentStateIndicator.gameObject.SetActive(false);
        
        _currentLevelId++;
        if (_currentLevelId >= LevelDataStorage.LevelCount)
            SceneManager.LoadScene(FinishSceneIndex);
        
        PlayerPrefs.SetInt("CurrentLevelId", _currentLevelId);
        StartCoroutine(LoadLevel(_currentLevelId));
    }

    private IEnumerator LoadLevel(int levelId)
    {
        SoundPlayer.Instance.PlayMusicLevel();
        
        var level = LevelDataStorage.GetLevelData(levelId);
        LoadLevelGrid(level);

        FadeLevelIn();

        var levelWaitTime = levelFadeTime - 0.4f;
        yield return new WaitForSeconds(levelWaitTime);
        
        _robot = Instantiate(robotPrefab);
        BeamRobotIn?.Invoke(levelBeamTime);
        _stateChartManager = _robot.GetComponent<StateChartManager>();
        PositionEnemiesInLevel(level);
        PositionRobotInLevel(level);

        TransitionLineDrawer.ResetColors();
        uiManager.SetupUIForLevel(level.AvailableActions, level.AvailableTransitionConditions, _stateChartManager);
    }

    private void PositionRobotInLevel(LevelData level)
    {
        var robotStartPositionOnGrid = levelGridManager.GetTilePosition(level.RobotStartPosition);
        _robot.transform.position = robotStartPositionOnGrid;
        _robot.Initialize(level.RobotStartPosition, level.RobotStartDirection, _enemiesInLevel);
    }

    private void PositionEnemiesInLevel(LevelData level)
    {
        if (_enemiesInLevel.Count > 0)
        {
            foreach (var enemy in _enemiesInLevel)
            {
                Destroy(enemy.gameObject);
            }
            _enemiesInLevel.Clear();
        }
        
        if(level.EnemyData == null)
            return;
            
        foreach (var (coordinates, direction) in level.EnemyData)
        {
            var enemyPositionOnGrid = levelGridManager.GetTilePosition(coordinates);
            var newEnemy = Instantiate(enemyPrefab, enemyPositionOnGrid, Quaternion.identity);
            newEnemy.Initialize(coordinates, direction, levelGridManager);
            _enemiesInLevel.Add(newEnemy);
        }
    }

    private void LoadLevelGrid(LevelData level)
    {
        levelGridManager.CreateLevelGrid(level.Grid);
        var tileRenderers = levelGridManager.GetTileSpriteRenderers();
        cameraController.AlignCameraWithLevel(tileRenderers);
        
        _currentKeyObjectData = new Dictionary<Vector2Int, (LevelGridManager.KeyType, GameObject)>();
        foreach (var (keyCoordinates, keyType) in level.KeyData)
        {
            DropKeyOnCoordinates(keyCoordinates, keyType);
        }

        _currentPortalCoordinates = level.PortalData;
    }

    private void FadeLevelIn()
    {
        var transparent = levelFadeColor;
        transparent.a = 0;
        var fadeSequence = DOTween.Sequence();
        fadeSequence.Append(DOVirtual.Color(levelFadeColor, transparent, levelFadeTime,
            value => levelFadeImage.color = value));
        fadeSequence.Join(DOVirtual.Float(0, 1, levelFadeTime,
            value => SoundPlayer.Instance.SetLevelBackgroundVolume(value)));
        fadeSequence.SetEase(Ease.InCirc);
    }

    private void FadeLevelOut()
    {
        var transparent = levelFadeImage.color;
        var fadeSequence = DOTween.Sequence();
        fadeSequence.Append(DOVirtual.Color(transparent, levelFadeColor, levelFadeTime,
            value => levelFadeImage.color = value));
        fadeSequence.Join(DOVirtual.Float(1, 0, levelFadeTime,
            value => SoundPlayer.Instance.SetLevelBackgroundVolume(value)));
        fadeSequence.SetEase(Ease.InCirc).OnComplete(LoadNextLevel);
    }
    
    public void ReachGoal()
    {
        SoundPlayer.Instance.PlayVictory();
        BeamRobotOut?.Invoke(levelBeamTime);
        Invoke(nameof(FadeLevelOut), 2.7f);
    }

    public void InitiateMoveThroughPortal(Vector2Int startCoordinates)
    {
        if (_currentPortalCoordinates[0] == startCoordinates)
        {
            StartCoroutine(MoveRobotThroughPortal(_currentPortalCoordinates[1]));
            return;
        }
        
        if (_currentPortalCoordinates[1] == startCoordinates)
        {
            StartCoroutine(MoveRobotThroughPortal(_currentPortalCoordinates[0]));
            return;
        }
        
        throw new ArgumentException("No correct portal was found");
    }

    private IEnumerator MoveRobotThroughPortal(Vector2Int destinationCoordinates)
    {
        BeamRobotOut?.Invoke(portalBeamTime);
        yield return new WaitForSeconds(portalBeamTime);
        
        _robot.transform.position = levelGridManager.GetTilePosition(destinationCoordinates);
        _robot.SetCoordinates(destinationCoordinates);
        BeamRobotIn?.Invoke(portalBeamTime);
    }

    public void CheckForEnemyCollision()
    {
        StartCoroutine(DelayedCollisionCheck());
    }

    private IEnumerator DelayedCollisionCheck()
    {
        var collisionCheckDelay = 0.3f;
        yield return new WaitForSeconds(collisionCheckDelay);
    }
}