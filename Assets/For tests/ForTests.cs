using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class ForTests : MonoBehaviour
{
    private ProfilerRecorder _systemMemoryRecorder;
    private ProfilerRecorder _gcMemoryRecorder;
    private ProfilerRecorder _mainThreadTimeRecorder;
    private string _statsText;

    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        
        var samples = new List<ProfilerRecorderSample>(samplesCount);
        recorder.CopyTo(samples);
        for (var i = 0; i < samplesCount; ++i)
            r += samples[i].Value;
        r /= samplesCount;

        return r;
    }

    void OnEnable()
    {
        _systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        _gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        _mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
    }

    void OnDisable()
    {
        _systemMemoryRecorder.Dispose();
        _gcMemoryRecorder.Dispose();
        _mainThreadTimeRecorder.Dispose();
    }

    public string getStats()
    {
        var sb = new StringBuilder(500);
        sb.AppendLine($"Frame Time: {GetRecorderFrameAverage(_mainThreadTimeRecorder) * (1e-6f):F1} ms");
        sb.AppendLine($"GC Memory: {_gcMemoryRecorder.LastValue / (1024 * 1024)} MB");
        sb.AppendLine($"System Memory: {_systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
        _statsText = sb.ToString();

        return _statsText;
    }
}
