using AudioControl;
using CustomDataStructures;
using CustomUxmlElements;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsTabHandler : MonoBehaviour
{
    // FIELDS
    private UiHandler _uiHandler;
    private AudioHandler _audioHandler;
    private UiReferences _uiReference;
    private DevicesUiModuleHandler _devicesUiModuleHandler;
    public bool classIsReady = false;

    

    // MANDATORY STANDARD FUNCTIONALITY

    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        _audioHandler = GetComponent<AudioHandler>();
        _devicesUiModuleHandler = GetComponent<DevicesUiModuleHandler>();
    }

    void Start()
    {
        _uiReference = _uiHandler.secondaryUiScreen;
        AddEventListeners();
        classIsReady = true;
    }

    void Update()
    {
    }

    // CUSTOM FUNCTIONALITY


    private void AddEventListeners()
    {
    }

    public void UpdateAudioDevices(AudioDataCrossClassesPacket parameters)
    {
        _devicesUiModuleHandler.UpdateAudioDevices(parameters);
    }
}