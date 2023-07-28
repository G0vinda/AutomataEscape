using FMOD.Studio;
using FMODUnity;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private EventReference buttonClickEvent;
    [SerializeField] private EventReference buttonClickMenuEvent;
    
    [SerializeField] private EventReference cableConnectEvent;
    [SerializeField] private EventReference cableHoldEvent;
    [SerializeField] private EventReference cableStartEvent;
    [SerializeField] private EventReference cableReleaseEvent;
    [SerializeField] private EventReference cableSelectEvent;
    [SerializeField] private EventReference stateDragEvent;
    [SerializeField] private EventReference stateDropEvent;
    [SerializeField] private EventReference ImpossibleActionSFXEvent;

    [SerializeField] private EventReference robotBeamEvent;
    [SerializeField] private EventReference levelStartSuccessEvent;
    [SerializeField] private EventReference levelStartFailEvent;
    [SerializeField] private EventReference VictorySFXEvent;
    [SerializeField] private EventReference SceneSwitchSFXEvent;

    [SerializeField] private EventReference RobotSpawnEvent;
    [SerializeField] private EventReference RobotDespawnEvent;
    [SerializeField] private EventReference RobotTurnOnEvent;
    [SerializeField] private EventReference RobotTurnOffEvent;
    [SerializeField] private EventReference RobotWalkEvent;
    [SerializeField] private EventReference RobotTurnEvent;
    [SerializeField] private EventReference RobotDropEvent;
    [SerializeField] private EventReference RobotGrabEvent;
    [SerializeField] private EventReference RobotTeleportEvent;
    [SerializeField] private EventReference RobotOpenGateEvent;
    [SerializeField] private EventReference EnemyWalkEvent;
    
    [SerializeField] private EventReference musicMenuEvent;
    [SerializeField] private EventReference atmoLevelEvent;
    [SerializeField] private EventReference musicLevelEvent;
    [SerializeField] private EventReference musicWalkingEvent;
    [SerializeField] private EventReference finalMusicEvent;
    
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
        if (Instance == null)
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
        GameManager.RobotStateChanged += HandleRobotStateChanged;
        UIManager.ViewStateChanged += HandleViewStateChanged;
    }

    private void OnDisable()
    {
        GameManager.RobotStateChanged -= HandleRobotStateChanged;
        UIManager.ViewStateChanged -= HandleViewStateChanged;
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

    private void Start()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        switch (currentSceneIndex)
        {
            case 0: 
                PlayMusicMenu();
                break;
            case 1:
                PlayMusicLevel();
                PlayAtmoLevel();
                break;
        }
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
    
    public void PlayButtonClickMenu()
    {
        RuntimeManager.PlayOneShot(buttonClickMenuEvent);
    }


    public void PlayMusicMenu()
    {
        _musicInstance.stop(STOP_MODE.IMMEDIATE);
        _musicInstance = RuntimeManager.CreateInstance(musicMenuEvent);
        _musicInstance.start();
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

    public void StopMusic()
    {
        _musicInstance.stop(STOP_MODE.IMMEDIATE);
    }

    public void StopAtmoLevel()
    {
        _atmoInstance.stop(STOP_MODE.IMMEDIATE);
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

    public void PlayCableSelect()
    {
        RuntimeManager.PlayOneShot(cableSelectEvent); //todo: neuer Methodentrigger für Auswählen des Kabels
    }

    public void PlayBeamSpawn()
    {
        RuntimeManager.PlayOneShot(RobotSpawnEvent);   //todo: neuer Methodentrigger für das Spawnen des Roboters
    }
    
    public void PlayBeamDespawn()
    {
        RuntimeManager.PlayOneShot(RobotDespawnEvent);   //todo: neuer Methodentrigger für das Despawnen des Roboters
    }
    
    public void PlayBeamTeleport()
    {
        RuntimeManager.PlayOneShot(RobotTeleportEvent);   //todo: neuer Methodentrigger fürs Teleportieren durch die Teleportfelder
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
        _musicInstance.stop(STOP_MODE.IMMEDIATE);
        RuntimeManager.PlayOneShot(levelStartSuccessEvent);
    }

    public void PlayRobotStartUp()
    {
        RuntimeManager.PlayOneShot(RobotTurnOnEvent);
    }
    
    public void PlayRobotShutdown()                     
    {
        RuntimeManager.PlayOneShot(RobotTurnOffEvent);
    }

    public void PlayRunError()
    {
        RuntimeManager.PlayOneShot(levelStartFailEvent);
    }

    public void PlayVictory()
    {
        RuntimeManager.PlayOneShot(VictorySFXEvent);
        _musicInstance.stop(STOP_MODE.IMMEDIATE);
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
        RuntimeManager.PlayOneShot(RobotTurnEvent);
    }

    public void PlayOpenGate()
    {
        RuntimeManager.PlayOneShot(RobotOpenGateEvent);
    }
    
    public void PlayEnemyMove()                             //todo: trigger me please
    {
        RuntimeManager.PlayOneShot(EnemyWalkEvent);
    }
    
}
