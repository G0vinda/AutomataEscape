using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LevelGrid;
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
    [SerializeField] private LevelInfoText levelInfoText;
    [SerializeField] private Image levelFadeImage;
    [SerializeField] private Color levelFadeColor;
    [SerializeField] private float levelFadeTime;
    [SerializeField] private float levelBeamTime;
    [SerializeField] private float portalBeamTime;

    public static GameManager Instance { get; private set; }
    public static event Action<bool> RobotStateChanged;
    public static event Action InvalidRunPress;
    public static event Action<float> BeamRobotIn;
    public static event Action<float> BeamRobotOut;

    private const int MenuSceneIndex = 0;
    private const int LevelSelectionSceneIndex = 1;
    private const int FinishSceneIndex = 4;
    
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

    public int GetCurrentLevelId()
    {
        return _currentLevelId;
    }

    public void ReturnToMainMenu()
    {
        SoundPlayer.Instance.StopMusic();
        SoundPlayer.Instance.StopAtmoLevel();
        SoundPlayer.Instance.PlayMusicMenu();
        SceneManager.LoadScene(MenuSceneIndex);
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

    public LevelGridManager.KeyType GetKeyTypeOnCoordinates(Vector2Int coordinates)
    {
        if (!_currentKeyObjectData.ContainsKey(coordinates))
            return LevelGridManager.KeyType.None;
        
        var (keyType, _) = _currentKeyObjectData[coordinates];
        return keyType;
    }

    public void GrabKeyOnCoordinates(Vector2Int coordinates)
    {
        var (_, keyObject) = _currentKeyObjectData[coordinates];
        Destroy(keyObject);
        _currentKeyObjectData.Remove(coordinates);
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
            if (!_stateChartManager.CheckIfStartStateIsConnected())
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
        uiManager.ResetInGameButtons();
        LoadLevelGrid(level);
        PositionEnemiesInLevel(level);
        PositionRobotInLevel(level, true);
    }

    private void FinishLevel()
    {
        if (_currentLevelId == LevelDataStorage.LevelCount - 1)
        {
            SceneManager.LoadScene(FinishSceneIndex);
            return;
        }

        _currentLevelId++;
        SoundPlayer.Instance.StopMusic();
        PlayerPrefs.SetInt("CurrentLevelId", _currentLevelId);
        var reachedLevel = PlayerPrefs.GetInt("ReachedLevelId", 0);
        if (_currentLevelId > reachedLevel)
        {
            PlayerPrefs.SetInt("ReachedLevelId", _currentLevelId);
            PlayerPrefs.SetString("ReachedNewLevel", true.ToString());
        }
        
        
        SceneManager.LoadScene(LevelSelectionSceneIndex);
    }

    private IEnumerator LoadLevel(int levelId)
    {
        SoundPlayer.Instance.PlayMusicLevel();
        uiManager.ResetInGameButtons();
        RobotStateChanged?.Invoke(false);
        
        var level = LevelDataStorage.GetLevelData(levelId);
        LoadLevelGrid(level);
        PositionEnemiesInLevel(level);

        FadeLevelIn();

        var levelWaitTime = levelFadeTime - 0.4f;
        yield return new WaitForSeconds(levelWaitTime);
        
        _robot = Instantiate(robotPrefab);
        BeamRobotIn?.Invoke(levelBeamTime);
        _stateChartManager = _robot.GetComponent<StateChartManager>();
        PositionRobotInLevel(level);
        levelInfoText.ShowLevelInfo(levelId);

        TransitionLineDrawer.ResetColors();
        uiManager.SetupUIForLevel(level.AvailableActions, level.AvailableTransitionConditions, _stateChartManager);
    }

    private void PositionRobotInLevel(LevelData level, bool reset = false)
    {
        var robotStartPositionOnGrid = levelGridManager.GetTilePosition(level.RobotStartPosition);
        _robot.transform.position = robotStartPositionOnGrid;
        if (reset)
            _robot.ResetPosition(level.RobotStartPosition, level.RobotStartDirection, _enemiesInLevel);
        else
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
            value => SoundPlayer.Instance.SetLevelMusicVolume(value)));
        fadeSequence.SetEase(Ease.InCirc);
    }

    private void FadeLevelOut()
    {
        var transparent = levelFadeImage.color;
        var fadeSequence = DOTween.Sequence();
        fadeSequence.Append(DOVirtual.Color(transparent, levelFadeColor, levelFadeTime,
            value => levelFadeImage.color = value));
        fadeSequence.Join(DOVirtual.Float(1, 0, levelFadeTime,
            value => SoundPlayer.Instance.SetLevelMusicVolume(value)));
        fadeSequence.SetEase(Ease.InCirc).OnComplete(FinishLevel);
    }
    
    public void ReachGoal()
    {
        SoundPlayer.Instance.PlayVictory();
        uiManager.HideInGameButtons();
        currentStateIndicator.gameObject.SetActive(false);
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
        SoundPlayer.Instance.PlayBeamTeleport();
        yield return new WaitForSeconds(portalBeamTime);
        
        _robot.transform.position = levelGridManager.GetTilePosition(destinationCoordinates);
        _robot.SetCoordinates(destinationCoordinates);
        var destinationPortalAnimator = levelGridManager.GetTileOnCoordinates(destinationCoordinates)
            .GetComponent<PortalTileAnimator>();
        destinationPortalAnimator.PlayReverseTeleport();
        BeamRobotIn?.Invoke(portalBeamTime);
    }

    public void CheckForEnemyCollision()
    {
        StartCoroutine(DelayedCollisionCheck());
    }

    private IEnumerator DelayedCollisionCheck()
    {
        if (_enemiesInLevel == null)
            yield break;
        
        var collisionCheckDelay = 0.3f;
        yield return new WaitForSeconds(collisionCheckDelay);

        var robotCoordinates = _robot.GetCoordinates();
        foreach (var enemy in _enemiesInLevel)
        {
            if (enemy.GetCoordinates() == robotCoordinates)
            {
                SoundPlayer.Instance.PlayRobotGotCaughtSound();
                SoundPlayer.Instance.PlayEnemyAlarmSoundLoop();
                currentStateIndicator.gameObject.SetActive(false);
                uiManager.HideInGameButtons();
                _robot.StopRun();
                enemy.StartAlarm();
                RobotStateChanged?.Invoke(false);
                var catchPosition = levelGridManager.GetTilePosition(robotCoordinates);
                StartCoroutine(EnemyCatchEffect(catchPosition));
            }
        }
    }

    private IEnumerator EnemyCatchEffect(Vector3 catchTilePosition)
    {
        var zoomTime = 1f;
        cameraController.ZoomCameraToTilePosition(catchTilePosition,zoomTime);
        yield return new WaitForSeconds(zoomTime + 2f);
        SoundPlayer.Instance.StopEnemyAlarmSoundLoop();
        
        ReloadLevel();
    }
}