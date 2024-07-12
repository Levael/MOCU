using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;


public class DifferentInfo_DebugUiModuleHandler : MonoBehaviour
{
    private UiHandler _uiHandler;
    private UiReferences _uiReference;

    private ProfilerRecorder _systemMemoryRecorder;
    private ProfilerRecorder _gcMemoryRecorder;

    private TextElement _memoryValueCell;
    private TextElement _gcValueCell;
    private TextElement _timeWorkingValueCell;



    // BASIC FUNCTIONALITY

    private void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();

        _systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        _gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
    }

    private void Start()
    {
        _uiReference = _uiHandler.secondaryUiScreen;

        _memoryValueCell = _uiReference.elements.debugTab.diffInfoModule.memory;
        _gcValueCell = _uiReference.elements.debugTab.diffInfoModule.gc;
        _timeWorkingValueCell = _uiReference.elements.debugTab.diffInfoModule.totalTimeWorking;
    }

    private void Update()
    {
        UpdateDifferentInfoModule();
    }


    // CUSTOM FUNCTIONALITY

    private void UpdateDifferentInfoModule()
    {
        _memoryValueCell.text = $"{_systemMemoryRecorder.LastValue / (1024 * 1024)} MB";
        _gcValueCell.text = $"{_gcMemoryRecorder.LastValue / (1024 * 1024)} MB";
        _timeWorkingValueCell.text = $"{(Time.realtimeSinceStartup / 60):F0} min";
    }
}
