using System;
using System.Linq;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class ForTests : MonoBehaviour
{
    private ProfilerRecorder _systemMemoryRecorder;
    private ProfilerRecorder _gcMemoryRecorder;
    private ProfilerRecorder _mainThreadTimeRecorder;

    private long[] _framesDurationsBuffer;
    private int _frameIndex = 0;
    private int _framesDurationsBufferSize = 15;

    private string _statsText;


    void OnEnable()
    {
        _systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        _gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        _mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
        // 1 is recorder.Capacity (no need in more than 1 because I take only the last one)
    }

    void OnDisable()
    {
        _systemMemoryRecorder.Dispose();
        _gcMemoryRecorder.Dispose();
        _mainThreadTimeRecorder.Dispose();
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





    public string getStats()
    {
        var averageFrameDuration = GetAverageFrameDuration(_mainThreadTimeRecorder);

        var sb = new StringBuilder(500);
        sb.AppendLine($"Average fps: {1000 / (averageFrameDuration * (1e-6f)):F0}");
        sb.AppendLine($"Average frame duration: {averageFrameDuration * (1e-6f):F1} ms");
        sb.AppendLine($"Last frame time: {_mainThreadTimeRecorder.LastValue * (1e-6f):F1} ms");
        sb.AppendLine($"");
        sb.AppendLine($"GC Memory: {_gcMemoryRecorder.LastValue / (1024 * 1024)} MB");
        sb.AppendLine($"System Memory: {_systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
        _statsText = sb.ToString();

        return _statsText;
    }
}
