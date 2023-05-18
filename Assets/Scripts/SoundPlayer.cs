using FMOD.Studio;
using FMODUnity;
using UI;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private EventReference buttonClickEvent;
    
    [SerializeField] private EventReference cableConnectEvent;
    [SerializeField] private EventReference cableHoldEvent;
    [SerializeField] private EventReference cableStartEvent;
    [SerializeField] private EventReference cableReleaseEvent;
    [SerializeField] private EventReference stateDragEvent;
    [SerializeField] private EventReference stateDropEvent;
    
    [SerializeField] private EventReference robotBeamEvent;
    [SerializeField] private EventReference levelStartSuccessEvent;
    [SerializeField] private EventReference levelStartFailEvent;
    [SerializeField] private EventReference VictorySFXEvent;

    [SerializeField] private EventReference RobotWalkEvent;
    [SerializeField] private EventReference RobotDropEvent;
    [SerializeField] private EventReference RobotGrabEvent;
    
    [SerializeField] private EventReference atmoLevelEvent;
    [SerializeField] private EventReference musicLevelEvent;
    [SerializeField] private EventReference musicWalkingEvent;
    
    public static SoundPlayer Instance;

    private EventInstance _musicInstance;
    private EventInstance _cableHoldInstance;
    private EventInstance _atmoInstance;

    private const string WalkStateParameterName = "Walkstate";
    private const string IdleLabelName = "Idle";
    private const string WalkingLabelName = "Walking";
    private const string GoalReachedLabelName = "Goal Reached";

    private const string ViewStateParameterName = "View";
    private const string ProgramViewLabelName = "ProgramView";
    private const string LevelViewLabelName = "LevelView";

    private void Awake()
    {
        if (Instance != null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region OnEnable/OnDisable
    private void OnEnable()
    {
        GameManager.Instance.RobotStateChanged += HandleRobotStateChanged;
        UIManager.ViewStateChanged += HandleViewStateChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.RobotStateChanged += HandleRobotStateChanged;
    }
    #endregion

    private void HandleRobotStateChanged(bool startsWalking)
    {
        var newLabelName = startsWalking ? WalkingLabelName : IdleLabelName;
        _musicInstance.setParameterByNameWithLabel(WalkStateParameterName, newLabelName);
    }

    private void HandleViewStateChanged(bool isInProgramView)
    {
        var newLabelName = isInProgramView ? ProgramViewLabelName : LevelViewLabelName;
        _atmoInstance.setParameterByNameWithLabel(ViewStateParameterName, newLabelName);
        _musicInstance.setParameterByNameWithLabel(ViewStateParameterName, newLabelName);
    }

    private void Start() // Todo: Change me later
    {
        PlayAtmoLevel();
    }

    public void SetLevelBackgroundVolume(float volume)
    {
        _musicInstance.setVolume(volume);
        _atmoInstance.setVolume(volume);
    }

    public void PlayButtonClick()
    {
        RuntimeManager.PlayOneShot(buttonClickEvent);
    }

    public void PlayAtmoLevel()
    {
        _atmoInstance = RuntimeManager.CreateInstance(atmoLevelEvent);
        _atmoInstance.start();
    }

    public void PlayMusicLevel()
    {
        _musicInstance.stop(STOP_MODE.IMMEDIATE);
        _musicInstance = RuntimeManager.CreateInstance(musicLevelEvent);
        _musicInstance.start();
    }

    public void PlayMusicWalking()
    {
        _musicInstance.stop(STOP_MODE.IMMEDIATE);
        _musicInstance = RuntimeManager.CreateInstance(musicWalkingEvent);
        _musicInstance.start();
    }

    public void PlayCableStart()
    {
        RuntimeManager.PlayOneShot(cableStartEvent);
        StartCableHold();
    }
    
    public void PlayCableConnect()
    {
        RuntimeManager.PlayOneShot(cableConnectEvent);
        _cableHoldInstance.stop(STOP_MODE.IMMEDIATE);
    }

    private void StartCableHold()
    {
        _cableHoldInstance = RuntimeManager.CreateInstance(cableHoldEvent);
        _cableHoldInstance.start();
    }

    public void PlayCableRelease()
    {
        RuntimeManager.PlayOneShot(cableReleaseEvent);
        _cableHoldInstance.stop(STOP_MODE.IMMEDIATE);
    }

    public void PlayBeamSpawn()
    {
        PlayBeam();
    }

    public void PlayBeamDespawn()
    {
        PlayBeam();
    }

    private void PlayBeam()
    {
        RuntimeManager.PlayOneShot(robotBeamEvent);   
    }

    public void PlayStateDragStart()
    {
        RuntimeManager.PlayOneShot(stateDragEvent);
    }

    public void PlayStateDragEnd()
    {
        RuntimeManager.PlayOneShot(stateDropEvent);
    }

    public void PlayRunStart()
    {
        RuntimeManager.PlayOneShot(levelStartSuccessEvent);
    }

    public void PlayRobotStartUp()
    {
        // play the robot start sound that comes after the player presses start
    }

    public void PlayRunError()
    {
        RuntimeManager.PlayOneShot(levelStartFailEvent);
    }

    public void PlayVictory()
    {
        RuntimeManager.PlayOneShot(VictorySFXEvent);
    }

    public void PlayRobotGrab()
    {
        RuntimeManager.PlayOneShot(RobotGrabEvent);
    }

    public void PlayRobotDrop()
    {
        RuntimeManager.PlayOneShot(RobotDropEvent);
    }

    public void PlayRobotMove()
    {
        RuntimeManager.PlayOneShot(RobotWalkEvent);
    }

    public void PlayRobotTurn()
    {
        // play the sound the robot makes, when it turns by 90 deg, no animation time (yet)
    }
}
