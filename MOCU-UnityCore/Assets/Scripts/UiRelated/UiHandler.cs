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

    private VisualElement _activeTab;           // main display
    private VisualElement _openedTab;           // second display

    public UiReferences mainTabScreen;          // use this (instead of _mainDisplayUiReferences)
    public UiReferences secondaryTabScreen;     // use this (instead of _secondDisplayUiReferences)

    

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


}
