using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UiHandler : MonoBehaviour
{
    public GameObject mainDisplayGameObject;
    public GameObject secondDisplayGameObject;

    private UiReferences _mainDisplayUiReferences;
    //private UiReferences _secondDisplayUiReferences;

    //private VisualElement _mainDisplayRoot;
    //private VisualElement _secondDisplayRoot;

    private List<VisualElement> _mainDisplayHeaderTabs;
    //private List<VisualElement> _secondDisplayHeaderTabs;

    private VisualElement _activeTab;   // main display
    private VisualElement _openedTab;   // second display

    private bool _openTabInSecondDisplay;

    void Start()
    {
        _mainDisplayUiReferences = mainDisplayGameObject.GetComponent<UiReferences>();
        //_secondDisplayUiReferences = secondDisplayGameObject.GetComponent<UiReferences>();

        /*_mainDisplayRoot = mainDisplayGameObject.GetComponent<UIDocument>().rootVisualElement;
        _secondDisplayRoot = secondDisplayGameObject.GetComponent<UIDocument>().rootVisualElement;*/

        //_mainDisplayUiReferences.SetupReferences();
        //_secondDisplayUiReferences.SetupReferences();


        _mainDisplayHeaderTabs = _mainDisplayUiReferences.GetHeaderTabs();
        //_secondDisplayHeaderTabs = _secondDisplayUiReferences.GetHeaderTabs();

        _openTabInSecondDisplay = true;

        TabHasBeenClicked(_mainDisplayUiReferences.GetElement("experiment-tab"));


        AddEventListeners();
    }

    private void AddEventListeners()
    {
        foreach (var tab in _mainDisplayHeaderTabs)
        {
            tab.pickingMode = PickingMode.Position;
            tab.RegisterCallback<ClickEvent>(eventObj => { TabHasBeenClicked((VisualElement)eventObj.currentTarget); });
        }

        /*foreach (var tab in _secondDisplayHeaderTabs)
        {
            tab.pickingMode = PickingMode.Position;
            tab.RegisterCallback<ClickEvent>(eventObj => { TabHasBeenClicked((VisualElement)eventObj.currentTarget); });
        }*/

        //_mainDisplayUiReferences.GetElement("debug-tab").RegisterCallback<ClickEvent>(eventObj => { ((VisualElement)eventObj.currentTarget).AddToClassList("isOpened"); });
        // adds class to clicked elem (currentTarget, not just target!)
    }

    private void TabHasBeenClicked(VisualElement clickedTab)
    {
        if (clickedTab == _activeTab || clickedTab == _openedTab) return;

        if (_openTabInSecondDisplay)
        {
            if (clickedTab == _mainDisplayUiReferences.GetElement("experiment-tab")) ActivateTab(clickedTab);  // main tab is exclusion
            else OpeneTab(clickedTab);
        }
        else // only one display
        {
            ActivateTab(clickedTab);
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

        // same on the second display. Change later (so shitty)
        /*var openedTabOnSecondDisplay = _secondDisplayUiReferences.GetElement(_openedTab.name);
        if (openedTabOnSecondDisplay != null) openedTabOnSecondDisplay.RemoveFromClassList("isOpened");
        Debug.Log(clickedTab.name);
        openedTabOnSecondDisplay = _secondDisplayUiReferences.GetElement(clickedTab.name);
        Debug.Log(openedTabOnSecondDisplay);
        openedTabOnSecondDisplay.AddToClassList("isOpened");*/
    }
}
