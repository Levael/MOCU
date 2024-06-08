using System;



/// <summary>
/// Just to know on what participant pressed
/// </summary>
public enum AnswerFromParticipant
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
    public AnswerFromParticipant answer;
    public DateTime timestamp;

    public ParticipantAnswerStruct(AnswerFromParticipant answer, DateTime timestamp)
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

    public void Deconstruct(out AnswerFromParticipant answer, out DateTime timestamp)
    {
        answer = this.answer;
        timestamp = this.timestamp;
    }

    public static implicit operator (AnswerFromParticipant answer, DateTime timestamp)(ParticipantAnswerStruct value)
    {
        return (value.answer, value.timestamp);
    }

    public static implicit operator ParticipantAnswerStruct((AnswerFromParticipant answer, DateTime timestamp) value)
    {
        return new ParticipantAnswerStruct(value.answer, value.timestamp);
    }
}