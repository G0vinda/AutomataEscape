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
    [SerializeField] private EventReference atmoLevelEvent;
    [SerializeField] private EventReference cableConnectEvent;
    [SerializeField] private EventReference cableHoldEvent;
    [SerializeField] private EventReference cableStartEvent;
    [SerializeField] private EventReference cableReleaseEvent;
    
    public static SoundPlayer Instance;

    private EventInstance _cableHoldInstance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void PlayButtonClick()
    {
        RuntimeManager.PlayOneShot(buttonClickEvent);
    }

    public void PlayAtmoLevel()
    {
        RuntimeManager.PlayOneShot(atmoLevelEvent);
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
