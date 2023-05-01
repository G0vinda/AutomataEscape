using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private EventReference buttonClickEvent; 
    [SerializeField] private EventReference atmoLevelEvent;
    [SerializeField] private EventReference cableConnectEvent;
    [SerializeField] private EventReference cableHoldEvent;
    [SerializeField] private EventReference cableStartEvent;
    [SerializeField] private EventReference cableReleaseEvent;
    
    public static SoundPlayer Instance;

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
        // Stop cable hold sound here
    }

    private void StartCableHold()
    {
        RuntimeManager.PlayOneShot(cableHoldEvent);
    }

    public void PlayCableRelease()
    {
        RuntimeManager.PlayOneShot(cableReleaseEvent);
        // Stop cable hold sound here
    }

}
