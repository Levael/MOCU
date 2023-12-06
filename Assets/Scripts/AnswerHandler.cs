using System;
using System.Collections.Concurrent;
using UnityEngine;

public class AnswerHandler : MonoBehaviour
{
    public ConcurrentStack<ParticipantAnswerStruct> answers;
    


    void OnEnable()
    {
        answers = new();
    }

    void Start()
    {
        
    }

    void Update()
    {
    }




    public void AddAnswer(SignalFromParticipant signalFromParticipant)
    {
        answers.Push(new ParticipantAnswerStruct(answer: signalFromParticipant, timestamp: DateTime.UtcNow));
    }
}

