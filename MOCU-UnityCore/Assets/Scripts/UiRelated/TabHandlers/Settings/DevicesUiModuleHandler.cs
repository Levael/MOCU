using CustomDataStructures;
using CustomUxmlElements;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class DevicesUiModuleHandler : MonoBehaviour
{
    public InterlinkedCollection<DeviceParametersSet> devicesInterlinkedCollection;         // connects data objects with UI stuff
    public VisualTreeAsset chooseDeviceRowTemplate;

    private AudioHandler _audioHandler;
    private UiReferences _uiReference;
    private UiHandler _uiHandler;

    private Dictionary<DeviceCardStatus, string> _deviceCardStatusToUssClassNameMap;        // classes for card labels
    private Dictionary<DeviceOptionStatus, string> _deviceOptionStatusToUssClassNameMap;    // classes for options



    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        _audioHandler = GetComponent<AudioHandler>();


        devicesInterlinkedCollection = new()
        {
            // Input Researcher
            new DeviceParametersSet{
                uiElementName = "settings-device-box-microphone-researcher",
                chosenDeviceName = null,
                chosenDeviceVolume = null,
                dataObjectField_deviceName = "audioInputDeviceName_Researcher",
                dataObjectField_deviceVolume = "audioInputDeviceVolume_Researcher",
                listOfOptions = _audioHandler.inputAudioDevices,
                isEnabled = true
            },

            // Input Participant
            new DeviceParametersSet{
                uiElementName = "settings-device-box-microphone-participant",
                chosenDeviceName = null,
                chosenDeviceVolume = null,
                dataObjectField_deviceName = "audioInputDeviceName_Participant",
                dataObjectField_deviceVolume = "audioInputDeviceVolume_Participant",
                listOfOptions = _audioHandler.inputAudioDevices,
                isEnabled = true
            },

            // Output Researcher
            new DeviceParametersSet{
                uiElementName = "settings-device-box-speaker-researcher",
                chosenDeviceName = null,
                chosenDeviceVolume = null,
                dataObjectField_deviceName = "audioOutputDeviceName_Researcher",
                dataObjectField_deviceVolume = "audioOutputDeviceVolume_Researcher",
                listOfOptions = _audioHandler.outputAudioDevices,
                isEnabled = true
            },
        };


        _deviceCardStatusToUssClassNameMap = new()
        {
            {DeviceCardStatus.Ready, "deviceCardIsReady"},
            {DeviceCardStatus.Disabled, "deviceCardIsDisabled"},
            {DeviceCardStatus.Connecting, "deviceCardIsConnecting"},
            {DeviceCardStatus.NotChosen, "deviceCardIsNotChosen"},
            {DeviceCardStatus.Error, "deviceCardHasError"},
        };


        _deviceOptionStatusToUssClassNameMap = new()
        {
            {DeviceOptionStatus.FreeToChoose, "deviceOptionIsFreeToChoose"},
            {DeviceOptionStatus.CurrentlyChosen, "deviceOptionIsCurrentlyChosen"},
            {DeviceOptionStatus.AlreadyChosen, "deviceOptionIsAlreadyChosen"},
        };
    }

    void Start()
    {
        _uiReference = _uiHandler.secondaryUiScreen;

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

        _uiReference.GetElement("settings-devices-back-btn").RegisterCallback<ClickEvent>(CloseDeviceBoxParameters);
        _uiReference.GetElement("settings-devices-update-btn").RegisterCallback<ClickEvent>(UpdateDevices);
    }

    public void UpdateDevicesCards()
    {
        try
        {
            foreach (var device in devicesInterlinkedCollection)
            {
                // get data from AudioHandler
                var chosenDeviceName = _audioHandler.GetAudioDeviceName(device.dataObjectField_deviceName);
                var chosenDeviceVolume = _audioHandler.GetAudioDeviceVolume(device.dataObjectField_deviceVolume) ?? 0f; // if null -> 0

                //update data in 'devicesInterlinkedCollection'
                devicesInterlinkedCollection.UpdateSingleValue(device.uiElementName, "chosenDeviceName", chosenDeviceName);
                devicesInterlinkedCollection.UpdateSingleValue(device.uiElementName, "chosenDeviceVolume", chosenDeviceVolume);

                // update ui
                _uiReference.GetElement(device.uiElementName).Q<CustomSlider>().value = (float)chosenDeviceVolume;
                // todo: color label if chosenDeviceName == null            <-- HERE

                // debug log console
                //print($"chosenDeviceName: {chosenDeviceName}. device.chosenDeviceName: {device.chosenDeviceName}");
                //print($"chosenDeviceVolume: {chosenDeviceVolume}. device.chosenDeviceVolume: {device.chosenDeviceVolume}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in UpdateDevicesCards: {ex}");
        }
    }

    // clear and refill with options (and set their statuses)
    private void RemakeDeviceOptionsModule(DeviceParametersSet deviceParametersSet)
    {

    }

    // in DeviceModule to show if a device doesn't work properly (label color)
    private void SetDeviceCardStatus(string uiElementName, DeviceCardStatus status)
    {

    }

    // in DeviceOptionsModule to differ between currently chosen device and chosen by someone else (colorful dot before line)
    private void SetDeviceOptionStatus(VisualElement uiElement, DeviceOptionStatus status)
    {

    }

    private void WeelSoundChange(WheelEvent evt)
    {
        var parentName = ((VisualElement)evt.currentTarget).name;
        var slider = _uiReference.GetElement(parentName).Q<CustomUxmlElements.CustomSlider>();
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
        if (_uiReference.GetElement("settings-devices-choose-device-window").style.display == DisplayStyle.Flex) return;

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
        foreach (var deviceBox in _uiReference.GetDevicesBoxes())                                             // hide every device box except clicked one
        {
            if (deviceBox == clickedDeviceBox) continue;
            deviceBox.style.display = DisplayStyle.None;
        }

        _uiReference.GetElement("settings-devices-choose-device-window").style.display = DisplayStyle.Flex;   // show device parameters window
        _uiReference.GetElement("settings-devices-back-btn").style.display = DisplayStyle.Flex;               // show close btn
        _uiReference.GetElement("settings-devices-update-btn").style.display = DisplayStyle.Flex;             // show upd btn

        // if it's a speaker -- show its volume slider (not only while :hover)
        var slider = clickedDeviceBox.Q<CustomUxmlElements.CustomSlider>();
        if (slider != null) slider.style.display = DisplayStyle.Flex;
        // not here; slider.value = 10;

        // local root
        var devicesList = _uiReference.GetElement("settings-devices-choose-device-window").Q<ScrollView>();

        // Clear devices list
        devicesList.Clear();

        // Fill devices list
        for (int i = 0; i < 1; i++)
        {
            // "audio-output" class is to show "bell icon"

            // change later: check if it speaker (currently mic passes too)
            var instance = chooseDeviceRowTemplate.CloneTree();
            instance.AddToClassList("current-device");
            instance.Q<TextElement>(className: "device-option-left-part-text").text = $"Device 1";
            instance.Q<TextElement>(className: "device-option-full-name").text = $"Speakers (Realtek High Definition Audio)";
            instance.Q<VisualElement>(className: "bell-btn").RegisterCallback<ClickEvent>(SendTestSoundToAudioOutputDevice);
            instance.Q<VisualElement>(className: "close-btn").RegisterCallback<ClickEvent>(DisconnectDevice);
            devicesList.Add(instance);

            var instance2 = chooseDeviceRowTemplate.CloneTree();
            instance2.AddToClassList("already-chosen-device");
            instance2.Q<TextElement>(className: "device-option-left-part-text").text = $"Device 2";
            instance2.Q<TextElement>(className: "device-option-full-name").text = $"Headphones (Rift Audio)";
            instance2.Q<VisualElement>(className: "bell-btn").RegisterCallback<ClickEvent>(SendTestSoundToAudioOutputDevice);
            instance2.Q<VisualElement>(className: "close-btn").RegisterCallback<ClickEvent>(DisconnectDevice);
            devicesList.Add(instance2);
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
        _uiReference.GetElement("settings-devices-choose-device-window").style.display = DisplayStyle.None;
        // hide close btn
        _uiReference.GetElement("settings-devices-back-btn").style.display = DisplayStyle.None;
        // hide upd btn
        _uiReference.GetElement("settings-devices-update-btn").style.display = DisplayStyle.None;
    }

    // todo: change name to 'load (down)' or smth else
    private void UpdateDevices(ClickEvent clickEvent) { }

    private void SendTestSoundToAudioOutputDevice(ClickEvent clickEvent)
    {
        var outputDeviceName = ((VisualElement)clickEvent.currentTarget).parent.parent.Q<TextElement>(className: "device-option-full-name").text;
        _audioHandler.SendTestAudioSignalToDevice(audioOutputDeviceName: outputDeviceName);
    }

    private void DisconnectDevice(ClickEvent clickEvent)
    {
        Debug.Log($"close btn {((VisualElement)clickEvent.currentTarget).parent.parent.Q<TextElement>(className: "device-option-full-name").text}");
    }
}




public class DeviceParametersSet
{
    [CanBeKey(true)]
    public string uiElementName { get; set; }

    [CanBeKey(true)]
    public string? chosenDeviceName { get; set; }

    [CanBeKey(true)]
    public string dataObjectField_deviceName { get; set; }

    [CanBeKey(true)]
    public string? dataObjectField_deviceVolume { get; set; }


    [CanBeKey(false)]
    public float? chosenDeviceVolume { get; set; }

    [CanBeKey(false)]
    public List<string>? listOfOptions { get; set; }

    [CanBeKey(false)]
    public bool? isEnabled { get; set; }
}

public enum DeviceCardStatus
{
    Ready,              // is ok and can be used
    Disabled,           // can't be chosen
    Connecting,         // in the process of connecting
    NotChosen,          // when the value is null
    Error               // failed to connect or disconnected during runtime (for any reason)
}

public enum DeviceOptionStatus
{
    FreeToChoose,       // Device is available to be chosen
    CurrentlyChosen,    // Device is currently chosen by this object
    AlreadyChosen       // Device is already chosen by another object
}