using UnityEngine;
using UnityEngine.UIElements;


public class Daemons_DebugUiModuleHandler : MonoBehaviour, IFullyControllable
{
    public bool IsComponentReady {  get; private set; }

    private UiHandler _uiHandler;
    private UiReferences _uiReference;
    private DaemonsHandler _daemonsHandler;
    
    private VisualTreeAsset _daemonActivitieTemplate;
    private TextElement _numberOfDaemonsCell;
    private ScrollView _daemonsActivities;



    // BASIC FUNCTIONALITY

    public void ControllableAwake()
    {
        _daemonActivitieTemplate = Resources.Load<VisualTreeAsset>("GUI/UXML/DebugModules/DaemonsSubModule_ActivityTemplate");
    }

    public void ControllableStart()
    {
        _uiHandler = GetComponent<UiHandler>();
        _daemonsHandler = GetComponent<DaemonsHandler>();

        _uiReference = _uiHandler.secondaryUiScreen;

        _daemonsActivities = _uiReference.elements.debugTab.daemonsModule.activities;
        _daemonsActivities.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        _numberOfDaemonsCell = _uiReference.elements.debugTab.daemonsModule.totalNumber;

        IsComponentReady = true;
    }

    public void ControllableUpdate()
    {
        UpdateDaemonsModule();
    }


    // CUSTOM FUNCTIONALITY

    private void UpdateDaemonsModule()
    {
        _numberOfDaemonsCell.text = $"({_daemonsHandler.GetDaemonsNumber()})";
    }

    // todo: add color to message according to its type
    // todo: do max capacity for logs. it's VisualElements, they 'eat' a lot
    public void AddDaemonActivity(string daemonName, string messageName, MessageDirection direction, DebugMessageType messageType)
    {
        var instance = _daemonActivitieTemplate.CloneTree();

        var iconClass = $"is{direction}";   // "isIncoming" or "isOutgoing"
        var icon = instance.Q<VisualElement>(className: "debug-daemons-activity-icon");
        var name = (TextElement)instance.Q<VisualElement>(className: "debug-daemons-activity-daemon");
        var description = (TextElement)instance.Q<VisualElement>(className: "debug-daemons-activity-message");

        icon.AddToClassList(iconClass);
        name.text = daemonName;
        description.text = $" - {messageName}";

        _daemonsActivities.Add(instance);

        // note: this **** needs to be postponed because UI updates with some delay
        // Force layout update and scroll to the bottom ensuring full visibility
        _daemonsActivities.schedule.Execute(() =>
        {
            _daemonsActivities.ScrollTo(instance);
            _daemonsActivities.schedule.Execute(() =>
            {
                _daemonsActivities.verticalScroller.value = _daemonsActivities.verticalScroller.highValue > 0 ? _daemonsActivities.verticalScroller.highValue : 0;
            }).ExecuteLater(0); // Execute on the next frame to ensure layout is updated
        }).ExecuteLater(0); // Execute on the next frame
    }
}
