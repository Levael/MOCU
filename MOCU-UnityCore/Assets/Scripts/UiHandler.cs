using Assets.Scripts;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class UiHandler : MonoBehaviour
{
    public GameObject mainDisplayGameObject;
    public GameObject secondDisplayGameObject;

    private UiReferences _mainDisplayUiReferences;
    private UiReferences _secondDisplayUiReferences;

    private AudioHandler _audioHandler;

    private VisualElement _activeTab;           // main display
    private VisualElement _openedTab;           // second display

    public UiReferences mainTabScreen;          // use this (instead of _mainDisplayUiReferences)
    public UiReferences secondaryTabScreen;     // use this (instead of _secondDisplayUiReferences)

    public VisualTreeAsset chooseDeviceRowTemplate;     // TODO: move it to separate class later

    private bool _openTabInSecondDisplay;



    void Awake()
    {
        _mainDisplayUiReferences = mainDisplayGameObject.GetComponent<UiReferences>();
        _secondDisplayUiReferences = secondDisplayGameObject.GetComponent<UiReferences>();

        _audioHandler = GetComponent<AudioHandler>();

        ApplyDefaultSettings();
    }

    void Start()
    {
        AddEventListeners();

        TabHasBeenClicked(mainTabScreen.GetElement("settings-tab"));
    }

    private void Update()
    {
        
    }

    


    public void ControllerButtonWasPressed(string btn_name) {
        mainTabScreen.GetElement(btn_name).AddToClassList("isActive");
    }

    public void ControllerButtonWasReleased(string btn_name)
    {
        mainTabScreen.GetElement(btn_name).RemoveFromClassList("isActive");
    }





    private void ApplyDefaultSettings()
    {
        _openTabInSecondDisplay = true && Display.displays.Length > 1;
        Display.displays[0].Activate();         // activation of main display (without it window isn't fullscreen)


        if (_openTabInSecondDisplay)
        {
            mainTabScreen = _mainDisplayUiReferences;
            secondaryTabScreen = _secondDisplayUiReferences;
            Display.displays[1].Activate();     // activation of second display

            ShowBody("", secondaryTabScreen);
        }
        else
        {
            mainTabScreen = _mainDisplayUiReferences;
            secondaryTabScreen = _mainDisplayUiReferences;

            secondDisplayGameObject.SetActive(false);
            GameObject.Find("SecondMonitorCamera").GetComponent<Camera>().enabled = false;
        }

        
        //TabHasBeenClicked(mainTabScreen.GetElement("experiment-tab"));


        //AddEventListeners();
        //HideElements();
        //UnhideElements();
    }

    private void AddEventListeners()
    {
        // HEADER TABS
        foreach (var tab in mainTabScreen.GetHeaderTabs())
        {
            tab.pickingMode = PickingMode.Position;
            tab.RegisterCallback<ClickEvent>(eventObj => { TabHasBeenClicked((VisualElement)eventObj.currentTarget); });
            // adds class to clicked elem (currentTarget, not just target!)
        }

        // EXIT / MINIMIZE GAME
        mainTabScreen.GetElement("close-game-btn").RegisterCallback<ClickEvent>(eventObj => { ShowExitConfirmationModalWindow(); });
        mainTabScreen.GetElement("minimize-game-btn").RegisterCallback<ClickEvent>(eventObj => { MinimizeGame(); });

        mainTabScreen.GetElement("exit-confirm-btn").RegisterCallback<ClickEvent>(eventObj => { ConfirmGameQuit(); });
        mainTabScreen.GetElement("exit-cancel-btn").RegisterCallback<ClickEvent>(eventObj => { CancelGameQuit(); });

        secondaryTabScreen.GetElement("minimize-game-btn").RegisterCallback<ClickEvent>(eventObj => { MinimizeSecondDisplay(); });


        // EXPERIMENT tab MAIN BTNS
        // ...

        // EXPERIMENT tab CONTROLLER
        // ...

        // EXPERIMENT tab STATUSES
        // ...

        // EXPERIMENT tab PROTOCOL
        // ...

        // SETTINGS tab DEVICES
        secondaryTabScreen.GetElement("settings-device-box-speaker-researcher").RegisterCallback<WheelEvent>(WeelSoundChange);
        secondaryTabScreen.GetElement("settings-device-box-speaker-participant").RegisterCallback<WheelEvent>(WeelSoundChange);

        foreach (var deviceBox in secondaryTabScreen.GetDevicesBoxes())
        {
            deviceBox.pickingMode = PickingMode.Position;
            deviceBox.RegisterCallback<ClickEvent>(eventObj => DeviceBoxClicked(eventObj));
        }

        secondaryTabScreen.GetElement("settings-devices-back-btn").RegisterCallback<ClickEvent>(CloseDeviceBoxParameters);
        secondaryTabScreen.GetElement("settings-devices-update-btn").RegisterCallback<ClickEvent>(UpdateDevices);
    }

    private void HideElements()
    {
        var hiddenByDefault = new List<VisualElement>();

        /*hiddenByDefault.Add(_mainDisplayUiReferences.GetElement("modal-windows"));
        hiddenByDefault.Add(_secondDisplayUiReferences.GetElement("modal-windows"));

        hiddenByDefault.Add(_mainDisplayUiReferences.GetElement("experiment-body-right-part-monitors-eeg"));
        hiddenByDefault.Add(_mainDisplayUiReferences.GetElement("eeg-status-block"));*/

        //hiddenByDefault.Add(_secondDisplayUiReferences.GetElement("close-game-btn"));


        /*foreach (var body in _mainDisplayUiReferences.GetElement("main-body").Children())
        {
            hiddenByDefault.Add(body);
        }

        foreach (var body in _secondDisplayUiReferences.GetElement("main-body").Children())
        {
            hiddenByDefault.Add(body);
        }*/

        /*foreach (var modal in _mainDisplayUiReferences.GetElement("modal-windows").Children())
        {
            hiddenByDefault.Add(modal);
        }

        foreach (var modal in _secondDisplayUiReferences.GetElement("modal-windows").Children())
        {
            hiddenByDefault.Add(modal);
        }

        foreach (var tab in _secondDisplayUiReferences.GetHeaderTabs())
        {
            hiddenByDefault.Add(tab);
        }*/


        foreach (var element in hiddenByDefault)
        {
            element.AddToClassList("hidden");
        }
    }

    private void UnhideElements()
    {
        var showedByDefault = new List<VisualElement>();

        showedByDefault.Add(mainTabScreen.GetElement("experiment-body"));



        foreach (var element in showedByDefault)
        {
            element.RemoveFromClassList("hidden");
        }
    }

    private void TabHasBeenClicked(VisualElement clickedTab)
    {
        if (_openTabInSecondDisplay) WinAPI.RestoreWindow("Unity Secondary Display");
        if (clickedTab == _activeTab || clickedTab == _openedTab) return;


        if (_openTabInSecondDisplay)
        {
            if (clickedTab == mainTabScreen.GetElement("experiment-tab"))
            {
                ActivateTab(clickedTab);
                ShowBody(clickedTab.name, mainTabScreen);
            }
            else
            {
                OpeneTab(clickedTab);
                ShowBody(clickedTab.name, secondaryTabScreen);
            }
        }
        else // only one display
        {
            ActivateTab(clickedTab);
            ShowBody(clickedTab.name, mainTabScreen);
        }
    }

    private void ActivateTab(VisualElement clickedTab)
    {
        // Tab is "Active" if there is only one display
        if (_activeTab != null) _activeTab.RemoveFromClassList("isActive");
        _activeTab = clickedTab;
        _activeTab.AddToClassList("isActive");
    }

    private void OpeneTab(VisualElement clickedTab)
    {
        // Tab is "Opened" if it is opened on second display
        if (_openedTab != null) _openedTab.RemoveFromClassList("isOpened");
        _openedTab = clickedTab;
        _openedTab.AddToClassList("isOpened");
    }

    private void ShowBody(string tabName, UiReferences uiReferences)
    {
        foreach (var body in uiReferences.GetElement("main-body").Children())
        {
            body.style.display = DisplayStyle.None;
        }

        if (tabName == "") return;
        uiReferences.GetElement(uiReferences.GetTabBodyRelation(tabName)).style.display = DisplayStyle.Flex;
    }


    // MODAL WINDOW

    private void CloseGame() {
        #if UNITY_EDITOR    // This code will be executed only in the editor

        UnityEditor.EditorApplication.isPlaying = false;

        #else   // This code will be executed in the assembled game

        Application.Quit();

        #endif
    }

    private void MinimizeGame() {
        // Firstly minimizes main display
        WinAPI.MinimizeGameWindow();

        // And after that -- second display (imitates click on second display btn. unity somehow knows which screen minimize, but can't minimize both automaticaly)
        var clickEvent = new ClickEvent();
        clickEvent.target = secondaryTabScreen.GetElement("minimize-game-btn");
        secondaryTabScreen.GetElement("minimize-game-btn").SendEvent(clickEvent);
    }

    private void MinimizeSecondDisplay() {
        WinAPI.MinimizeGameWindow();
    }

    private void ShowExitConfirmationModalWindow()
    {
        mainTabScreen.GetElement("modal-windows").style.display = DisplayStyle.Flex;
        mainTabScreen.GetElement("exit-confirmation-modal-window").style.display = DisplayStyle.Flex;
    }

    private void CloseExitConfirmationModalWindow()
    {
        mainTabScreen.GetElement("modal-windows").style.display = DisplayStyle.None;
        mainTabScreen.GetElement("exit-confirmation-modal-window").style.display = DisplayStyle.None;
    }

    private void ConfirmGameQuit()
    {
        CloseGame();
    }

    private void CancelGameQuit()
    {
        CloseExitConfirmationModalWindow();
    }


    // Sliders
    private void WeelSoundChange(WheelEvent evt)
    {
        var parentName = ((VisualElement)evt.currentTarget).name;
        var slider = secondaryTabScreen.GetElement(parentName).Q<CustomUxmlElements.CustomSlider>();
        slider.value += evt.delta.y > 0 ? -2 : +2;
    }


    // Devices (settings tab)
    private void DeviceBoxClicked(ClickEvent eventObj)
    {
        // if device parameters already opened
        if (secondaryTabScreen.GetElement("settings-devices-choose-device-window").style.display == DisplayStyle.Flex) return;

        // if clicked on Slider -- ignore "OpenDeviceBoxParameters" action
        var currentElement = eventObj.target as VisualElement;
        while (currentElement != null)
        {
            if (currentElement.name == "settings-devices-module-window") break; // upper bound 
            if (currentElement is Slider) return;
            currentElement = currentElement.parent;
        }

        OpenDeviceBoxParameters((VisualElement)eventObj.currentTarget);
    }

    private void OpenDeviceBoxParameters(VisualElement clickedDeviceBox)
    {
        foreach (var deviceBox in secondaryTabScreen.GetDevicesBoxes())                                             // hide every device box except clicked one
        {
            if (deviceBox == clickedDeviceBox) continue;
            deviceBox.style.display = DisplayStyle.None;
        }
        
        secondaryTabScreen.GetElement("settings-devices-choose-device-window").style.display = DisplayStyle.Flex;   // show device parameters window
        secondaryTabScreen.GetElement("settings-devices-back-btn").style.display = DisplayStyle.Flex;               // show close btn
        secondaryTabScreen.GetElement("settings-devices-update-btn").style.display = DisplayStyle.Flex;             // show upd btn

        // if it's a speaker -- show its volume slider (not only while :hover)
        var slider = clickedDeviceBox.Q<CustomUxmlElements.CustomSlider>();
        if (slider != null) slider.style.display = DisplayStyle.Flex;

        // local root
        var devicesList = secondaryTabScreen.GetElement("settings-devices-choose-device-window").Q<ScrollView>();

        // Clear devices list
        devicesList.Clear();

        // Fill devices list
        for (int i = 0; i < 1; i++)
        {
            // "audio-output" class is to show "bell icon"

            var instance = chooseDeviceRowTemplate.CloneTree();
            if (slider != null)  instance.AddToClassList("audio-output");
            instance.AddToClassList("current-device");
            instance.Q<TextElement>(className: "device-option-left-part-text").text = $"Device 1";
            instance.Q<TextElement>(className: "device-option-full-name").text = $"Speakers (Realtek High Definition Audio)";
            instance.Q<VisualElement>(className: "bell-btn").RegisterCallback<ClickEvent>(SendTestSoundToAudioOutputDevice);
            instance.Q<VisualElement>(className: "close-btn").RegisterCallback<ClickEvent>(DisconnectDevice);
            devicesList.Add(instance);

            var instance2 = chooseDeviceRowTemplate.CloneTree();
            if (slider != null)  instance2.AddToClassList("audio-output");
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
        // unhide every device box
        foreach (var deviceBox in secondaryTabScreen.GetDevicesBoxes())
        {
            deviceBox.style.display = DisplayStyle.Flex;

            // if it's a speaker -- show its volume slider
            var slider = deviceBox.Q<CustomUxmlElements.CustomSlider>();
            if (slider != null) slider.style.display = StyleKeyword.Null;
            // can't use "DisplayStyle.None;" because it makes style inline (and it's stronger than uss and slider will be always hidden)
        }

        // hide device parameters window
        secondaryTabScreen.GetElement("settings-devices-choose-device-window").style.display = DisplayStyle.None;
        // hide close btn
        secondaryTabScreen.GetElement("settings-devices-back-btn").style.display = DisplayStyle.None;
        // hide upd btn
        secondaryTabScreen.GetElement("settings-devices-update-btn").style.display = DisplayStyle.None;
    }

    // TODO later
    private void UpdateDevices(ClickEvent clickEvent) { }

    private void SendTestSoundToAudioOutputDevice(ClickEvent clickEvent)
    {
        var outputDeviceName = ((VisualElement)clickEvent.currentTarget).parent.parent.Q<TextElement>(className: "device-option-full-name").text;
        _audioHandler.SendTestAudioSignalToDevice(audioOutputDeviceName: outputDeviceName);
        Debug.Log($"bell btn: {outputDeviceName}");
    }


    private void DisconnectDevice(ClickEvent clickEvent)
    {
        Debug.Log($"close btn {((VisualElement)clickEvent.currentTarget).parent.parent.Q<TextElement>(className: "device-option-full-name").text}");
    }

}
