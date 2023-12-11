using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//TODO: Fix Cedrus. Add controller to numpad. Connect mi2audio

public class UiHandler : MonoBehaviour
{
    public GameObject mainDisplayGameObject;
    public GameObject secondDisplayGameObject;

    private UiReferences _mainDisplayUiReferences;
    private UiReferences _secondDisplayUiReferences;

    private ForTests _forTests;
    private Cedrus _cedrus;
    private AnswerHandler _answerHandler;
    private ConnectionsStatusesHandler _connectionsStatusesHandler;

    private VisualElement _activeTab;       // main display
    private VisualElement _openedTab;       // second display

    public UiReferences mainTabScreen;         // use this (instead of _mainDisplayUiReferences)
    public UiReferences secondaryTabScreen;    // use this (instead of _secondDisplayUiReferences)

    private bool _openTabInSecondDisplay;



    void Awake()
    {
        _mainDisplayUiReferences = mainDisplayGameObject.GetComponent<UiReferences>();
        _secondDisplayUiReferences = secondDisplayGameObject.GetComponent<UiReferences>();

        _forTests = GetComponent<ForTests>();
        _cedrus = GetComponent<Cedrus>();
        _answerHandler = GetComponent<AnswerHandler>();
        _connectionsStatusesHandler = GetComponent<ConnectionsStatusesHandler>();

    }

    void Start()
    {
        ApplyDefaultSettings();
    }

    private void Update()
    {
        PrintToConsole(_forTests.getStats(), clearTextElement: true);
        PrintToInfo($"Cedrus connection: {_cedrus.CedrusConnectionStatus}\nAnswers got: {_answerHandler.answers.Count}", clearTextElement: true);

        /*PrintToWarnings("", clearTextElement: true);
        foreach (var answerStruct in _answerHandler.answers)
        {
            PrintToWarnings($"{answerStruct.answer} - {answerStruct.timestamp.ToBinary()}\n", clearTextElement: false);
        }*/

        _connectionsStatusesHandler.UpdateConnectionStatuses();
    }


    public void PrintToInfo(string message, bool clearTextElement = false) {
        var textElement = (TextElement)mainTabScreen.GetElement("info-module-textbox");
        if (clearTextElement) textElement.text = "";
        textElement.text += message;
    }

    public void PrintToWarnings(string message, bool clearTextElement = false)
    {
        var textElement = (TextElement)mainTabScreen.GetElement("warnings-module-textbox");
        if (clearTextElement) textElement.text = "";
        textElement.text += message;
    }

    public void PrintToConsole(string message, bool clearTextElement = false)
    {
        var textElement = (TextElement)secondaryTabScreen.GetElement("debug-console-module-textbox");
        if (clearTextElement) textElement.text = "";
        textElement.text += message;
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
        _openTabInSecondDisplay = true;

        if (_openTabInSecondDisplay)
        {
            mainTabScreen = _mainDisplayUiReferences;
            secondaryTabScreen = _secondDisplayUiReferences;
        }
        else
        {
            mainTabScreen = secondaryTabScreen = _mainDisplayUiReferences;
        }

        TabHasBeenClicked(mainTabScreen.GetElement("experiment-tab"));
        ShowBody("experiment-tab", mainTabScreen);

        if (_openTabInSecondDisplay) ShowBody("", secondaryTabScreen);

        AddEventListeners();
        //HideElements();
        //UnhideElements();
    }

    private void AddEventListeners()
    {
        // HEADER TABS
        foreach (var tab in _mainDisplayUiReferences.GetHeaderTabs())
        {
            tab.pickingMode = PickingMode.Position;
            tab.RegisterCallback<ClickEvent>(eventObj => { TabHasBeenClicked((VisualElement)eventObj.currentTarget); });
            // adds class to clicked elem (currentTarget, not just target!)
        }

        // EXIT / MINIMIZE GAME
        _mainDisplayUiReferences.GetElement("close-game-btn").RegisterCallback<ClickEvent>(eventObj => { ShowExitConfirmationModalWindow(); });
        _mainDisplayUiReferences.GetElement("minimize-game-btn").RegisterCallback<ClickEvent>(eventObj => { MinimizeGame(); });

        _mainDisplayUiReferences.GetElement("exit-confirm-btn").RegisterCallback<ClickEvent>(eventObj => { ConfirmGameQuit(); });
        _mainDisplayUiReferences.GetElement("exit-cancel-btn").RegisterCallback<ClickEvent>(eventObj => { CancelGameQuit(); });

        _secondDisplayUiReferences.GetElement("minimize-game-btn").RegisterCallback<ClickEvent>(eventObj => { MinimizeSecondDisplay(); });

        // EXPERIMENT tab MAIN BTNS
        // ...

        // EXPERIMENT tab CONTROLLER
        // ...

        // EXPERIMENT tab STATUSES
        // ...

        // EXPERIMENT tab PROTOCOL
        // ...

        // SETTINGS tab DEVICES
        // ...
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

        showedByDefault.Add(_mainDisplayUiReferences.GetElement("experiment-body"));



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
            if (clickedTab == _mainDisplayUiReferences.GetElement("experiment-tab")) ActivateTab(clickedTab);  // main tab is exclusion
            else
            {
                OpeneTab(clickedTab);
                ShowBody(clickedTab.name, _secondDisplayUiReferences);
            }
        }
        else // only one display
        {
            ActivateTab(clickedTab);
            ShowBody(clickedTab.name, _mainDisplayUiReferences);
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
        clickEvent.target = _secondDisplayUiReferences.GetElement("minimize-game-btn");
        _secondDisplayUiReferences.GetElement("minimize-game-btn").SendEvent(clickEvent);
    }

    private void MinimizeSecondDisplay() {
        WinAPI.MinimizeGameWindow();
    }

    private void ShowExitConfirmationModalWindow()
    {
        _mainDisplayUiReferences.GetElement("modal-windows").style.display = DisplayStyle.Flex;
        _mainDisplayUiReferences.GetElement("exit-confirmation-modal-window").style.display = DisplayStyle.Flex;
    }

    private void CloseExitConfirmationModalWindow()
    {
        _mainDisplayUiReferences.GetElement("modal-windows").style.display = DisplayStyle.None;
        _mainDisplayUiReferences.GetElement("exit-confirmation-modal-window").style.display = DisplayStyle.None;
    }

    private void ConfirmGameQuit()
    {
        CloseGame();
    }

    private void CancelGameQuit()
    {
        CloseExitConfirmationModalWindow();
    }

}
