using System;
using System.Collections.Concurrent;
using UnityEngine;


public class UnityMainThreadDispatcher : MonoBehaviour, IFullyControllable
{
    private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
    public bool IsComponentReady {  get; private set; }


    public void ControllableAwake() { }

    public void ControllableStart()
    {
        IsComponentReady = true;
    }

    public void ControllableUpdate()
    {
        while (_executionQueue.TryDequeue(out var action))
            action.Invoke();
    }

    public static void Enqueue(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        _executionQueue.Enqueue(action);
    }
}
