using System;
using UnityEngine;

using MoogModule;


public class TrajectoryMakerForMoogTest : ManagedMonoBehaviour
{
    private JoystickForMoogTest _inputSourse;

    private double _maxSurgePosition = 0.259;
    private double _minSurgePosition = -0.241;
    private double _maxSwayPosition = 0.259;
    private double _minSwayPosition = -0.259;

    public event Action<DofParameters> OnPositionChanged;

    public override void ManagedAwake()
    {
        _inputSourse = GetComponent<JoystickForMoogTest>();
        _inputSourse.OnStickPositionChanged += OnStickPositionChanged;
    }

    private void OnStickPositionChanged(Vector2 stickPosition)
    {
        // 0.9 is a factor to reduce the maximum position to 90% of the maximum value
        var restrictedY = Mathf.Clamp(stickPosition.y, -1f, 1f) * 0.9;
        var restrictedX = Mathf.Clamp(stickPosition.x, -1f, 1f) * 0.9;

        var surgePos    = (float)(restrictedY * (restrictedY >= 0 ? _maxSurgePosition : _minSurgePosition));
        var swayPos     = (float)(restrictedX * (restrictedX >= 0 ? _maxSwayPosition : _minSwayPosition));

        var dofParameters = new DofParameters { Surge = surgePos, Sway = swayPos };
        OnPositionChanged?.Invoke(dofParameters);
    }

    private void OnDestroy()
    {
        if (_inputSourse != null)
            _inputSourse.OnStickPositionChanged -= OnStickPositionChanged;
    }
}