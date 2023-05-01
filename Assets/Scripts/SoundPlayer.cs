using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private EventReference buttonClickEvent;
    
    [SerializeField] private EventReference cableConnectEvent;
    [SerializeField] private EventReference cableHoldEvent;
    [SerializeField] private EventReference cableStartEvent;
    [SerializeField] private EventReference cableReleaseEvent;
    
    [SerializeField] private EventReference atmoLevelEvent;
    [SerializeField] private EventReference musicLevelEvent;
    
    public static SoundPlayer Instance;

    private EventInstance _cableHoldInstance;
    private EventInstance _atmoLevelInstance;
    private EventInstance _musicLevelInstance;

    private const string WalkStateParameterName = "Walkstate";
    private const string IdleLabelName = "Idle";
    private const string WalkingLabelName = "Walking";
    private const string GoalReachedLabelName = "Goal Reached";

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region OnEnable/OnDisable
    private void OnEnable()
    {
        GameManager.Instance.RobotStateChanged += HandleRobotStateChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.RobotStateChanged += HandleRobotStateChanged;
    }
    #endregion

    private void HandleRobotStateChanged(bool startsWalking)
    {
        var newLabelName = startsWalking ? WalkingLabelName : IdleLabelName;
        Debug.Log(newLabelName);
        
        _musicLevelInstance.setParameterByNameWithLabel(WalkStateParameterName, newLabelName);
    }

    private void Start() // Todo: Change me later
    {
        //PlayAtmoLevel();
    }

    public void PlayButtonClick()
    {
        RuntimeManager.PlayOneShot(buttonClickEvent);
    }

    public void PlayAtmoLevel()
    {
        _atmoLevelInstance = RuntimeManager.CreateInstance(atmoLevelEvent);
        _atmoLevelInstance.start();
    }

    public void PlayMusicLevel()
    {
        _musicLevelInstance.stop(STOP_MODE.IMMEDIATE);
        _musicLevelInstance = RuntimeManager.CreateInstance(musicLevelEvent);
        _musicLevelInstance.start();
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

}
