using UnityEngine;

using AudioControl;


public class SettingsTabHandler : MonoBehaviour, IControllableInitiation
{
    // FIELDS
    public bool IsComponentReady {  get; private set; }
    private UiHandler _uiHandler;
    private AudioHandler _audioHandler;
    private UiReferences _uiReference;
    private Devices_SettingsUiModuleHandler _devicesUiModuleHandler;

    

    // MANDATORY STANDARD FUNCTIONALITY

    public void ControllableAwake() { }

    public void ControllableStart()
    {
        _uiHandler = GetComponent<UiHandler>();
        _audioHandler = GetComponent<AudioHandler>();
        _devicesUiModuleHandler = GetComponent<Devices_SettingsUiModuleHandler>();

        _uiReference = _uiHandler.secondaryUiScreen;
        AddEventListeners();
        IsComponentReady = true;
    }

    // CUSTOM FUNCTIONALITY


    private void AddEventListeners() { }

    public void UpdateAudioDevices(UnifiedAudioDataPacket parameters)
    {
        _devicesUiModuleHandler.ApplyChanges(parameters);
    }
}