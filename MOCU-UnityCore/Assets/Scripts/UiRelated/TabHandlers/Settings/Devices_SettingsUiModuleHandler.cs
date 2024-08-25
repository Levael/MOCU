// TODO: refactor everything //
// ========================= //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using CustomDataStructures;
using CustomUxmlElements;
using AudioControl;
using System.Linq;

using Debug = UnityEngine.Debug;


// add to documentation: the only update this class can get are from outside. it only sends what is wanted to be changed


public class Devices_SettingsUiModuleHandler : MonoBehaviour, IControllableInitiation
{
    public bool IsComponentReady {  get; private set; }

    public InterlinkedCollection<DeviceParametersSet> devicesInterlinkedCollection;         // connects data objects with UI stuff
    private VisualTreeAsset _chooseDeviceRowTemplate;

    private AudioHandler _audioHandler;
    private UiReferences _uiReference;
    private UiHandler _uiHandler;

    private Dictionary<DeviceCardStatus, string> _deviceCardStatusToUssClassNameMap;        // classes for card labels
    private Dictionary<DeviceOptionStatus, string> _deviceOptionStatusToUssClassNameMap;    // classes for options

    private VisualElement _backToMainBtn;
    private VisualElement _openChoosingWindowBtn;
    private VisualElement _currentlyOpenedDeviceBox;



    public void ControllableAwake()
    {
        _chooseDeviceRowTemplate = Resources.Load<VisualTreeAsset>("GUI/UXML/SettingsModules/DeviceOptionModule");


        _deviceOptionStatusToUssClassNameMap = new()
        {
            {DeviceOptionStatus.FreeToChoose,       "ready-to-use-device"},
            {DeviceOptionStatus.CurrentlyChosen,    "current-device"},
            {DeviceOptionStatus.AlreadyChosen,      "already-chosen-device"},
        };
    }

    public void ControllableStart()
    {
        _uiHandler = GetComponent<UiHandler>();
        _audioHandler = GetComponent<AudioHandler>();

        _uiReference = _uiHandler.secondaryUiScreen;
        _backToMainBtn = _uiReference.elements.settingsTab.devicesModule.backToMainBtn;
        _openChoosingWindowBtn = _uiReference.elements.settingsTab.devicesModule.choosingWindow;
        _currentlyOpenedDeviceBox = null;

        DeviceParametersSet audioDevice_InputResearcher = null;
        DeviceParametersSet audioDevice_InputParticipant = null;
        DeviceParametersSet audioDevice_OutputResearcher = null;
        DeviceParametersSet audioDevice_OutputParticipant = null;

        audioDevice_InputResearcher = new DeviceParametersSet {
            uiElement               = _uiReference.elements.settingsTab.devicesModule.microphoneResearcher,
            deviceStatus            = DeviceCardStatus.NotChosen,
            deviceType              = DeviceType.AudioInput,
            allowMultipleDevices    = false,

            chosenDeviceName        = null,
            chosenDeviceVolume      = null,
            listOfOptions           = null,

            GetDeviceName           = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioInputDeviceName_Researcher,
            GetDeviceVolume         = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioInputDeviceVolume_Researcher,
            GetDevicesList          = (data) => AssignStatusesToListOfOptions(((UnifiedAudioDataPacket)data).inputAudioDevices, audioDevice_InputResearcher),

            SetDeviceName = (newName) =>
            {
                var data = _audioHandler.GetAudioData().audioDevicesInfo;
                data.audioInputDeviceName_Researcher = newName;
                _audioHandler.SetAudioData(data);
            },
            SetDeviceVolume = (newVolume) =>
            {
                var data = _audioHandler.GetAudioData().audioDevicesInfo;
                data.audioInputDeviceVolume_Researcher = newVolume;
                _audioHandler.SetAudioData(data);
            },
        };

        audioDevice_InputParticipant = new DeviceParametersSet {
            uiElement               = _uiReference.elements.settingsTab.devicesModule.microphoneParticipant,
            deviceStatus            = DeviceCardStatus.NotChosen,
            deviceType              = DeviceType.AudioInput,
            allowMultipleDevices    = false,

            chosenDeviceName        = null,
            chosenDeviceVolume      = null,
            listOfOptions           = null,

            GetDeviceName           = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioInputDeviceName_Participant,
            GetDeviceVolume         = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioInputDeviceVolume_Participant,
            GetDevicesList          = (data) => AssignStatusesToListOfOptions(((UnifiedAudioDataPacket)data).inputAudioDevices, audioDevice_InputParticipant),

            SetDeviceName = (newName) =>
            {
                var data = _audioHandler.GetAudioData().audioDevicesInfo;
                data.audioInputDeviceName_Participant = newName;
                _audioHandler.SetAudioData(data);
            },
            SetDeviceVolume = (newVolume) =>
            {
                var data = _audioHandler.GetAudioData().audioDevicesInfo;
                data.audioInputDeviceVolume_Participant = newVolume;
                _audioHandler.SetAudioData(data);
            },
        };

        audioDevice_OutputResearcher = new DeviceParametersSet {
            uiElement               = _uiReference.elements.settingsTab.devicesModule.speakerResearcher,
            deviceStatus            = DeviceCardStatus.NotChosen,
            deviceType              = DeviceType.AudioOutput,
            allowMultipleDevices    = false,

            chosenDeviceName        = null,
            chosenDeviceVolume      = null,
            listOfOptions           = null,

            GetDeviceName           = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioOutputDeviceName_Researcher,
            GetDeviceVolume         = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioOutputDeviceVolume_Researcher,
            GetDevicesList          = (data) => AssignStatusesToListOfOptions(((UnifiedAudioDataPacket)data).outputAudioDevices, audioDevice_OutputResearcher),

            SetDeviceName = (newName) =>
            {
                var data = _audioHandler.GetAudioData().audioDevicesInfo;
                data.audioOutputDeviceName_Researcher = newName;
                _audioHandler.SetAudioData(data);
            },
            SetDeviceVolume = (newVolume) =>
            {
                var data = _audioHandler.GetAudioData().audioDevicesInfo;
                data.audioOutputDeviceVolume_Researcher = newVolume;
                _audioHandler.SetAudioData(data);
            },
        };

        audioDevice_OutputParticipant = new DeviceParametersSet
        {
            uiElement = _uiReference.elements.settingsTab.devicesModule.speakerParticipant,
            deviceStatus = DeviceCardStatus.NotChosen,
            deviceType = DeviceType.AudioOutput,
            allowMultipleDevices = false,

            chosenDeviceName = null,
            chosenDeviceVolume = null,
            listOfOptions = null,

            GetDeviceName = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioOutputDeviceName_Participant,
            GetDeviceVolume = (data) => ((UnifiedAudioDataPacket)data).audioDevicesInfo.audioOutputDeviceVolume_Participant,
            GetDevicesList = (data) => AssignStatusesToListOfOptions(((UnifiedAudioDataPacket)data).outputAudioDevices, audioDevice_OutputParticipant),

            SetDeviceName = (newName) =>
            {
                var data = _audioHandler.GetAudioData().audioDevicesInfo;
                data.audioOutputDeviceName_Participant = newName;
                _audioHandler.SetAudioData(data);
            },
            SetDeviceVolume = (newVolume) =>
            {
                var data = _audioHandler.GetAudioData().audioDevicesInfo;
                data.audioOutputDeviceVolume_Participant = newVolume;
                _audioHandler.SetAudioData(data);
            },
        };


        devicesInterlinkedCollection = new()
        {
            audioDevice_InputResearcher,
            audioDevice_InputParticipant,
            audioDevice_OutputResearcher,
            audioDevice_OutputParticipant,
        };

        SomeStuff_RenameLater();
        IsComponentReady = true;
    }


    private void SomeStuff_RenameLater()
    {
        foreach (var device in devicesInterlinkedCollection)
        {
            var deviceCardUiElement = device.uiElement;

            deviceCardUiElement.pickingMode = PickingMode.Position;
            deviceCardUiElement.RegisterCallback<ClickEvent>(eventObj => DeviceBoxClicked(eventObj));

            var customSlider = deviceCardUiElement.Q<CustomSlider>();
            if (customSlider != null)
                customSlider.RegisterCallback<ClickEvent>(eventObj => DeviceVolumeSliderClicked(eventObj));
        }

        _backToMainBtn.RegisterCallback<ClickEvent>(CloseDeviceBoxParameters);
    }

    public void ApplyAudioChanges(UnifiedAudioDataPacket parameters)
    {
        UpdateAudioDevicesData(parameters);
        UpdateAudioDevicesUi();
    }

    private void UpdateAudioDevicesData(UnifiedAudioDataPacket parameters)
    {
        try
        {
            foreach (var device in devicesInterlinkedCollection)
            {
                var chosenDeviceName    = device.GetDeviceName(parameters);
                var chosenDeviceVolume  = device.GetDeviceVolume(parameters) ?? 0f;
                var deviceStatus        = (!String.IsNullOrEmpty(chosenDeviceName)) ? DeviceCardStatus.Ready : DeviceCardStatus.NotChosen;

                devicesInterlinkedCollection.UpdateSingleValue(device.uiElement, "chosenDeviceName", chosenDeviceName);
                devicesInterlinkedCollection.UpdateSingleValue(device.uiElement, "chosenDeviceVolume", chosenDeviceVolume);
                devicesInterlinkedCollection.UpdateSingleValue(device.uiElement, "deviceStatus", deviceStatus);
            }

            // must be after after previuos 'foreach' cause it depends on it 
            foreach (var device in devicesInterlinkedCollection)
            {
                var listOfOptions = device.GetDevicesList(parameters);
                devicesInterlinkedCollection.UpdateSingleValue(device.uiElement, "listOfOptions", listOfOptions);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in UpdateAudioDevicesData: {ex}");
        }
    }
    
    private void UpdateAudioDevicesUi()
    {
        try
        {
            foreach (var device in devicesInterlinkedCollection)
            {
                device.uiElement.SetStatus(device.deviceStatus);
                device.uiElement.SliderValue = (float)device.chosenDeviceVolume;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in UpdateAudioDevicesUi: {ex}");
        }
    }

    private IReadOnlyList<(string deviceName, DeviceOptionStatus status)> AssignStatusesToListOfOptions (IReadOnlyList<string> list, DeviceParametersSet currentDevice)
    {
        var result = new List<(string deviceName, DeviceOptionStatus status)>();

        foreach (var option in list)
        {
            var currentOption = (deviceName: option, status: DeviceOptionStatus.FreeToChoose);

            if (option == currentDevice.chosenDeviceName)
            {
                currentOption.status = DeviceOptionStatus.CurrentlyChosen;
                result.Add(currentOption);
                continue;
            }

            foreach (var device in devicesInterlinkedCollection)
            {
                if (device == currentDevice)
                    continue;

                if (option == device.chosenDeviceName)
                {
                    currentOption.status = DeviceOptionStatus.AlreadyChosen;
                    break;
                }
            }

            result.Add(currentOption);
        }

        return result;
    }


    private void WeelSoundChange(WheelEvent evt)
    {
        var parentName = ((VisualElement)evt.currentTarget).name;
        var slider = _uiReference.GetElement(parentName).Q<CustomSlider>();
        slider.value += evt.delta.y > 0 ? -2 : +2;
    }

    private void DeviceBoxClicked(ClickEvent eventObj)
    {
        Debug.Log("Device box clicked");

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

        var slider = eventObj.currentTarget as CustomSlider;
        var device = devicesInterlinkedCollection[slider.parent];
        device.SetDeviceVolume(slider.value);
    }

    private void OpenDeviceBoxParameters(VisualElement clickedDeviceBox)
    {
        _currentlyOpenedDeviceBox = clickedDeviceBox;

        foreach (var deviceBox in _uiReference.GetDevicesBoxes())   // hide every device box except clicked one
        {
            if (deviceBox == clickedDeviceBox) continue;
            deviceBox.style.display = DisplayStyle.None;
        }

        _openChoosingWindowBtn.style.display = DisplayStyle.Flex;   // show device parameters window
        _backToMainBtn.style.visibility = Visibility.Visible;       // show close btn

        var slider = clickedDeviceBox.Q<CustomSlider>();
        if (!slider.ClassListContains("hidden-slider")) slider.style.visibility = Visibility.Visible;

        FillDeviceOptions(devicesInterlinkedCollection[clickedDeviceBox]);
    }

    private void FillDeviceOptions(DeviceParametersSet deviceParametersSet)
    {
        var devicesList = ((VisualElement)_uiReference.elements.settingsTab.devicesModule.choosingWindow).Q<ScrollView>();
        devicesList.Clear();

        foreach (var option in deviceParametersSet.listOfOptions)
        {
            var instance = _chooseDeviceRowTemplate.CloneTree();
            instance.Q<TextElement>(className: "device-option-full-name").text = option.deviceName;

            // Bell btn visibility (for output audio devices only)
            if (deviceParametersSet.deviceType == DeviceType.AudioOutput)
            {
                var bellIcon = instance.Q<VisualElement>(className: "device-option-bell-btn");
                bellIcon.RegisterCallback<ClickEvent>(SendTestSoundToAudioOutputDevice);
                bellIcon.style.visibility = Visibility.Visible;
            }

            instance.AddToClassList(_deviceOptionStatusToUssClassNameMap[option.status]);

            instance.Q<VisualElement>(className: "device-option-full-name").RegisterCallback<ClickEvent>(DeviceOptionOnClick);
            
            devicesList.Add(instance);
        }
    }

    private void CloseDeviceBoxParameters(ClickEvent clickEvent = null)
    {
        _currentlyOpenedDeviceBox = null;

        // unhide every device box
        foreach (var deviceBox in _uiReference.GetDevicesBoxes())
        {
            deviceBox.style.display = DisplayStyle.Flex;

            // if it's a speaker -- show its volume slider
            var slider = deviceBox.Q<CustomSlider>();
            if (slider != null) slider.style.visibility = StyleKeyword.Null;    // resets to what it was before

            // can't use "DisplayStyle.None;" because it makes style inline (and it's stronger than uss and slider will be always hidden)
        }

        // hide device parameters window
        _openChoosingWindowBtn.style.display = DisplayStyle.None;
        // hide close btn
        _backToMainBtn.style.visibility = Visibility.Hidden;
    }

    

    private void SendTestSoundToAudioOutputDevice(ClickEvent clickEvent)
    {
        var outputDeviceName = ((VisualElement)clickEvent.currentTarget).parent.parent.Q<TextElement>(className: "device-option-full-name").text;
        _audioHandler.SendTestAudioSignalToDevice(audioOutputDeviceName: outputDeviceName);
    }


    private void DeviceOptionOnClick(ClickEvent clickEvent)
    {
        if (_currentlyOpenedDeviceBox == null)
        {
            print("error in 'DeviceOptionOnClick'. '_currentlyOpenedDeviceBox' can't be 'null'");
            return;
        }

        var optionName = ((TextElement)clickEvent.currentTarget).text;
        var device = devicesInterlinkedCollection[_currentlyOpenedDeviceBox];
        var optionStatus = device.listOfOptions.FirstOrDefault(option => option.deviceName == optionName).status;

        if (optionStatus == DeviceOptionStatus.CurrentlyChosen)
            device.SetDeviceName(null);
        else if (optionStatus == DeviceOptionStatus.AlreadyChosen)
        {
            foreach (var thatDevice in devicesInterlinkedCollection)
            {
                if (thatDevice.chosenDeviceName == optionName)
                {
                    thatDevice.SetDeviceName(null);
                    device.SetDeviceName(optionName);
                    break;
                }
            }
        }
        else if (optionStatus == DeviceOptionStatus.FreeToChoose)
            device.SetDeviceName(optionName);
        else
            print("you should not see that message");

        CloseDeviceBoxParameters();
    }
}




public class DeviceParametersSet
{
    [CanBeKey(true)]
    public SettingsDeviceBox uiElement { get; set; }

    [CanBeKey(true)]
    public string? chosenDeviceName { get; set; }
    
    
    
    [CanBeKey(false)]
    public Func<object, string?> GetDeviceName { get; set; }
    
    [CanBeKey(false)]
    public Func<object, float?> GetDeviceVolume { get; set; }
    
    [CanBeKey(false)]
    public Func<object, IReadOnlyList<(string deviceName, DeviceOptionStatus status)>?> GetDevicesList { get; set; }



    [CanBeKey(false)]
    public Action<string?> SetDeviceName { get; set; }

    [CanBeKey(false)]
    public Action<float?> SetDeviceVolume { get; set; }



    [CanBeKey(false)]
    public float? chosenDeviceVolume { get; set; }

    [CanBeKey(false)]
    public IReadOnlyList<(string deviceName, DeviceOptionStatus status)>? listOfOptions { get; set; }

    [CanBeKey(false)]
    public DeviceCardStatus deviceStatus { get; set; }
    
    [CanBeKey(false)]
    public DeviceType deviceType { get; set; }

    [CanBeKey(false)]
    public bool allowMultipleDevices { get; set; }
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