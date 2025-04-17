using System;
using System.Collections.Concurrent;
using UnityEngine;


public class UnityMainThreadDispatcher : ManagedMonoBehaviour
{
    private static readonly ConcurrentQueue<Action> _executionQueue = new();

    public override void ManagedAwake() { }

    public override void ManagedStart()
    {
        IsComponentReady = true;
    }

    public override void ManagedUpdate()
    {
        while (_executionQueue.TryDequeue(out var action))
            action?.Invoke();
    }

    public static void Enqueue(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        _executionQueue.Enqueue(action);
    }
}