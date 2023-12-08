using System;

/// <summary>
/// Status of device connection. May be 'Connected', 'Disconnected' or 'InProgress'
/// </summary>
public enum DeviceConnectionStatus
{
    Connected,
    Disconnected,
    InProgress,
    NotRelevant
}

/*public class DeviceInfo
{
    public DeviceInfo(string name, DeviceConnectionStatus status)
    {
        Status = status;
        Name = name;
    }

    public DeviceConnectionStatus Status { get; set; }
    public string Name { get; set; }
}*/

/// <summary>
/// Just to know on what participant pressed
/// </summary>
public enum SignalFromParticipant
{
    Left,
    Right,
    Up,
    Down,
    Center,
    Intercom,
    Error,
    MultipleAnswer
}

/// <summary>
/// More for business logic: to know what was pressed and when
/// </summary>
public struct ParticipantAnswerStruct
{
    public SignalFromParticipant answer;
    public DateTime timestamp;

    public ParticipantAnswerStruct(SignalFromParticipant answer, DateTime timestamp)
    {
        this.answer = answer;
        this.timestamp = timestamp;
    }

    public override bool Equals(object obj)
    {
        return obj is ParticipantAnswerStruct other &&
               answer == other.answer &&
               timestamp == other.timestamp;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(answer, timestamp);
    }

    public void Deconstruct(out SignalFromParticipant answer, out DateTime timestamp)
    {
        answer = this.answer;
        timestamp = this.timestamp;
    }

    public static implicit operator (SignalFromParticipant answer, DateTime timestamp)(ParticipantAnswerStruct value)
    {
        return (value.answer, value.timestamp);
    }

    public static implicit operator ParticipantAnswerStruct((SignalFromParticipant answer, DateTime timestamp) value)
    {
        return new ParticipantAnswerStruct(value.answer, value.timestamp);
    }
}