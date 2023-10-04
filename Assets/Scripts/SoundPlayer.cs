using System;
using FMOD.Studio;
using FMODUnity;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private float defaultMusicVolume;
    [SerializeField] private float defaultSfxVolume;
    
    [SerializeField] private EventReference buttonClickEvent;
    [SerializeField] private EventReference buttonClickMenuEvent;

    [SerializeField] private EventReference unlockLevelEvent;
    
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
    [SerializeField] private EventReference RunStateChange;
    
    [SerializeField] private EventReference RobotSpawnEvent;
    [SerializeField] private EventReference RobotDespawnEvent;
    [SerializeField] private EventReference RobotTurnOnEvent;
    [SerializeField] private EventReference RobotTurnOffEvent;
    [SerializeField] private EventReference RobotWalkEvent;
    [SerializeField] private EventReference RobotTurnEvent;
    [SerializeField] private EventReference RobotHitWallEvent;
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

    private Bus Music;
    private Bus SFX;

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
            Music = RuntimeManager.GetBus("bus:/MusicMix");
            SFX = RuntimeManager.GetBus("bus:/Sounds");
            
            UpdateMusicVolume(PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume));
            UpdateSfxVolume(PlayerPrefs.GetFloat("SfxVolume", defaultSfxVolume));
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

    public void UpdateSfxVolume(float newVolume)
    {
        SFX.setVolume(newVolume);
    }

    public void UpdateMusicVolume(float newVolume)
    {
        Music.setVolume(newVolume);
    }

    public float GetSfxVolume()
    {
        SFX.getVolume(out var volume);
        return volume;
    }

    public float GetMusicVolume()
    {
        Music.getVolume(out var volume);
        return volume;
    }

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
            case 1: // this is only for test purposes
                PlayMusicLevel();
                PlayAtmoLevel();
                break;
        }
    }

    public void SetLevelMusicVolume(float volume)
    {
        _musicInstance.setVolume(volume);
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
        if(_musicInstance.hasHandle())
            return;
        _musicInstance = RuntimeManager.CreateInstance(musicMenuEvent);
        _musicInstance.start();
    }
    
    public void PlayAtmoLevel()
    {
        if(_atmoInstance.hasHandle())
            return;
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
        _musicInstance.clearHandle();
    }

    public void StopAtmoLevel()
    {
        _atmoInstance.stop(STOP_MODE.IMMEDIATE);
        _atmoInstance.clearHandle();
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
        RuntimeManager.PlayOneShot(cableSelectEvent); 
    }

    public void PlayBeamSpawn()
    {
        RuntimeManager.PlayOneShot(RobotSpawnEvent);   
    }
    
    public void PlayBeamDespawn()
    {
        RuntimeManager.PlayOneShot(RobotDespawnEvent);   
    }
    
    public void PlayBeamTeleport()
    {
        RuntimeManager.PlayOneShot(RobotTeleportEvent);
    }

    public void PlayRunStateChange()
    {
        RuntimeManager.PlayOneShot(RunStateChange);
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

    public void PlayLevelUnlock()
    {
        RuntimeManager.PlayOneShot(unlockLevelEvent);
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

    public void PlayRobotHitWall()                          
    {
        RuntimeManager.PlayOneShot(RobotHitWallEvent);
    }
    
    public void PlayOpenGate()
    {
        RuntimeManager.PlayOneShot(RobotOpenGateEvent);
    }
    
    public void PlayEnemyMove()                            
    {
        RuntimeManager.PlayOneShot(EnemyWalkEvent);
    }
    
}
