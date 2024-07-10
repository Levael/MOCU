using System.Linq;
using System.Text;
using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;


// todo: separate to different monobehaviours (like settings)
public class DebugTabHandler : MonoBehaviour
{
    public event Action<ClickEvent> testBtn1Clicked;
    public event Action<ClickEvent> testBtn2Clicked;

    private UiHandler _uiHandler;
    private UiReferences _uiReference;
    private DaemonsHandler _daemonsHandler;

    private ProfilerRecorder _systemMemoryRecorder;
    private ProfilerRecorder _gcMemoryRecorder;
    private ProfilerRecorder _mainThreadTimeRecorder;

    private long[] _framesDurationsBuffer;
    private int _frameIndex = 0;
    private int _framesDurationsBufferSize = 15;

    private string _statsText;

    // Diff. info
    private TextElement _memoryValueCell;
    private TextElement _gcValueCell;
    private TextElement _timeWorkingValueCell;

    // Fps
    private TextElement _currentFpsValueCell;
    private TextElement _lastFrameTimeValueCell;
    private TextElement _averageFrameTimeValueCell;

    // Console
    private TextElement _console;

    // Daemons
    private TextElement _numberOfDaemonsCell;
    private ScrollView _daemonsActivities;
    [SerializeField]
    private VisualTreeAsset _daemonActivitieTemplate;



    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        _daemonsHandler = GetComponent<DaemonsHandler>();

        _systemMemoryRecorder   = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        _gcMemoryRecorder       = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        _mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
        // 1 is recorder.Capacity (no need in more than 1 because I take only the last one)
    }

    void Start()
    {
        _uiReference = _uiHandler.secondaryUiScreen;

        _uiReference.GetElement("debug-test-btn-1").RegisterCallback<ClickEvent>(eventObj => { testBtn1Clicked?.Invoke(eventObj); });
        _uiReference.GetElement("debug-test-btn-2").RegisterCallback<ClickEvent>(eventObj => { testBtn2Clicked?.Invoke(eventObj); });

        _memoryValueCell = (TextElement)_uiReference.GetElement("debug-memory-value");
        _gcValueCell = (TextElement)_uiReference.GetElement("debug-gc-value");
        _timeWorkingValueCell = (TextElement)_uiReference.GetElement("debug-time-working-value");

        _currentFpsValueCell = (TextElement)_uiReference.GetElement("debug-current-fps-value");
        _lastFrameTimeValueCell = (TextElement)_uiReference.GetElement("debug-last-frame-value");
        _averageFrameTimeValueCell = (TextElement)_uiReference.GetElement("debug-average-frame-value");

        _console = (TextElement)_uiReference.GetElement("debug-console-module-textbox");

        _daemonsActivities = (ScrollView)_uiReference.GetElement("debug-daemons-activities");
        _daemonsActivities.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        _numberOfDaemonsCell = (TextElement)_uiReference.GetElement("number_of_running_daemons_label");
    }

    void Update()
    {
        UpdateDifferentInfoModule();
        UpdateFpsModule();
        UpdateDaemonsModule();
    }




    public void PrintToConsole(string message, bool clearTextElement = false)
    {
        if (clearTextElement) _console.text = "";
        _console.text += message;
    }

    public void AddDaemonActivity(string daemonName, string messageName, MessageDirection direction)
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




    private void UpdateDifferentInfoModule()
    {
        _memoryValueCell.text = $"{_systemMemoryRecorder.LastValue / (1024 * 1024)} MB";
        _gcValueCell.text = $"{_gcMemoryRecorder.LastValue / (1024 * 1024)} MB";
        _timeWorkingValueCell.text = $"{(Time.realtimeSinceStartup / 60):F0} min";
    }

    private void UpdateFpsModule()
    {
        var averageFrameDuration = GetAverageFrameDuration(_mainThreadTimeRecorder);

        _currentFpsValueCell.text = $"{1000 / (averageFrameDuration * (1e-6f)):F0}";
        _lastFrameTimeValueCell.text = $"{_mainThreadTimeRecorder.LastValue * (1e-6f):F1} ms";
        _averageFrameTimeValueCell.text = $"{averageFrameDuration * (1e-6f):F1} ms";
    }

    private void UpdateDaemonsModule()
    {
        _numberOfDaemonsCell.text = $"({_daemonsHandler.GetDaemonsNumber()})";
    }



    private double GetAverageFrameDuration(ProfilerRecorder profilerRecorder)                               // function should be called only once per frame. returns ns
    {
        if (_framesDurationsBuffer == null) _framesDurationsBuffer = new long[_framesDurationsBufferSize];  // first init

        _framesDurationsBuffer[_frameIndex++] = profilerRecorder.LastValue;                                 // upd last frame + increment index
        if (_frameIndex == _framesDurationsBuffer.Length) _frameIndex = 0;                                  // jump to start in case of overflow

        if (Array.Exists(_framesDurationsBuffer, element => element == 0)) return 0;                        // if not all cells are full

        return _framesDurationsBuffer.Sum() / _framesDurationsBuffer.Length;                                // arithmetic mean

        // if func is called several times per same frame -- "average frame duration" will be closer to "last frame duration"
        // if func is called not every frame -- "average frame duration" will be more "average" and not for "_framesDurationsBufferSize" last frames
    }





    /*public string getStats()
    {
        var averageFrameDuration = GetAverageFrameDuration(_mainThreadTimeRecorder);

        var sb = new StringBuilder(500);
        sb.AppendLine($"Average fps: {1000 / (averageFrameDuration * (1e-6f)):F0}");
        sb.AppendLine($"Average frame duration: {averageFrameDuration * (1e-6f):F1} ms");
        sb.AppendLine($"Last frame time: {_mainThreadTimeRecorder.LastValue * (1e-6f):F1} ms");
        sb.AppendLine($"");
        sb.AppendLine($"GC Memory: {_gcMemoryRecorder.LastValue / (1024 * 1024)} MB");
        sb.AppendLine($"System Memory: {_systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
        sb.AppendLine($"Time from start: {(Time.realtimeSinceStartup / 60):F0} min");
        _statsText = sb.ToString();

        return _statsText;
    }*/
}
