using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using CustomDataStructures;
using CustomUxmlElements;
using AudioControl;
using System.Security.Cryptography;
using System.Linq;


// add to documentation: the only update this class can get are from outside. it only sends what is wanted to be changed


public class Devices_SettingsUiModuleHandler : MonoBehaviour
{
    public InterlinkedCollection<DeviceParametersSet> devicesInterlinkedCollection;         // connects data objects with UI stuff
    public VisualTreeAsset chooseDeviceRowTemplate;

    private AudioHandler _audioHandler;
    private UiReferences _uiReference;
    private UiHandler _uiHandler;

    private Dictionary<DeviceCardStatus, string> _deviceCardStatusToUssClassNameMap;        // classes for card labels
    private Dictionary<DeviceOptionStatus, string> _deviceOptionStatusToUssClassNameMap;    // classes for options

    private VisualElement _backToMainBtn;
    private VisualElement _openChoosingWindowBtn;



    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        _audioHandler = GetComponent<AudioHandler>();

        // connects UiElementName, DeviceName, DeviceStatus and DeviceOptions
        devicesInterlinkedCollection = new()
        {
            // Input Researcher
            new DeviceParametersSet{
                uiElementName       = "settings-device-box-microphone-researcher",
                deviceStatus        = DeviceCardStatus.NotChosen,
                deviceType          = DeviceType.AudioInput,

                GetDeviceName       = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioInputDeviceName_Researcher,
                GetDeviceVolume     = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioInputDeviceVolume_Researcher,
                GetDevicesList      = (data) => ((UnifiedAudioDataPacket)data).inputAudioDevices.Select(deviceName => (deviceName, DeviceOptionStatus.FreeToChoose)).ToList(),

                chosenDeviceName    = null,
                chosenDeviceVolume  = null,
                listOfOptions       = null,
            },

            // Input Participant
            new DeviceParametersSet{
                uiElementName       = "settings-device-box-microphone-participant",
                deviceStatus        = DeviceCardStatus.NotChosen,
                deviceType          = DeviceType.AudioInput,

                GetDeviceName       = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioInputDeviceName_Participant,
                GetDeviceVolume     = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioInputDeviceVolume_Participant,
                GetDevicesList      = (data) => ((UnifiedAudioDataPacket)data).inputAudioDevices.Select(deviceName => (deviceName, DeviceOptionStatus.FreeToChoose)).ToList(),

                chosenDeviceName    = null,
                chosenDeviceVolume  = null,
                listOfOptions       = null,
            },

            // Output Researcher
            new DeviceParametersSet{
                uiElementName       = "settings-device-box-speaker-researcher",
                deviceStatus        = DeviceCardStatus.NotChosen,
                deviceType          = DeviceType.AudioOutput,

                GetDeviceName       = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioOutputDeviceName_Researcher,
                GetDeviceVolume     = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioOutputDeviceVolume_Researcher,
                GetDevicesList      = (data) => ((UnifiedAudioDataPacket)data).outputAudioDevices.Select(deviceName => (deviceName, DeviceOptionStatus.FreeToChoose)).ToList(),

                chosenDeviceName    = null,
                chosenDeviceVolume  = null,
                listOfOptions       = null,
            },
        };


        _deviceCardStatusToUssClassNameMap = new()
        {
            {DeviceCardStatus.Ready,        "deviceCardIsReady"},
            {DeviceCardStatus.Disabled,     "deviceCardIsDisabled"},
            {DeviceCardStatus.Connecting,   "deviceCardIsConnecting"},
            {DeviceCardStatus.NotChosen,    "deviceCardIsNotChosen"},
            {DeviceCardStatus.Error,        "deviceCardHasError"},
        };


        _deviceOptionStatusToUssClassNameMap = new()
        {
            {DeviceOptionStatus.FreeToChoose,       "ready-to-use-device"},
            {DeviceOptionStatus.CurrentlyChosen,    "current-device"},
            {DeviceOptionStatus.AlreadyChosen,      "already-chosen-device"},
        };
    }

    void Start()
    {
        _uiReference = _uiHandler.secondaryUiScreen;
        _backToMainBtn = _uiReference.elements.settingsTab.devicesModule.backToMainBtn;
        _openChoosingWindowBtn = _uiReference.elements.settingsTab.devicesModule.choosingWindow;

        AddEventListeners();
    }


    private void AddEventListeners()
    {
        foreach (var device in devicesInterlinkedCollection)
        {
            var deviceCardUiElement = _uiReference.GetElement(device.uiElementName);

            deviceCardUiElement.pickingMode = PickingMode.Position;
            deviceCardUiElement.RegisterCallback<ClickEvent>(eventObj => DeviceBoxClicked(eventObj));

            var customSlider = deviceCardUiElement.Q<CustomSlider>();
            if (customSlider != null)
            {
                customSlider.RegisterCallback<ClickEvent>(eventObj => DeviceVolumeSliderClicked(eventObj));
            }
        }

        _backToMainBtn.RegisterCallback<ClickEvent>(CloseDeviceBoxParameters);
    }

    public void ApplyChanges(UnifiedAudioDataPacket parameters)
    {
        UpdateAudioDevicesData(parameters);
        UpdateAudioDevicesUi();
    }

    //private void UpdateDevices(ClickEvent clickEvent) { }

    // Locally
    private void UpdateAudioDevicesData(UnifiedAudioDataPacket parameters)
    {
        try
        {
            foreach (var device in devicesInterlinkedCollection)
            {
                // get data from 'UnifiedAudioDataPacket'
                var chosenDeviceName    = device.GetDeviceName(parameters);
                var chosenDeviceVolume  = device.GetDeviceVolume(parameters) ?? 0f;
                var listOfOptions       = device.GetDevicesList(parameters);
                var deviceStatus        = (!String.IsNullOrEmpty(chosenDeviceName)) ? DeviceCardStatus.Ready : DeviceCardStatus.NotChosen;

                //update data in 'devicesInterlinkedCollection'
                devicesInterlinkedCollection.UpdateSingleValue(device.uiElementName, "chosenDeviceName", chosenDeviceName);
                devicesInterlinkedCollection.UpdateSingleValue(device.uiElementName, "chosenDeviceVolume", chosenDeviceVolume);
                devicesInterlinkedCollection.UpdateSingleValue(device.uiElementName, "deviceStatus", deviceStatus);

                // todo: update default values of status with calculated ones   <-- HERE
                devicesInterlinkedCollection.UpdateSingleValue(device.uiElementName, "listOfOptions", listOfOptions);

                //print($"device.uiElementName: {device.uiElementName} == {devicesInterlinkedCollection[device.uiElementName].deviceStatus}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in UpdateAudioDevicesData: {ex}");
        }
    }
    
    private void UpdateAudioDevicesUi()
    {
        /*try
        {
            foreach (var device in devicesInterlinkedCollection)
            {
                // update ui
                _uiReference.GetElement(device.uiElementName).Q<CustomSlider>().value = (float)chosenDeviceVolume;
                if (String.IsNullOrEmpty(chosenDeviceName))
                    SetDeviceCardStatus(device, DeviceCardStatus.NotChosen);
                if (device.listOfOptions == null || device.listOfOptions.Count == 0)
                    SetDeviceCardStatus(device, DeviceCardStatus.Error);
                else
                    SetDeviceCardStatus(device, DeviceCardStatus.Ready);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in UpdateAudioDevicesUi: {ex}");
        }*/
    }
    

    // clear and refill with options (and set their statuses)
    private void RemakeDeviceOptionsModule(DeviceParametersSet deviceParametersSet)
    {

    }

    // in DeviceModule to show if a device doesn't work properly (label color)
    private void SetDeviceCardStatus(DeviceParametersSet deviceParametersSet, DeviceCardStatus status)
    {

    }

    // in DeviceOptionsModule to differ between currently chosen device and chosen by someone else (colorful dot before line)
    private void SetDeviceOptionStatus(DeviceParametersSet deviceParametersSet, DeviceOptionStatus status)
    {

    }

    private void WeelSoundChange(WheelEvent evt)
    {
        var parentName = ((VisualElement)evt.currentTarget).name;
        var slider = _uiReference.GetElement(parentName).Q<CustomSlider>();
        slider.value += evt.delta.y > 0 ? -2 : +2;
    }

    private void DeviceBoxClicked(ClickEvent eventObj)
    {
        UnityEngine.Debug.Log("Device box clicked");

        // if clicked on Slider -- ignore "OpenDeviceBoxParameters" action
        var currentElement = eventObj.target as VisualElement;

        while (currentElement != null)
        {
            if (currentElement.name == "settings-devices-module-window") break; // upper bound
            // todo: take care of it in "DeviceVolumeSliderClicked"
            /*if (currentElement is Slider slider)
            {
                //Debug.Log($"Pressed on slider. The value is {slider.value},\n{currentElement.parent.name}\n");
                var name = currentElement.parent.name;  // name of device box (like "settings-device-box-speaker-participant")
                _audioHandler.ChangeAudioDeviceVolume(chosenDeviceName: name, volume: slider.value);
                return;
            }*/
            currentElement = currentElement.parent;
        }

        // if device parameters already opened (todo: maybe change it later)
        if (_openChoosingWindowBtn.style.display == DisplayStyle.Flex) return;

        OpenDeviceBoxParameters((VisualElement)eventObj.currentTarget);
    }

    private void DeviceVolumeSliderClicked(ClickEvent eventObj)
    {
        // If cliked on slider than there is no need to call device box event
        eventObj.StopPropagation();

        var currentElement = eventObj.target as VisualElement;
        UnityEngine.Debug.Log("Slider clicked");
        // todo: continue working on it
    }

    private void OpenDeviceBoxParameters(VisualElement clickedDeviceBox)
    {
        foreach (var deviceBox in _uiReference.GetDevicesBoxes())   // hide every device box except clicked one
        {
            if (deviceBox == clickedDeviceBox) continue;
            deviceBox.style.display = DisplayStyle.None;
        }

        _openChoosingWindowBtn.style.display = DisplayStyle.Flex;   // show device parameters window
        _backToMainBtn.style.display = DisplayStyle.Flex;           // show close btn

        var slider = clickedDeviceBox.Q<CustomUxmlElements.CustomSlider>();
        if (slider != null) slider.style.display = DisplayStyle.Flex;

        FillDeviceOptions(devicesInterlinkedCollection[clickedDeviceBox.name]);
    }

    private void FillDeviceOptions(DeviceParametersSet deviceParametersSet)
    {
        var devicesList = _uiReference.GetElement("settings-devices-choose-device-window").Q<ScrollView>();
        devicesList.Clear();

        foreach (var option in deviceParametersSet.listOfOptions)
        {
            var instance = chooseDeviceRowTemplate.CloneTree();
            instance.Q<TextElement>(className: "device-option-full-name").text = option.deviceName;

            // Bell btn visibility (for output audio devices only)
            if (deviceParametersSet.deviceType == DeviceType.AudioOutput)
            {
                var bellIcon = instance.Q<VisualElement>(className: "device-option-bell-btn");
                bellIcon.RegisterCallback<ClickEvent>(SendTestSoundToAudioOutputDevice);
                bellIcon.style.visibility = Visibility.Visible;
            }

            // Disconnect btn visibility
            /*DeviceOptionStatus optionStatus = DeviceOptionStatus.FreeToChoose;
            if (deviceParametersSet.chosenDeviceName == option.deviceName)
                optionStatus = DeviceOptionStatus.CurrentlyChosen;
            foreach (var device in devicesInterlinkedCollection)
            {
                if (option.deviceName == device.chosenDeviceName && device.chosenDeviceName != deviceParametersSet.chosenDeviceName)
                    option.status = DeviceOptionStatus.AlreadyChosen;
            }*/

            instance.AddToClassList(_deviceOptionStatusToUssClassNameMap[option.status]);

            instance.Q<VisualElement>(className: "device-option-full-name").RegisterCallback<ClickEvent>(ChooseThisDevice);
            instance.Q<VisualElement>(className: "device-option-disconnect-btn").RegisterCallback<ClickEvent>(DisconnectDevice);
            
            devicesList.Add(instance);
        }
    }

    private void CloseDeviceBoxParameters(ClickEvent clickEvent)
    {
        // clears the data if will be open again


        // unhide every device box
        foreach (var deviceBox in _uiReference.GetDevicesBoxes())
        {
            deviceBox.style.display = DisplayStyle.Flex;

            // if it's a speaker -- show its volume slider
            var slider = deviceBox.Q<CustomUxmlElements.CustomSlider>();
            if (slider != null) slider.style.display = StyleKeyword.Null;
            // can't use "DisplayStyle.None;" because it makes style inline (and it's stronger than uss and slider will be always hidden)
        }

        // hide device parameters window
        _openChoosingWindowBtn.style.display = DisplayStyle.None;
        // hide close btn
        _backToMainBtn.style.display = DisplayStyle.None;
    }

    

    private void SendTestSoundToAudioOutputDevice(ClickEvent clickEvent)
    {
        var outputDeviceName = ((VisualElement)clickEvent.currentTarget).parent.parent.Q<TextElement>(className: "device-option-full-name").text;
        _audioHandler.SendTestAudioSignalToDevice(audioOutputDeviceName: outputDeviceName);
    }

    private void DisconnectDevice(ClickEvent clickEvent)
    {
        // 'path' look at 'DeviceOptionModule.uxml'
        var deviceName = ((VisualElement)clickEvent.currentTarget).parent.parent.Q<TextElement>(className: "device-option-full-name").text;

        var deviceParametersSet = devicesInterlinkedCollection[deviceName];
        deviceParametersSet.chosenDeviceName = null;

        // todo: send "update" to 'AudioHandler'

        Debug.Log($"close btn {((VisualElement)clickEvent.currentTarget).parent.parent.Q<TextElement>(className: "device-option-full-name").text}; deviceName: {deviceName}");
    }

    private void ChooseThisDevice(ClickEvent clickEvent)
    {
        var deviceName = ((TextElement)clickEvent.currentTarget).text;
        Debug.Log($"Func: ChooseThisDevice; deviceName: {deviceName}");
    }
}




public class DeviceParametersSet
{
    [CanBeKey(true)]
    public string uiElementName { get; set; }

    [CanBeKey(true)]
    public string? chosenDeviceName { get; set; }
    
    
    
    [CanBeKey(false)]
    public Func<object, string?> GetDeviceName { get; set; }
    
    [CanBeKey(false)]
    public Func<object, float?> GetDeviceVolume { get; set; }
    
    [CanBeKey(false)]
    public Func<object, List<(string deviceName, DeviceOptionStatus status)>> GetDevicesList { get; set; }




    [CanBeKey(false)]
    public float? chosenDeviceVolume { get; set; }

    [CanBeKey(false)]
    public List<(string deviceName, DeviceOptionStatus status)>? listOfOptions { get; set; }

    [CanBeKey(false)]
    public DeviceCardStatus deviceStatus { get; set; }
    
    [CanBeKey(false)]
    public DeviceType deviceType { get; set; }
}

public enum DeviceCardStatus
{
    Ready,              // is ok and can be used
    Disabled,           // can't be chosen
    Connecting,         // in the process of connecting
    NotChosen,          // when the value is null
    Error,              // failed to connect or disconnected during runtime (for any reason)
    Warning             // If sound is too high or smth like that
}

public enum DeviceOptionStatus
{
    FreeToChoose,       // Device is available to be chosen
    CurrentlyChosen,    // Device is currently chosen by this object
    AlreadyChosen       // Device is already chosen by another object
}

public enum DeviceType
{
    AudioOutput,
    AudioInput,
    Camera,
    Controller
}