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
    
    public void PlayCableConnect()
    {
        RuntimeManager.PlayOneShot(cableConnectEvent);
    }
    
}
