using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class UiHandler : MonoBehaviour
{
    public GameObject mainDisplayGameObject;
    public GameObject secondDisplayGameObject;

    private UiReferences _mainDisplayUiReferences;
    private UiReferences _secondDisplayUiReferences;

    private VisualElement _activeTabOnMainUiScreen;
    private VisualElement _activeTabOnSecondaryUiScreen;

    public UiReferences mainUiScreen;          // use this (instead of _mainDisplayUiReferences)
    public UiReferences secondaryUiScreen;     // use this (instead of _secondDisplayUiReferences)

    

    private bool _openTabInSecondDisplay;



    void Awake()
    {
        _mainDisplayUiReferences = mainDisplayGameObject.GetComponent<UiReferences>();
        _secondDisplayUiReferences = secondDisplayGameObject.GetComponent<UiReferences>();

        ApplyDefaultSettings();
    }

    void Start()
    {
        AddEventListeners();

        // default tabs to show (first secondary, than main (in case there is only one monitor))
        TabHasBeenClicked(secondaryUiScreen.GetElement("debug-tab"));
        TabHasBeenClicked(mainUiScreen.GetElement("experiment-tab"));
    }

    private void Update()
    { 
    }






    private void ApplyDefaultSettings()
    {
        _openTabInSecondDisplay = true && Display.displays.Length > 1;
        Display.displays[0].Activate();         // activation of main display (without it window isn't fullscreen)


        if (_openTabInSecondDisplay)
        {
            mainUiScreen = _mainDisplayUiReferences;
            secondaryUiScreen = _secondDisplayUiReferences;
            Display.displays[1].Activate();     // activation of second display

            ShowBody("", secondaryUiScreen);
        }
        else
        {
            mainUiScreen = _mainDisplayUiReferences;
            secondaryUiScreen = _mainDisplayUiReferences;

            secondDisplayGameObject.SetActive(false);
            GameObject.Find("SecondMonitorCamera").GetComponent<Camera>().enabled = false;
        }
    }

    private void AddEventListeners()
    {
        // HEADER TABS
        foreach (var tab in mainUiScreen.GetHeaderTabs())
        {
            tab.pickingMode = PickingMode.Position;
            tab.RegisterCallback<ClickEvent>(eventObj => { TabHasBeenClicked((VisualElement)eventObj.currentTarget); });
            // adds class to clicked elem (currentTarget, not just target!)
        }

        // EXIT / MINIMIZE GAME
        mainUiScreen.GetElement("close-game-btn").RegisterCallback<ClickEvent>(eventObj         => { ShowExitConfirmationModalWindow(); });
        mainUiScreen.GetElement("minimize-game-btn").RegisterCallback<ClickEvent>(eventObj      => { MinimizeGame(); });

        mainUiScreen.GetElement("exit-confirm-btn").RegisterCallback<ClickEvent>(eventObj       => { ConfirmGameQuit(); });
        mainUiScreen.GetElement("exit-cancel-btn").RegisterCallback<ClickEvent>(eventObj        => { CancelGameQuit(); });

        secondaryUiScreen.GetElement("minimize-game-btn").RegisterCallback<ClickEvent>(eventObj => { MinimizeSecondDisplay(); });
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

        showedByDefault.Add(mainUiScreen.GetElement("experiment-body"));


        foreach (var element in showedByDefault)
        {
            element.RemoveFromClassList("hidden");
        }
    }

    private void TabHasBeenClicked(VisualElement clickedTab)
    {
        if (_openTabInSecondDisplay) WinAPI.RestoreWindow("Unity Secondary Display");
        if (clickedTab == _activeTabOnMainUiScreen || clickedTab == _activeTabOnSecondaryUiScreen) return;


        if (_openTabInSecondDisplay)
        {
            if (clickedTab == mainUiScreen.GetElement("experiment-tab"))
            {
                OpenTabOnMainScreen(clickedTab);
                ShowBody(clickedTab.name, mainUiScreen);
            }
            else
            {
                OpeneTabOnSecondaryScreen(clickedTab);
                ShowBody(clickedTab.name, secondaryUiScreen);
            }
        }
        else // only one display
        {
            OpenTabOnMainScreen(clickedTab);
            ShowBody(clickedTab.name, mainUiScreen);
        }
    }

    private void OpenTabOnMainScreen(VisualElement clickedTab)
    {
        // Tab is "Active" if there is only one display
        if (_activeTabOnMainUiScreen != null) _activeTabOnMainUiScreen.RemoveFromClassList("isActive");
        _activeTabOnMainUiScreen = clickedTab;
        _activeTabOnMainUiScreen.AddToClassList("isActive");

        // todo: change classes names later
    }

    private void OpeneTabOnSecondaryScreen(VisualElement clickedTab)
    {
        // Tab is "Opened" if it is opened on second display
        if (_activeTabOnSecondaryUiScreen != null) _activeTabOnSecondaryUiScreen.RemoveFromClassList("isOpened");
        _activeTabOnSecondaryUiScreen = clickedTab;
        _activeTabOnSecondaryUiScreen.AddToClassList("isOpened");

        // todo: change classes names later
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
        clickEvent.target = secondaryUiScreen.GetElement("minimize-game-btn");
        secondaryUiScreen.GetElement("minimize-game-btn").SendEvent(clickEvent);
    }

    private void MinimizeSecondDisplay() {
        WinAPI.MinimizeGameWindow();
    }

    private void ShowExitConfirmationModalWindow()
    {
        mainUiScreen.GetElement("modal-windows").style.display = DisplayStyle.Flex;
        mainUiScreen.GetElement("exit-confirmation-modal-window").style.display = DisplayStyle.Flex;
    }

    private void CloseExitConfirmationModalWindow()
    {
        mainUiScreen.GetElement("modal-windows").style.display = DisplayStyle.None;
        mainUiScreen.GetElement("exit-confirmation-modal-window").style.display = DisplayStyle.None;
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
