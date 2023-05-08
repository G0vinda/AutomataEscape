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
    
    [SerializeField] private EventReference robotBeamEvent;
    
    [SerializeField] private EventReference atmoLevelEvent;
    [SerializeField] private EventReference musicLevelEvent;
    
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
        Instance = this;
        DontDestroyOnLoad(gameObject);
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

    public void PlayGoalMusic()
    {
        // Switch Music to Goal music
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

}
