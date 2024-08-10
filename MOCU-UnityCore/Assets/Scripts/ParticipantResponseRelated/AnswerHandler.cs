using System;
using System.Collections.Concurrent;
using UnityEngine;

public class AnswerHandler : MonoBehaviour, IControllableComponent
{
    public ConcurrentStack<ParticipantAnswerStruct> answers;    // make later IEnumerable for public use
    public bool IsComponentReady { get; private set; }


    public void ControllableAwake()
    {
        answers = new();
    }

    public void ControllableStart()
    {
        IsComponentReady = true;
    }




    public void AddAnswer(AnswerFromParticipant signalFromParticipant)
    {
        answers.Push(new ParticipantAnswerStruct(answer: signalFromParticipant, timestamp: DateTime.UtcNow));
    }
}

