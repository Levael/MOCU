using UnityEngine;

using AudioControl;


public class SettingsTabHandler : MonoBehaviour
{
    // FIELDS
    private UiHandler _uiHandler;
    private AudioHandler _audioHandler;
    private UiReferences _uiReference;
    private Devices_SettingsUiModuleHandler _devicesUiModuleHandler;
    public bool classIsReady = false;

    

    // MANDATORY STANDARD FUNCTIONALITY

    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        _audioHandler = GetComponent<AudioHandler>();
        _devicesUiModuleHandler = GetComponent<Devices_SettingsUiModuleHandler>();
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

    public void UpdateAudioDevices(UnifiedAudioDataPacket parameters)
    {
        _devicesUiModuleHandler.ApplyChanges(parameters);
    }
}